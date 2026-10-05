using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DragonHatchling.Desktop;

public partial class MainWindow : Window
{
    private readonly string? diagnosticsPath;

    public MainWindow()
    {
        // Opt-in spike diagnostics only; no preferences or lifecycle state are saved.
        var args = Environment.GetCommandLineArgs();
        var index = Array.IndexOf(args, "--diagnostics");
        if (index >= 0 && index + 1 < args.Length)
            diagnosticsPath = Path.GetFullPath(args[index + 1]);

        InitializeComponent();
        Loaded += (_, _) => { CenterOnPrimaryDisplay(); Record("loaded"); };
        Activated += (_, _) => Record("activated");
        Deactivated += (_, _) => Record("deactivated");
        LocationChanged += (_, _) => Record("location");
        DpiChanged += (_, _) => Record("dpi-changed");
        Closed += (_, _) => Record("closed");
    }

    private void Placeholder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState != MouseButtonState.Pressed) return;
        e.Handled = true;
        Record("drag-start");
        // Native WPF move loop handles capture and release, with no timer or polling.
        DragMove();
        Record("drag-end");
    }

    private void Topmost_Changed(object sender, RoutedEventArgs e)
    {
        Topmost = TopmostToggle.IsChecked == true;
        Record("topmost-toggle");
    }

    private void Reset_Click(object sender, RoutedEventArgs e) => CenterOnPrimaryDisplay();
    private void Exit_Click(object sender, RoutedEventArgs e) => Close();

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Home && Keyboard.Modifiers == ModifierKeys.Control)
        {
            CenterOnPrimaryDisplay();
            e.Handled = true;
        }
    }

    private void CenterOnPrimaryDisplay()
    {
        // WPF work area and window bounds are device-independent units.
        // Cross-monitor restoration belongs to milestone 3, after mixed-DPI testing.
        var area = SystemParameters.WorkArea;
        Left = area.Left + Math.Max(0, (area.Width - Width) / 2);
        Top = area.Top + Math.Max(0, (area.Height - Height) / 2);
        Record("reset-position");
    }

    private void Record(string eventName)
    {
        if (diagnosticsPath is null) return;
        var dpi = VisualTreeHelper.GetDpi(this);
        var line = string.Create(CultureInfo.InvariantCulture,
            $"{DateTimeOffset.Now:O} {eventName} left={Left:F2} top={Top:F2} width={ActualWidth:F2} height={ActualHeight:F2} dpi={dpi.DpiScaleX:F2},{dpi.DpiScaleY:F2} active={IsActive} topmost={Topmost}\n");
        try { File.AppendAllText(diagnosticsPath, line); }
        catch (IOException) { /* Diagnostics must not disrupt desktop interaction. */ }
        catch (UnauthorizedAccessException) { }
    }
}
