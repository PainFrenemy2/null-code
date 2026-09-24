using System.Windows;
using System.Windows.Threading;
using NullMonitor.Services;

namespace NullMonitor;

public partial class App : Application
{
    private Mutex? _mutex;
    private TaskbarWidget? _widget;
    private MainWindow? _dashboard;

    public HardwareService Hardware { get; } = new();
    public AppSettings Settings { get; private set; } = new();

    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += OnUnhandled;
        base.OnStartup(e);

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
    }

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

    public void Quit()
    {
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
