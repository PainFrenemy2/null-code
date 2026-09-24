using System.Windows;
using System.Windows.Media;

namespace NullMonitor.Controls;

/// <summary>Per-core/thread load bars.</summary>
public sealed class CoreBars : FrameworkElement
{
    private IReadOnlyList<float> _values = Array.Empty<float>();

    private static readonly Brush Track = Frozen(new SolidColorBrush(Color.FromArgb(0x1F, 0xFF, 0xFF, 0xFF)));
    private static readonly Brush Fill = Frozen(new LinearGradientBrush(
        Color.FromRgb(0x22, 0xD3, 0xEE), Color.FromRgb(0x7C, 0x3A, 0xED), 90));
    private static readonly Brush Hot = Frozen(new LinearGradientBrush(
        Color.FromRgb(0xF9, 0x73, 0x16), Color.FromRgb(0xEF, 0x44, 0x44), 90));

    private static Brush Frozen(Brush b) { b.Freeze(); return b; }

    public void SetValues(IReadOnlyList<float> values)
    {
        _values = values;
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        int n = _values.Count;
        double w = ActualWidth, h = ActualHeight;
        if (n == 0 || w <= 0 || h <= 0) return;

        double gap = n > 24 ? 2 : 4;
        double bw = Math.Max(2, (w - gap * (n - 1)) / n);
        double radius = Math.Min(3, bw / 2);

        for (int i = 0; i < n; i++)
        {
            double x = i * (bw + gap);
            dc.DrawRoundedRectangle(Track, null, new Rect(x, 0, bw, h), radius, radius);
            double v = Math.Clamp(_values[i], 0, 100) / 100;
            double bh = Math.Max(bw > 4 ? 3 : 1, v * h);
            dc.DrawRoundedRectangle(v > 0.85 ? Hot : Fill, null, new Rect(x, h - bh, bw, bh), radius, radius);
        }
    }
}
