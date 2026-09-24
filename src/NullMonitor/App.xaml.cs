using System.Windows;
using System.Windows.Threading;
using NullMonitor.Services;

namespace NullMonitor;

public partial class App : Application
{
    private Mutex? _mutex;
    private TaskbarWidget? _widget;
    private MainWindow? _dashboard;
    private readonly DispatcherTimer _updateTimer = new() { Interval = TimeSpan.FromHours(4) };
    private bool _updating;

    public HardwareService Hardware { get; } = new();
    public AppSettings Settings { get; private set; } = new();

    /// <summary>A newer release found by the last check (null = up to date).</summary>
    public UpdateInfo? PendingUpdate { get; private set; }
    public event Action? UpdateStateChanged;

    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += OnUnhandled;
        base.OnStartup(e);

        // Commands used by the installer/uninstaller: do the job and exit, no UI.
        if (e.Args.Contains("--register-autostart"))
        {
            Autostart.Enable();
            Shutdown();
            return;
        }
        if (e.Args.Contains("--unregister-autostart"))
        {
            Autostart.Disable();
            Shutdown();
            return;
        }

        _mutex = new Mutex(true, "NullMonitor.SingleInstance", out bool first);
        if (!first)
        {
            MessageBox.Show("Null Monitor уже запущен — смотри на панель задач возле часов.",
                "Null Monitor", MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        Settings = AppSettings.Load();
        Hardware.Interval = TimeSpan.FromSeconds(Settings.IntervalSeconds);
        Hardware.Start();

        // Keep the scheduled task pointing at this exe (the file name changes with each release).
        Task.Run(() =>
        {
            if (Autostart.IsEnabled()) Autostart.Enable();
        });

        _widget = new TaskbarWidget(this);
        _widget.Show();

        bool fromAutostart = e.Args.Contains("--autostart");
        if (Settings.IsFirstRun && !fromAutostart)
        {
            Settings.Save();
            ShowDashboard();
        }

        // First update check shortly after start, then every few hours.
        var firstCheck = new DispatcherTimer { Interval = TimeSpan.FromSeconds(fromAutostart ? 60 : 8) };
        firstCheck.Tick += async (_, _) =>
        {
            firstCheck.Stop();
            await CheckForUpdatesAsync(automatic: true);
        };
        firstCheck.Start();
        _updateTimer.Tick += async (_, _) => await CheckForUpdatesAsync(automatic: true);
        _updateTimer.Start();
    }

    /// <summary>
    /// Looks for a new release. In automatic mode an installed app updates itself silently;
    /// otherwise the update is only offered in the menu.
    /// </summary>
    public async Task CheckForUpdatesAsync(bool automatic)
    {
        if (_updating) return;

        var info = await Updater.CheckAsync();
        PendingUpdate = info;
        UpdateStateChanged?.Invoke();

        if (info == null)
        {
            if (!automatic)
                MessageBox.Show($"У тебя последняя версия — v{Updater.Current}.", "Null Monitor",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (automatic && Settings.AutoUpdate && Updater.IsInstalled)
            await InstallUpdateAsync();
        else if (!automatic)
        {
            var answer = MessageBox.Show(
                $"Доступна новая версия v{info.Version} (у тебя v{Updater.Current}).\n\nОбновить сейчас? Программа перезапустится сама.",
                "Null Monitor — обновление", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (answer == MessageBoxResult.Yes) await InstallUpdateAsync();
        }
    }

    public async Task InstallUpdateAsync()
    {
        if (PendingUpdate == null || _updating) return;
        _updating = true;
        UpdateStateChanged?.Invoke();

        bool keepAutostart = await Task.Run(() => Autostart.IsEnabled());
        if (await Updater.DownloadAndRunAsync(PendingUpdate, keepAutostart))
        {
            Quit();
            return;
        }

        _updating = false;
        UpdateStateChanged?.Invoke();
        MessageBox.Show("Не получилось скачать обновление. Проверь интернет или скачай вручную со страницы релизов.",
            "Null Monitor", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    public bool IsUpdating => _updating;

    public void ShowDashboard()
    {
        if (_dashboard == null)
        {
            _dashboard = new MainWindow(Hardware);
            _dashboard.Closed += (_, _) => _dashboard = null;
            _dashboard.Show();
        }
        else
        {
            if (_dashboard.WindowState == WindowState.Minimized) _dashboard.WindowState = WindowState.Normal;
            _dashboard.Activate();
        }
    }

    public void Uninstall()
    {
        var uninstaller = Updater.UninstallerPath;
        if (uninstaller == null) return;

        var answer = MessageBox.Show("Удалить Null Monitor с компьютера?\n\nАвтозапуск и настройки тоже будут удалены.",
            "Null Monitor", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (answer != MessageBoxResult.Yes) return;

        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(uninstaller) { UseShellExecute = true });
            Quit();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Null Monitor", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void Quit()
    {
        _updateTimer.Stop();
        _dashboard?.Close();
        _widget?.Close();
        Hardware.Dispose();
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Hardware.Dispose();
        _mutex?.Dispose();
        base.OnExit(e);
    }

    private static void OnUnhandled(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(e.Exception.Message, "Null Monitor", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}
