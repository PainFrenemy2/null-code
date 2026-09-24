using System.Windows;
using System.Windows.Media;

namespace NullMonitor.Controls;

/// <summary>Circular progress ring, 0..100, with a gradient stroke.</summary>
public sealed class RingGauge : FrameworkElement
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(double), typeof(RingGauge),
        new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty FromColorProperty = DependencyProperty.Register(
        nameof(FromColor), typeof(Color), typeof(RingGauge),
        new FrameworkPropertyMetadata(Color.FromRgb(0x7C, 0x3A, 0xED), FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ToColorProperty = DependencyProperty.Register(
        nameof(ToColor), typeof(Color), typeof(RingGauge),
        new FrameworkPropertyMetadata(Color.FromRgb(0x22, 0xD3, 0xEE), FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ThicknessProperty = DependencyProperty.Register(
        nameof(Thickness), typeof(double), typeof(RingGauge),
        new FrameworkPropertyMetadata(12d, FrameworkPropertyMetadataOptions.AffectsRender));

    public double Value { get => (double)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public Color FromColor { get => (Color)GetValue(FromColorProperty); set => SetValue(FromColorProperty, value); }
    public Color ToColor { get => (Color)GetValue(ToColorProperty); set => SetValue(ToColorProperty, value); }
    public double Thickness { get => (double)GetValue(ThicknessProperty); set => SetValue(ThicknessProperty, value); }

    private static readonly Brush Track = Freeze(new SolidColorBrush(Color.FromArgb(0x26, 0xFF, 0xFF, 0xFF)));

    private static T Freeze<T>(T f) where T : Freezable { f.Freeze(); return f; }

    protected override void OnRender(DrawingContext dc)
    {
        double size = Math.Min(ActualWidth, ActualHeight);
        if (size <= Thickness * 2) return;

        var center = new Point(ActualWidth / 2, ActualHeight / 2);
        double r = (size - Thickness) / 2;

        dc.DrawEllipse(null, new Pen(Track, Thickness), center, r, r);

        double v = Math.Clamp(Value, 0, 100);
        if (v <= 0.1) return;

        var brush = new LinearGradientBrush(FromColor, ToColor, new Point(0, 0), new Point(1, 1));
        brush.Freeze();
        var pen = new Pen(brush, Thickness) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        pen.Freeze();

        if (v >= 99.9)
        {
            dc.DrawEllipse(null, pen, center, r, r);
            return;
        }

        double angle = v / 100 * 360;
        Point PointAt(double deg)
        {
            double rad = (deg - 90) * Math.PI / 180;
            return new Point(center.X + r * Math.Cos(rad), center.Y + r * Math.Sin(rad));
        }

        var geo = new StreamGeometry();
        using (var ctx = geo.Open())
        {
            ctx.BeginFigure(PointAt(0), false, false);
            ctx.ArcTo(PointAt(angle), new Size(r, r), 0, angle > 180, SweepDirection.Clockwise, true, false);
        }
        geo.Freeze();
        dc.DrawGeometry(null, pen, geo);
    }
}
