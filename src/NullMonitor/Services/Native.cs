using System.Runtime.InteropServices;
using System.Text;

namespace NullMonitor.Services;

internal static class Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left, Top, Right, Bottom;
        public int Width => Right - Left;
        public int Height => Bottom - Top;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT { public int X, Y; }

    [StructLayout(LayoutKind.Sequential)]
    public struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct MONITORINFOEX
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szDevice;
    }

    public static readonly IntPtr HWND_TOPMOST = new(-1);
    public const int SW_HIDE = 0, SW_SHOWNOACTIVATE = 4;
    public const uint SWP_NOSIZE = 0x0001, SWP_NOMOVE = 0x0002, SWP_NOZORDER = 0x0004, SWP_NOACTIVATE = 0x0010, SWP_SHOWWINDOW = 0x0040;
    public const int GWL_EXSTYLE = -20;
    public const long WS_EX_TOOLWINDOW = 0x00000080, WS_EX_NOACTIVATE = 0x08000000;
    public const uint MONITOR_DEFAULTTONEAREST = 2;

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr FindWindow(string? className, string? windowName);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr FindWindowEx(IntPtr parent, IntPtr childAfter, string? className, string? windowName);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetWindowPos(IntPtr hwnd, IntPtr insertAfter, int x, int y, int cx, int cy, uint flags);

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint processId);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetClassName(IntPtr hwnd, StringBuilder buffer, int maxCount);

    [DllImport("user32.dll")]
    public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint flags);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetMonitorInfo(IntPtr monitor, ref MONITORINFO info);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetMonitorInfoW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetMonitorInfoEx(IntPtr monitor, ref MONITORINFOEX info);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetCursorPos(out POINT point);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ShowWindow(IntPtr hwnd, int cmd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWindowVisible(IntPtr hwnd);

    /// <summary>2 = busy (fullscreen app), 3 = Direct3D exclusive fullscreen, 4 = presentation mode.</summary>
    [DllImport("shell32.dll")]
    public static extern int SHQueryUserNotificationState(out int state);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    public static extern IntPtr GetWindowLongPtr(IntPtr hwnd, int index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    public static extern IntPtr SetWindowLongPtr(IntPtr hwnd, int index, IntPtr value);

    public static string ClassName(IntPtr hwnd)
    {
        var sb = new StringBuilder(256);
        GetClassName(hwnd, sb, sb.Capacity);
        return sb.ToString();
    }

    /// <summary>A Windows taskbar: the main one or one on an extra monitor.</summary>
    public sealed record Taskbar(IntPtr Hwnd, bool Primary, string Device, IntPtr Monitor, RECT Screen);

    /// <summary>All taskbars, the main one first. Extra monitors have one only when
    /// "Show taskbar on all displays" is on.</summary>
    public static List<Taskbar> Taskbars()
    {
        var list = new List<Taskbar>();
        var main = FindWindow("Shell_TrayWnd", null);
        if (main != IntPtr.Zero && Describe(main, true) is { } t) list.Add(t);

        var h = IntPtr.Zero;
        while ((h = FindWindowEx(IntPtr.Zero, h, "Shell_SecondaryTrayWnd", null)) != IntPtr.Zero)
        {
            if (IsWindowVisible(h) && Describe(h, false) is { } s) list.Add(s);
        }
        return list;
    }

    private static Taskbar? Describe(IntPtr hwnd, bool primary)
    {
        var mon = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
        var info = new MONITORINFOEX { cbSize = Marshal.SizeOf<MONITORINFOEX>(), szDevice = "" };
        if (!GetMonitorInfoEx(mon, ref info)) return null;
        return new Taskbar(hwnd, primary, info.szDevice ?? "", mon, info.rcMonitor);
    }

    /// <summary>
    /// True when a game, video or other fullscreen app is in front on the given monitor.
    /// Uses the same signal Windows uses to mute notifications, plus a window-size check
    /// for borderless fullscreen windows.
    /// </summary>
    public static bool IsFullscreenAppOn(IntPtr monitor)
    {
        var fg = GetForegroundWindow();
        if (fg == IntPtr.Zero) return false;

        GetWindowThreadProcessId(fg, out var pid);
        if (pid == Environment.ProcessId) return false;

        var cls = ClassName(fg);
        if (cls is "Progman" or "WorkerW" or "Shell_TrayWnd" or "Shell_SecondaryTrayWnd") return false;

        if (MonitorFromWindow(fg, MONITOR_DEFAULTTONEAREST) != monitor) return false;

        if (SHQueryUserNotificationState(out var state) == 0 && state is 2 or 3 or 4)
            return true;

        if (!GetWindowRect(fg, out var r)) return false;
        var info = new MONITORINFO { cbSize = Marshal.SizeOf<MONITORINFO>() };
        if (!GetMonitorInfo(monitor, ref info)) return false;

        // Small tolerance: some borderless games leave a pixel or two, others overshoot.
        const int tol = 2;
        var m = info.rcMonitor;
        return r.Left <= m.Left + tol && r.Top <= m.Top + tol && r.Right >= m.Right - tol && r.Bottom >= m.Bottom - tol;
    }
}
