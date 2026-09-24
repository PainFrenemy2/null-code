using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using NullMonitor.Services;

namespace NullMonitor;

public partial class MainWindow : Window
{
    private readonly HardwareService _hw;
    private readonly DispatcherTimer _clock = new() { Interval = TimeSpan.FromSeconds(1) };
    private readonly Dictionary<string, DiskRow> _diskRows = new();
    private int _tick;

    private static readonly Brush Cool = Hex("#4ADE80");
    private static readonly Brush Warm = Hex("#FBBF24");
    private static readonly Brush HotBrush = Hex("#F87171");
    private static readonly Brush TextBrush = Hex("#EEF0F7");
    private static readonly Brush SubBrush = Hex("#8A91A8");

    public MainWindow(HardwareService hw)
    {
        _hw = hw;
        InitializeComponent();

        var v = Assembly.GetExecutingAssembly().GetName().Version;
        if (v != null) VersionText.Text = $"v{v.Major}.{v.Minor}.{v.Build}";
        Title = $"Null Monitor {VersionText.Text}";

        Action<Snapshot> onUpdate = s => Dispatcher.InvokeAsync(() => Render(s));
        Action<string> onFail = msg => Dispatcher.InvokeAsync(() => SetStatus($"Ошибка датчиков: {msg}", Warm));
        _hw.Updated += onUpdate;
        _hw.Failed += onFail;

        _clock.Tick += (_, _) => UpdateClock();
        _clock.Start();
        UpdateClock();

        StateChanged += (_, _) =>
        {
            Root.Margin = WindowState == WindowState.Maximized ? new Thickness(7) : new Thickness(0);
            MaxButton.Content = WindowState == WindowState.Maximized ? "" : "";
        };
        Closed += (_, _) =>
        {
            _hw.Updated -= onUpdate;
            _hw.Failed -= onFail;
            _clock.Stop();
        };
    }

    // ───────────────────────── rendering ─────────────────────────

    private void Render(Snapshot s)
    {
        // CPU
        CpuName.Text = s.CpuName;
        CpuGauge.Value = s.CpuLoad ?? 0;
        CpuLoadText.Text = Pct(s.CpuLoad);
        SetTemp(CpuTempText, s.CpuTemp);
        if (s.CpuTempIsApprox) CpuTempText.Text = "≈ " + CpuTempText.Text;
        CpuClockText.Text = s.CpuClock is { } cc ? $"{cc / 1000:0.00} ГГц" : "—";
        CpuPowerText.Text = s.CpuPower is { } cp ? $"{cp:0} Вт" : "—";
        CpuThreadsText.Text = Environment.ProcessorCount.ToString();
        CpuCores.SetValues(s.CoreLoads);
        CpuChart.Push(s.CpuLoad ?? 0, s.CpuTemp ?? 0);

        // GPU
        GpuName.Text = s.GpuName;
        GpuGauge.Value = s.GpuLoad ?? 0;
        GpuLoadText.Text = Pct(s.GpuLoad);
        SetTemp(GpuTempText, s.GpuTemp);
        GpuClockText.Text = s.GpuClock is { } gc ? $"{gc:0} МГц" : "—";
        GpuPowerText.Text = s.GpuPower is { } gp ? $"{gp:0} Вт" : "—";
        if (s.GpuHotSpot.HasValue)
        {
            GpuExtraLabel.Text = "Hot spot";
            SetTemp(GpuExtraText, s.GpuHotSpot);
        }
        else
        {
            GpuExtraLabel.Text = "Вентилятор";
            GpuExtraText.Foreground = TextBrush;
            GpuExtraText.Text = s.GpuFan is { } f ? $"{f:0} об/мин" : "—";
        }

        if (s.GpuMemUsed is { } mu && s.GpuMemTotal is { } mt && mt > 0)
        {
            VramText.Text = $"{mu / 1024:0.0} / {mt / 1024:0.0} ГБ";
            var host = (FrameworkElement)VramBar.Parent;
            VramBar.Width = host.ActualWidth * Math.Clamp(mu / mt, 0, 1);
        }
        else
        {
            VramText.Text = s.GpuMemUsed is { } only ? $"{only / 1024:0.0} ГБ" : "—";
            VramBar.Width = 0;
        }
        GpuChart.Push(s.GpuLoad ?? 0, s.GpuTemp ?? 0);

        // RAM
        RamGauge.Value = s.RamLoad ?? 0;
        RamLoadText.Text = Pct(s.RamLoad);
        RamUsedText.Text = s.RamUsed is { } ru ? $"{ru:0.0} ГБ" : "—";
        RamFreeText.Text = s.RamAvailable is { } ra ? $"{ra:0.0} ГБ" : "—";
        RamTotalText.Text = s is { RamUsed: { } u, RamAvailable: { } a } ? $"{u + a:0} ГБ" : "—";
        RamChart.Push(s.RamLoad ?? 0);

        // Network
        NetDownText.Text = Rate(s.NetDown);
        NetUpText.Text = Rate(s.NetUp);
        NetChart.Push(s.NetDown, s.NetUp);

        // Disks
        RenderDisks(s.Disks);

        // System
        BoardName.Text = string.IsNullOrWhiteSpace(s.BoardName) ? Environment.MachineName : s.BoardName;
        RenderFans(s.Fans);

        if (s.CpuTemp is null || s.CpuTempIsApprox)
            SetStatus("Точная температура CPU недоступна — запустите от администратора и установите драйвер PawnIO (pawnio.eu), затем перезапустите программу", Warm);
        else
            SetStatus($"Всё работает · обновление каждую секунду · {DateTime.Now:HH:mm:ss}", Cool);
    }

    private void RenderDisks(List<DiskStat> disks)
    {
        NoDisksText.Visibility = disks.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        var seen = new HashSet<string>();
        foreach (var d in disks)
        {
            var key = d.Name;
            for (int i = 2; !seen.Add(key); i++) key = $"{d.Name} #{i}";

            if (!_diskRows.TryGetValue(key, out var row))
            {
                row = new DiskRow(d.Name);
                _diskRows[key] = row;
                DiskList.Children.Add(row.Root);
            }
            row.Update(d);
        }

        foreach (var gone in _diskRows.Keys.Where(k => !seen.Contains(k)).ToList())
        {
            DiskList.Children.Remove(_diskRows[gone].Root);
            _diskRows.Remove(gone);
        }
    }

    private void RenderFans(List<FanStat> fans)
    {
        NoFansText.Visibility = fans.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        FanList.Children.Clear();
        foreach (var f in fans)
        {
            var panel = new StackPanel();
            panel.Children.Add(new TextBlock { Text = f.Name, FontSize = 11, Foreground = SubBrush });
            panel.Children.Add(new TextBlock { Text = $"{f.Rpm:0} об/мин", FontSize = 14, FontWeight = FontWeights.SemiBold });
            FanList.Children.Add(new Border
            {
                Background = Hex("#0FFFFFFF"),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(0, 0, 8, 8),
                Child = panel,
            });
        }
    }

    private void UpdateClock()
    {
        ClockText.Text = DateTime.Now.ToString("HH:mm:ss");
        var up = TimeSpan.FromMilliseconds(Environment.TickCount64);
        UptimeText.Text = up.TotalDays >= 1 ? $"{(int)up.TotalDays} д {up:hh\\:mm}" : up.ToString(@"hh\:mm\:ss");

        if (_tick++ % 3 == 0)
        {
            var procs = Process.GetProcesses();
            ProcText.Text = procs.Length.ToString();
            foreach (var p in procs) p.Dispose();
        }
    }

    private void SetStatus(string text, Brush dot)
    {
        StatusText.Text = text;
        StatusDot.Fill = dot;
    }

    // ───────────────────────── helpers ─────────────────────────

    private static Brush TempBrush(float t) => t < 60 ? Cool : t < 80 ? Warm : HotBrush;

    private static void SetTemp(TextBlock tb, float? t)
    {
        tb.Text = t is { } v ? $"{v:0} °C" : "—";
        tb.Foreground = t is { } c ? TempBrush(c) : TextBrush;
    }

    private static string Pct(float? v) => v is { } x ? $"{x:0}%" : "—";

    internal static string Rate(float? bytesPerSec)
    {
        double b = bytesPerSec ?? 0;
        if (b >= 1024 * 1024 * 1024) return $"{b / (1024d * 1024 * 1024):0.0} ГБ/с";
        if (b >= 1024 * 1024) return $"{b / (1024d * 1024):0.0} МБ/с";
        if (b >= 1024) return $"{b / 1024:0} КБ/с";
        return $"{b:0} Б/с";
    }

    internal static Brush Hex(string hex)
    {
        var b = (Brush)new BrushConverter().ConvertFromString(hex)!;
        b.Freeze();
        return b;
    }

    internal static Brush TempColor(float t) => TempBrush(t);

    // ───────────────────────── window chrome ─────────────────────────

    private void Pin_Click(object sender, RoutedEventArgs e)
    {
        Topmost = !Topmost;
        PinButton.Content = Topmost ? "" : "";
        PinButton.Foreground = Topmost ? Hex("#A78BFA") : SubBrush;
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void Maximize_Click(object sender, RoutedEventArgs e) =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        var hwnd = new WindowInteropHelper(this).Handle;
        int dark = 1, round = 2 /* DWMWCP_ROUND */;
        DwmSetWindowAttribute(hwnd, 20 /* DWMWA_USE_IMMERSIVE_DARK_MODE */, ref dark, sizeof(int));
        DwmSetWindowAttribute(hwnd, 33 /* DWMWA_WINDOW_CORNER_PREFERENCE */, ref round, sizeof(int));
    }

    // ───────────────────────── disk row ─────────────────────────

    private sealed class DiskRow
    {
        public Grid Root { get; } = new() { Margin = new Thickness(0, 10, 0, 0) };
        private readonly TextBlock _temp = new() { FontSize = 12, FontWeight = FontWeights.SemiBold };
        private readonly TextBlock _info = new() { FontSize = 11, Foreground = SubBrush, Margin = new Thickness(0, 6, 0, 0) };
        private readonly ColumnDefinition _used = new() { Width = new GridLength(0, GridUnitType.Star) };
        private readonly ColumnDefinition _free = new() { Width = new GridLength(100, GridUnitType.Star) };

        public DiskRow(string name)
        {
            Root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var head = new DockPanel();
            DockPanel.SetDock(_temp, Dock.Right);
            head.Children.Add(_temp);
            head.Children.Add(new TextBlock
            {
                Text = name, FontSize = 13, FontWeight = FontWeights.SemiBold,
                TextTrimming = TextTrimming.CharacterEllipsis, Margin = new Thickness(0, 0, 10, 0),
            });
            Root.Children.Add(head);

            var bar = new Grid { Height = 8, Margin = new Thickness(0, 6, 0, 0) };
            bar.ColumnDefinitions.Add(_used);
            bar.ColumnDefinitions.Add(_free);
            var track = new Border { Background = Hex("#1FFFFFFF"), CornerRadius = new CornerRadius(4) };
            Grid.SetColumnSpan(track, 2);
            bar.Children.Add(track);
            bar.Children.Add(new Border
            {
                CornerRadius = new CornerRadius(4),
                Background = new LinearGradientBrush(
                    (Color)ColorConverter.ConvertFromString("#7C3AED"),
                    (Color)ColorConverter.ConvertFromString("#22D3EE"), 0),
            });
            Grid.SetRow(bar, 1);
            Root.Children.Add(bar);

            Grid.SetRow(_info, 2);
            Root.Children.Add(_info);
        }

        public void Update(DiskStat d)
        {
            double used = Math.Clamp(d.UsedPercent ?? 0, 0, 100);
            _used.Width = new GridLength(used, GridUnitType.Star);
            _free.Width = new GridLength(100 - used, GridUnitType.Star);

            _temp.Text = d.Temperature is { } t ? $"{t:0} °C" : "";
            _temp.Foreground = d.Temperature is { } c ? TempColor(c) : TextBrush;

            var parts = new List<string>();
            if (d.UsedPercent is { } u) parts.Add($"занято {u:0}%");
            if (d.ReadRate.HasValue || d.WriteRate.HasValue)
                parts.Add($"чтение {Rate(d.ReadRate)} · запись {Rate(d.WriteRate)}");
            _info.Text = parts.Count > 0 ? string.Join("  ·  ", parts) : "нет данных";
        }
    }
}
