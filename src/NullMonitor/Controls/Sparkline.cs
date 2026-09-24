using System.Windows;
using System.Windows.Media;

namespace NullMonitor.Controls;

/// <summary>Scrolling history chart with one or two series and a gradient fill.</summary>
public sealed class Sparkline : FrameworkElement
{
    public static readonly DependencyProperty LineColorProperty = DependencyProperty.Register(
        nameof(LineColor), typeof(Color), typeof(Sparkline),
        new FrameworkPropertyMetadata(Color.FromRgb(0x22, 0xD3, 0xEE), FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty LineColor2Property = DependencyProperty.Register(
        nameof(LineColor2), typeof(Color), typeof(Sparkline),
        new FrameworkPropertyMetadata(Color.FromRgb(0xEC, 0x48, 0x99), FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Fixed maximum; 0 = auto-scale.</summary>
    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
        nameof(Maximum), typeof(double), typeof(Sparkline),
        new FrameworkPropertyMetadata(100d, FrameworkPropertyMetadataOptions.AffectsRender));

    public Color LineColor { get => (Color)GetValue(LineColorProperty); set => SetValue(LineColorProperty, value); }
    public Color LineColor2 { get => (Color)GetValue(LineColor2Property); set => SetValue(LineColor2Property, value); }
    public double Maximum { get => (double)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }

    public int Capacity { get; set; } = 60;

    private readonly List<double>[] _series = { new(), new() };

    public void Push(double a, double? b = null)
    {
        Add(_series[0], a);
        if (b.HasValue) Add(_series[1], b.Value);
        InvalidateVisual();
    }

    private void Add(List<double> list, double v)
    {
        list.Add(double.IsFinite(v) ? v : 0);
        if (list.Count > Capacity) list.RemoveAt(0);
    }

    private static readonly Pen GridPen = CreateGridPen();

    private static Pen CreateGridPen()
    {
        var p = new Pen(new SolidColorBrush(Color.FromArgb(0x14, 0xFF, 0xFF, 0xFF)), 1) { DashStyle = DashStyles.Dash };
        p.Freeze();
        return p;
    }

    protected override void OnRender(DrawingContext dc)
    {
        double w = ActualWidth, h = ActualHeight;
        if (w <= 2 || h <= 2) return;

        dc.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, w, h));
        for (int i = 1; i < 4; i++)
        {
            double y = Math.Round(h * i / 4) + 0.5;
            dc.DrawLine(GridPen, new Point(0, y), new Point(w, y));
        }

        double max = Maximum;
        if (max <= 0)
        {
            max = _series.SelectMany(s => s).DefaultIfEmpty(0).Max() * 1.15;
            if (max < 1) max = 1;
        }

        DrawSeries(dc, _series[1], LineColor2, w, h, max);
        DrawSeries(dc, _series[0], LineColor, w, h, max);
    }

    private void DrawSeries(DrawingContext dc, List<double> data, Color color, double w, double h, double max)
    {
        if (data.Count < 2) return;

        double step = w / (Capacity - 1);
        double x0 = w - step * (data.Count - 1);
        Point P(int i) => new(x0 + i * step, h - Math.Clamp(data[i] / max, 0, 1) * (h - 2) - 1);

        var line = new StreamGeometry();
        using (var ctx = line.Open())
        {
            ctx.BeginFigure(P(0), false, false);
            for (int i = 1; i < data.Count; i++) ctx.LineTo(P(i), true, true);
        }
        line.Freeze();

        var fill = new StreamGeometry();
        using (var ctx = fill.Open())
        {
            ctx.BeginFigure(new Point(x0, h), true, true);
            for (int i = 0; i < data.Count; i++) ctx.LineTo(P(i), true, true);
            ctx.LineTo(new Point(w, h), true, true);
        }
        fill.Freeze();

        var fillBrush = new LinearGradientBrush(
            Color.FromArgb(0x66, color.R, color.G, color.B),
            Color.FromArgb(0x00, color.R, color.G, color.B), 90);
        fillBrush.Freeze();
        var pen = new Pen(new SolidColorBrush(color), 2) { LineJoin = PenLineJoin.Round };
        pen.Freeze();

        dc.DrawGeometry(fillBrush, null, fill);
        dc.DrawGeometry(null, pen, line);
    }
}
