using System.Windows.Automation;

namespace NullMonitor.Services;

/// <summary>
/// Finds the left edge of the notification area (^ button, network, language, clock) on a taskbar.
/// On Windows 11 the tray is drawn by XAML and the old TrayNotifyWnd window can report a wrong
/// position, so UI Automation is asked for the real buttons. The scan is slow (~100 ms),
/// so it runs in the background and the result is cached.
/// </summary>
public sealed class TrayLocator
{
    private readonly object _lock = new();
    private IntPtr _hwnd;
    private int? _left;
    private DateTime _scannedAt = DateTime.MinValue;
    private bool _scanning;

    /// <summary>Cached left edge (physical px) for this taskbar, or null while unknown.</summary>
    public int? GetLeft(IntPtr taskbar)
    {
        lock (_lock)
        {
            if (taskbar != _hwnd)
            {
                _hwnd = taskbar;
                _left = null;
                _scannedAt = DateTime.MinValue;
            }

            if (!_scanning && (DateTime.UtcNow - _scannedAt).TotalSeconds > 3)
            {
                _scanning = true;
                Task.Run(() => Scan(taskbar));
            }
            return _left;
        }
    }

    private void Scan(IntPtr taskbar)
    {
        int? result = null;
        try
        {
            if (Native.GetWindowRect(taskbar, out var tb))
            {
                double left = double.MaxValue;
                var all = AutomationElement.FromHandle(taskbar)
                    .FindAll(TreeScope.Descendants, Condition.TrueCondition);
                foreach (AutomationElement el in all)
                {
                    var cls = el.Current.ClassName ?? "";
                    // Windows 11: SystemTray.NormalButton, SystemTray.OmniButton, SystemTray.ImeButton, ...
                    // Windows 10: TrayNotifyWnd / TrayClockWClass / TrayButton.
                    bool tray = cls.StartsWith("SystemTray.", StringComparison.Ordinal) ||
                                cls is "TrayNotifyWnd" or "TrayClockWClass" or "TrayShowDesktopButtonWClass";
                    if (!tray) continue;

                    var r = el.Current.BoundingRectangle;
                    if (r.IsEmpty || r.Width <= 0 || r.Height <= 0) continue;
                    // The tray always sits in the right half of a horizontal taskbar.
                    if (r.Left < tb.Left + tb.Width / 2.0 || r.Left > tb.Right) continue;
                    left = Math.Min(left, r.Left);
                }
                if (left < double.MaxValue) result = (int)left;
            }
        }
        catch
        {
            // UI Automation unavailable — the caller falls back to other methods.
        }

        lock (_lock)
        {
            if (taskbar == _hwnd)
            {
                _left = result ?? _left;
                _scannedAt = DateTime.UtcNow;
            }
            _scanning = false;
        }
    }
}
