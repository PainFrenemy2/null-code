using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using NullMonitor.Services;

namespace NullMonitor;

/// <summary>
/// Small always-on-top strip that sits on the Windows taskbar next to the tray icons
/// and shows live numbers. Right click = settings, double click = full dashboard, drag = move.
/// </summary>
public partial class TaskbarWidget : Window
{
    private readonly App _app;
    private readonly DispatcherTimer _layoutTimer = new() { Interval = TimeSpan.FromMilliseconds(500) };
    private readonly Dictionary<string, TextBlock> _values = new();
    private Snapshot? _last;
    private IntPtr _hwnd;
    private ContextMenu? _menu;
    private bool? _whiteText;
    private int _themeTick;

    private bool _mouseDown, _dragging;
    private int _dragStartX, _dragStartOffset;

    private Brush _fg = Brushes.White;
    private Brush _label = Brushes.White;
    private static readonly Brush Cool = MainWindow.Hex("#4ADE80");
    private static readonly Brush CoolDark = MainWindow.Hex("#15803D");
    private static readonly Brush Warm = MainWindow.Hex("#FBBF24");
    private static readonly Brush WarmDark = MainWindow.Hex("#B45309");
    private static readonly Brush Hot = MainWindow.Hex("#F87171");
    private static readonly Brush HotDark = MainWindow.Hex("#DC2626");

    private AppSettings S => _app.Settings;

    public TaskbarWidget(App app)
    {
        _app = app;
        InitializeComponent();

        BuildLayout();

        _app.Hardware.Updated += OnSnapshot;
        Closed += (_, _) =>
        {
            _app.Hardware.Updated -= OnSnapshot;
            _layoutTimer.Stop();
        };

        _layoutTimer.Tick += (_, _) => Reposition();
        SizeChanged += (_, _) => Reposition();

        MouseEnter += (_, _) => Root.Background = new SolidColorBrush(Color.FromArgb(0x22, 0x80, 0x80, 0x80));
        MouseLeave += (_, _) => Root.Background = new SolidColorBrush(Color.FromArgb(0x01, 0, 0, 0));
        MouseLeftButtonDown += OnLeftDown;
        MouseMove += OnMouseMoved;
        MouseLeftButtonUp += OnLeftUp;
        MouseRightButtonUp += (_, e) => { OpenMenu(); e.Handled = true; };
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        _hwnd = new WindowInteropHelper(this).Handle;

        // Never steal focus, never show in Alt+Tab.
        long ex = Native.GetWindowLongPtr(_hwnd, Native.GWL_EXSTYLE).ToInt64();
        ex |= Native.WS_EX_TOOLWINDOW | Native.WS_EX_NOACTIVATE;
        Native.SetWindowLongPtr(_hwnd, Native.GWL_EXSTYLE, new IntPtr(ex));

        _layoutTimer.Start();
        Reposition();
    }

    // ───────────────────────── content ─────────────────────────

    private void BuildLayout()
    {
        ApplyTheme();
        Columns.Children.Clear();
        _values.Clear();

        var items = new List<FrameworkElement>();
        if (S.ShowCpuLoad || S.ShowCpuTemp) items.Add(Item("CPU", ("cpu", S.ShowCpuLoad, 4), ("cput", S.ShowCpuTemp, 4)));
        if (S.ShowGpuLoad || S.ShowGpuTemp) items.Add(Item("GPU", ("gpu", S.ShowGpuLoad, 4), ("gput", S.ShowGpuTemp, 4)));
        if (S.ShowNetDown) items.Add(Item("↓", ("down", true, -8)));
        if (S.ShowNetUp) items.Add(Item("↑", ("up", true, -8)));
        if (S.ShowRam) items.Add(Item("RAM", ("ram", true, 4)));

        if (items.Count == 0)
            items.Add(Item("Null Monitor", ("none", false, 0)));

        int rows = Math.Clamp(S.Rows, 1, 2);
        for (int i = 0; i < items.Count; i += rows)
        {
            var col = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 14, 0) };
            for (int j = i; j < Math.Min(i + rows, items.Count); j++) col.Children.Add(items[j]);
            Columns.Children.Add(col);
        }
        if (Columns.Children.Count > 0)
            ((StackPanel)Columns.Children[^1]).Margin = new Thickness(0);

        if (_last != null) Render(_last);
    }

    private FrameworkElement Item(string label, params (string key, bool on, int chars)[] parts)
    {
        var sp = new StackPanel { Orientation = Orientation.Horizontal };
        sp.Children.Add(new TextBlock
        {
            Text = label, FontSize = S.FontSize, Foreground = _label, Margin = new Thickness(0, 0, 5, 0),
            FontWeight = FontWeights.Normal,
        });

        foreach (var (key, on, chars) in parts)
        {
            if (!on) continue;
            var tb = new TextBlock
            {
                Text = "—",
                FontSize = S.FontSize,
                Foreground = _fg,
                // Negative width = left-aligned (network speeds sit right next to their arrow).
                MinWidth = Math.Abs(chars) * S.FontSize * 0.62,
                TextAlignment = chars < 0 ? TextAlignment.Left : TextAlignment.Right,
                Margin = new Thickness(sp.Children.Count > 1 ? 6 : 0, 0, 0, 0),
            };
            System.Windows.Documents.Typography.SetNumeralAlignment(tb, FontNumeralAlignment.Tabular);
            _values[key] = tb;
            sp.Children.Add(tb);
        }
        return sp;
    }

    private void OnSnapshot(Snapshot s) => Dispatcher.InvokeAsync(() => Render(s));

    private void Render(Snapshot s)
    {
        _last = s;
        Set("cpu", Pct(s.CpuLoad));
        SetTemp("cput", s.CpuTemp, s.CpuTempIsApprox);
        Set("gpu", Pct(s.GpuLoad));
        SetTemp("gput", s.GpuTemp);
        Set("ram", Pct(s.RamLoad));
        Set("down", MainWindow.Rate(s.NetDown));
        Set("up", MainWindow.Rate(s.NetUp));

        Root.ToolTip =
            $"{s.CpuName}\n  нагрузка {Pct(s.CpuLoad)} · {Temp(s.CpuTemp)}" +
            (s.CpuTempIsApprox ? " (примерно, датчик платы)" : "") + "\n" +
            $"{s.GpuName}\n  нагрузка {Pct(s.GpuLoad)} · {Temp(s.GpuTemp)}\n" +
            $"ОЗУ {Pct(s.RamLoad)}" + (s.RamUsed is { } u ? $" · {u:0.0} ГБ" : "") + "\n" +
            $"Сеть ↓ {MainWindow.Rate(s.NetDown)}  ↑ {MainWindow.Rate(s.NetUp)}\n\n" +
            "Двойной клик — панель · ПКМ — настройки · тяни — сдвинуть";
    }

    private void Set(string key, string text)
    {
        if (_values.TryGetValue(key, out var tb)) tb.Text = text;
    }

    private void SetTemp(string key, float? t, bool approx = false)
    {
        if (!_values.TryGetValue(key, out var tb)) return;
        tb.Text = (approx ? "~" : "") + Temp(t);
        tb.Foreground = S.ColorTemps && t is { } v ? TempBrush(v) : _fg;
    }

    private Brush TempBrush(float t)
    {
        bool light = IsLightText();
        return t < 60 ? (light ? Cool : CoolDark) : t < 80 ? (light ? Warm : WarmDark) : (light ? Hot : HotDark);
    }

    private static string Pct(float? v) => v is { } x ? $"{x:0}%" : "—";
    private static string Temp(float? v) => v is { } x ? $"{x:0}°C" : "—°C";

    // ───────────────────────── theme ─────────────────────────

    /// <summary>True = white text (dark taskbar).</summary>
    private bool IsLightText()
    {
        if (S.Theme == "light") return false;
        if (S.Theme == "dark") return true;
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            return key?.GetValue("SystemUsesLightTheme") is not int v || v == 0;
        }
        catch
        {
            return true;
        }
    }

    private void ApplyTheme()
    {
        bool white = IsLightText();
        _whiteText = white;
        _fg = white ? Brushes.White : MainWindow.Hex("#1A1A1A");
        _label = white ? MainWindow.Hex("#B8FFFFFF") : MainWindow.Hex("#A0000000");
    }

    // ───────────────────────── positioning ─────────────────────────

    private void Reposition()
    {
        if (_hwnd == IntPtr.Zero) return;

        // Follow Windows light/dark switches without a restart (checked every ~5 s).
        if (++_themeTick % 10 == 0 && S.Theme == "auto" && IsLightText() != _whiteText)
            BuildLayout();

        var taskbar = Native.FindWindow("Shell_TrayWnd", null);
        if (taskbar == IntPtr.Zero || !Native.GetWindowRect(taskbar, out var tb) || tb.Width <= 0)
        {
            Root.Opacity = 0;
            return;
        }

        bool hide = S.HideInFullscreen && Native.IsForegroundFullscreen();
        Root.Opacity = hide ? 0 : 1;
        IsHitTestVisible = !hide;

        double scale = VisualTreeHelper.GetDpi(this).DpiScaleX;
        bool horizontal = tb.Width >= tb.Height;
        int barHeight = horizontal ? tb.Height : (int)(48 * scale);

        double wantHeight = barHeight / scale;
        if (Math.Abs(Height - wantHeight) > 0.5) Height = wantHeight;

        int w = (int)Math.Ceiling(ActualWidth * scale);
        int h = (int)Math.Round(Height * scale);

        int x, y;
        if (horizontal)
        {
            y = tb.Top + (tb.Height - h) / 2;
            if (S.Anchor == "left")
            {
                x = tb.Left + (int)(12 * scale);
            }
            else
            {
                // Fallback when the tray area can't be found: roughly where the tray starts on Windows 11.
                int trayLeft = tb.Right - (int)(260 * scale);
                var notify = Native.FindWindowEx(taskbar, IntPtr.Zero, "TrayNotifyWnd", null);
                if (notify != IntPtr.Zero && Native.GetWindowRect(notify, out var nr) &&
                    nr.Width > 0 && nr.Left > tb.Left && nr.Left <= tb.Right)
                    trayLeft = nr.Left;
                x = trayLeft - w - (int)(6 * scale);
            }
            x += S.OffsetPx;
            x = Math.Clamp(x, tb.Left, Math.Max(tb.Left, tb.Right - w));
        }
        else
        {
            x = tb.Left + (tb.Width - w) / 2;
            y = tb.Bottom - h - (int)(120 * scale) + S.OffsetPx;
        }

        // While our menu is open don't jump above it, just keep the position.
        uint flags = Native.SWP_NOSIZE | Native.SWP_NOACTIVATE | Native.SWP_SHOWWINDOW;
        if (_menu?.IsOpen == true) flags |= Native.SWP_NOZORDER;
        Native.SetWindowPos(_hwnd, Native.HWND_TOPMOST, x, y, 0, 0, flags);
    }

    // ───────────────────────── mouse ─────────────────────────

    private void OnLeftDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount >= 2)
        {
            _mouseDown = false;
            _app.ShowDashboard();
            return;
        }

        Native.GetCursorPos(out var p);
        _mouseDown = true;
        _dragging = false;
        _dragStartX = p.X;
        _dragStartOffset = S.OffsetPx;
        CaptureMouse();
    }

    private void OnMouseMoved(object sender, MouseEventArgs e)
    {
        if (!_mouseDown) return;
        Native.GetCursorPos(out var p);
        int dx = p.X - _dragStartX;
        if (!_dragging && Math.Abs(dx) < 4) return;

        _dragging = true;
        Cursor = Cursors.SizeWE;
        S.OffsetPx = _dragStartOffset + dx;
        Reposition();
    }

    private void OnLeftUp(object sender, MouseButtonEventArgs e)
    {
        if (!_mouseDown) return;
        _mouseDown = false;
        ReleaseMouseCapture();
        Cursor = null;
        if (_dragging) S.Save();
        _dragging = false;
    }

    // ───────────────────────── settings menu ─────────────────────────

    private void OpenMenu()
    {
        var menu = new ContextMenu();

        if (_app.PendingUpdate is { } upd)
        {
            var update = new MenuItem
            {
                Header = _app.IsUpdating ? $"Обновляюсь до v{upd.Version}…" : $"⬆  Обновить до v{upd.Version}",
                FontWeight = FontWeights.SemiBold,
                Foreground = MainWindow.Hex("#A78BFA"),
                IsEnabled = !_app.IsUpdating,
            };
            update.Click += async (_, _) => await _app.InstallUpdateAsync();
            menu.Items.Add(update);
            menu.Items.Add(new Separator());
        }

        var open = new MenuItem { Header = "Открыть панель", FontWeight = FontWeights.SemiBold };
        open.Click += (_, _) => _app.ShowDashboard();
        menu.Items.Add(open);
        menu.Items.Add(new Separator());

        var show = new MenuItem { Header = "Показывать" };
        show.Items.Add(Toggle("Нагрузка процессора", S.ShowCpuLoad, v => S.ShowCpuLoad = v));
        show.Items.Add(Toggle("Температура процессора", S.ShowCpuTemp, v => S.ShowCpuTemp = v));
        show.Items.Add(new Separator());
        show.Items.Add(Toggle("Нагрузка видеокарты", S.ShowGpuLoad, v => S.ShowGpuLoad = v));
        show.Items.Add(Toggle("Температура видеокарты", S.ShowGpuTemp, v => S.ShowGpuTemp = v));
        show.Items.Add(new Separator());
        show.Items.Add(Toggle("Оперативная память", S.ShowRam, v => S.ShowRam = v));
        show.Items.Add(Toggle("Скорость загрузки ↓", S.ShowNetDown, v => S.ShowNetDown = v));
        show.Items.Add(Toggle("Скорость отдачи ↑", S.ShowNetUp, v => S.ShowNetUp = v));
        menu.Items.Add(show);

        var view = new MenuItem { Header = "Вид" };
        view.Items.Add(Choice("В две строки", S.Rows == 2, () => S.Rows = 2));
        view.Items.Add(Choice("В одну строку", S.Rows == 1, () => S.Rows = 1));
        view.Items.Add(new Separator());
        view.Items.Add(Choice("Мелкий шрифт", S.FontSize < 11.5, () => S.FontSize = 11));
        view.Items.Add(Choice("Обычный шрифт", S.FontSize is >= 11.5 and < 12.5, () => S.FontSize = 12));
        view.Items.Add(Choice("Крупный шрифт", S.FontSize >= 12.5, () => S.FontSize = 13));
        view.Items.Add(new Separator());
        view.Items.Add(Choice("Цвет текста: авто", S.Theme == "auto", () => S.Theme = "auto"));
        view.Items.Add(Choice("Белый текст", S.Theme == "dark", () => S.Theme = "dark"));
        view.Items.Add(Choice("Чёрный текст", S.Theme == "light", () => S.Theme = "light"));
        view.Items.Add(new Separator());
        view.Items.Add(Toggle("Цветная температура", S.ColorTemps, v => S.ColorTemps = v));
        menu.Items.Add(view);

        var rate = new MenuItem { Header = "Обновление" };
        foreach (var sec in new[] { 0.5, 1, 2, 5 })
        {
            var s = sec;
            rate.Items.Add(Choice(s < 1 ? "Каждые 0,5 с" : $"Каждые {s:0} с", Math.Abs(S.IntervalSeconds - s) < 0.01, () =>
            {
                S.IntervalSeconds = s;
                _app.Hardware.Interval = TimeSpan.FromSeconds(s);
            }));
        }
        menu.Items.Add(rate);

        var pos = new MenuItem { Header = "Положение" };
        pos.Items.Add(Choice("Возле значков трея", S.Anchor == "tray", () => { S.Anchor = "tray"; S.OffsetPx = 0; }));
        pos.Items.Add(Choice("Слева на панели задач", S.Anchor == "left", () => { S.Anchor = "left"; S.OffsetPx = 0; }));
        pos.Items.Add(new Separator());
        var reset = new MenuItem { Header = "Сбросить сдвиг" };
        reset.Click += (_, _) => { S.OffsetPx = 0; S.Save(); Reposition(); };
        pos.Items.Add(reset);
        pos.Items.Add(new MenuItem { Header = "Совет: зажми и тяни мышью", IsEnabled = false });
        menu.Items.Add(pos);

        if (_last is { CpuTemp: null } or { CpuTempIsApprox: true })
        {
            var pawn = new MenuItem { Header = "Нет точной температуры CPU → установить PawnIO" };
            pawn.Click += (_, _) =>
            {
                try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://pawnio.eu") { UseShellExecute = true }); }
                catch { /* no browser */ }
            };
            menu.Items.Add(pawn);
        }

        menu.Items.Add(Toggle("Скрывать в полноэкранных играх", S.HideInFullscreen, v => S.HideInFullscreen = v));
        menu.Items.Add(new Separator());

        var auto = new MenuItem { Header = "Запускать вместе с Windows", IsCheckable = true, IsChecked = Autostart.IsEnabled() };
        auto.Click += (_, _) =>
        {
            bool ok = auto.IsChecked ? Autostart.Enable() : Autostart.Disable();
            if (!ok)
                MessageBox.Show("Не получилось изменить автозапуск. Запусти программу от имени администратора.",
                    "Null Monitor", MessageBoxButton.OK, MessageBoxImage.Warning);
        };
        menu.Items.Add(auto);
        menu.Items.Add(new Separator());

        var about = new MenuItem { Header = $"О программе · v{Updater.Current}" };
        var check = new MenuItem { Header = "Проверить обновления", IsEnabled = !_app.IsUpdating };
        check.Click += async (_, _) => await _app.CheckForUpdatesAsync(automatic: false);
        about.Items.Add(check);
        about.Items.Add(Toggle(Updater.IsInstalled ? "Обновляться автоматически" : "Обновляться автоматически (после установки)",
            S.AutoUpdate, v => S.AutoUpdate = v));
        about.Items.Add(new Separator());
        var page = new MenuItem { Header = "Страница на GitHub" };
        page.Click += (_, _) => Updater.OpenUrl(Updater.RepoUrl);
        about.Items.Add(page);
        var releases = new MenuItem { Header = "Все версии и что нового" };
        releases.Click += (_, _) => Updater.OpenUrl(Updater.RepoUrl + "/releases");
        about.Items.Add(releases);
        if (Updater.IsInstalled)
        {
            about.Items.Add(new Separator());
            var uninstall = new MenuItem { Header = "Удалить Null Monitor…" };
            uninstall.Click += (_, _) => _app.Uninstall();
            about.Items.Add(uninstall);
        }
        menu.Items.Add(about);
        menu.Items.Add(new Separator());

        var exit = new MenuItem { Header = "Выход" };
        exit.Click += (_, _) => _app.Quit();
        menu.Items.Add(exit);

        _menu = menu;
        menu.PlacementTarget = this;
        menu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
        menu.IsOpen = true;
    }

    private MenuItem Toggle(string header, bool value, Action<bool> set)
    {
        var item = new MenuItem { Header = header, IsCheckable = true, IsChecked = value, StaysOpenOnClick = true };
        item.Click += (_, _) => { set(item.IsChecked); Apply(); };
        return item;
    }

    private MenuItem Choice(string header, bool selected, Action select)
    {
        var item = new MenuItem { Header = header, IsChecked = selected };
        item.Click += (_, _) => { select(); Apply(); };
        return item;
    }

    private void Apply()
    {
        S.Save();
        BuildLayout();
        Reposition();
    }
}
