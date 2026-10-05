using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DragonHatchling.Desktop;

public partial class MainWindow : Window
{
    private readonly string? diagnosticsPath;
    private readonly PetController controller = new();
    private readonly PetAnimator animator;
    private readonly CancellationTokenSource lifetime = new();
    private bool closing;

    public MainWindow()
    {
        // Opt-in diagnostics only; persistence belongs to milestone 3.
        var args = Environment.GetCommandLineArgs();
        var index = Array.IndexOf(args, "--diagnostics");
        if (index >= 0 && index + 1 < args.Length)
            diagnosticsPath = Path.GetFullPath(args[index + 1]);

        InitializeComponent();
        animator = new(PetImage);
        Loaded += (_, _) => { CenterOnPrimaryDisplay(); Record("loaded"); };
        Activated += (_, _) => Record("activated");
        Deactivated += (_, _) => Record("deactivated");
        LocationChanged += (_, _) => Record("location");
        DpiChanged += (_, _) => Record("dpi-changed");
        Closed += (_, _) =>
        {
            closing = true;
            lifetime.Cancel();
            Record("closed");
        };
    }

    private async void Hatch_Click(object sender, RoutedEventArgs e)
    {
        if (!controller.TryHatch()) { Record("hatch-ignored"); return; }
        HatchButton.IsEnabled = false;
        StatusText.Text = "Hatching…";
        Record("hatch-accepted");
        try { await animator.HatchAsync(lifetime.Token); }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested) { }
        finally
        {
            controller.CompleteHatch();
            if (!closing)
            {
                animator.ShowIdle(controller.State.Stage);
                StatusText.Text = "Baby · Idle";
                HatchButton.Content = "Hatched";
                Record("hatch-completed");
            }
            else Record("hatch-cancelled");
        }
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
            $"{DateTimeOffset.Now:O} {eventName} left={Left:F2} top={Top:F2} width={ActualWidth:F2} height={ActualHeight:F2} dpi={dpi.DpiScaleX:F2},{dpi.DpiScaleY:F2} active={IsActive} topmost={Topmost} stage={controller.State.Stage} activity={controller.State.Activity}\n");
        try { File.AppendAllText(diagnosticsPath, line); }
        catch (IOException) { /* Diagnostics must not disrupt desktop interaction. */ }
        catch (UnauthorizedAccessException) { }
    }
}
