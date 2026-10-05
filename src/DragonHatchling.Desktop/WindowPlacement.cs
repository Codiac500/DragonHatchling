using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace DragonHatchling.Desktop;

// Native monitor bounds and window origin share physical pixels under the existing PerMonitorV2 manifest.
internal static class WindowPlacement
{
    [StructLayout(LayoutKind.Sequential)] private struct Rect { public int Left, Top, Right, Bottom; }
    [StructLayout(LayoutKind.Sequential)] private struct MonitorInfo { public int Size; public Rect Monitor, Work; public uint Flags; }
    private delegate bool MonitorCallback(nint monitor, nint dc, ref Rect bounds, nint data);
    [DllImport("user32.dll")] private static extern bool EnumDisplayMonitors(nint dc, nint clip, MonitorCallback callback, nint data);
    [DllImport("user32.dll")] private static extern bool GetMonitorInfo(nint monitor, ref MonitorInfo info);
    [DllImport("user32.dll")] private static extern bool GetWindowRect(nint window, out Rect rect);
    [DllImport("user32.dll")] private static extern bool SetWindowPos(nint window, nint after, int x, int y, int width, int height, uint flags);

    public static (int X, int Y) Capture(Window window)
    {
        if (!GetWindowRect(new WindowInteropHelper(window).Handle, out var rect))
            throw new InvalidOperationException("Window position is unavailable.");
        return (rect.Left, rect.Top);
    }

    public static void Restore(Window window, double? x = null, double? y = null)
    {
        var areas = new List<WorkArea>();
        var primary = 0;
        MonitorCallback callback = (nint monitor, nint dc, ref Rect bounds, nint data) =>
        {
            var info = new MonitorInfo { Size = Marshal.SizeOf<MonitorInfo>() };
            if (GetMonitorInfo(monitor, ref info))
            {
                if ((info.Flags & 1) != 0) primary = areas.Count;
                areas.Add(new(info.Work.Left, info.Work.Top, info.Work.Right, info.Work.Bottom));
            }
            return true;
        };
        var handle = new WindowInteropHelper(window).Handle;
        if (!EnumDisplayMonitors(0, 0, callback, 0) || areas.Count == 0 || !GetWindowRect(handle, out var rect))
            return;
        // Recheck size after moving: WPF may change the physical dimensions at a new monitor's DPI.
        for (var i = 0; i < 2; i++)
        {
            var target = Placement.Recover(x, y, rect.Right - rect.Left, rect.Bottom - rect.Top, areas, primary);
            SetWindowPos(handle, 0, target.X, target.Y, 0, 0, 0x15); // NOSIZE | NOZORDER | NOACTIVATE
            GetWindowRect(handle, out rect);
        }
    }
}
