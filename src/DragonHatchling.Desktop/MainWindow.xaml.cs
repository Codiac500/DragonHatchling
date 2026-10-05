using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DragonHatchling.Desktop;

public partial class MainWindow : Window
{
    private readonly string? diagnosticsPath;
    private readonly PetController controller;
    private readonly SaveStore saveStore;
    private readonly SaveLoadResult restored;
    private readonly PetAnimator animator;
    private readonly CancellationTokenSource lifetime = new();
    private bool closing;
    private bool ready;
    private string? saveMessage;

    public MainWindow() : this(null) { }

    public MainWindow(string? savePath)
    {
        var args = Environment.GetCommandLineArgs();
        var index = Array.IndexOf(args, "--diagnostics");
        if (index >= 0 && index + 1 < args.Length)
            diagnosticsPath = Path.GetFullPath(args[index + 1]);

        index = Array.IndexOf(args, "--save-path");
        saveStore = new(savePath ?? (index >= 0 && index + 1 < args.Length ? Path.GetFullPath(args[index + 1]) : SaveStore.DefaultPath));
        restored = saveStore.Load();
        controller = new(restored.Data?.Stage ?? PetStage.Egg);
        saveMessage = restored.Message;

        InitializeComponent();
        animator = new(PetImage);
        Loaded += (_, _) =>
        {
            WindowPlacement.Restore(this, restored.Data?.X, restored.Data?.Y);
            TopmostToggle.IsChecked = restored.Data?.AlwaysOnTop ?? false;
            animator.ShowIdle(controller.State.Stage);
            ready = true;
            UpdateActions();
            Record("loaded");
        };
        Microsoft.Win32.SystemEvents.DisplaySettingsChanged += DisplaySettingsChanged;
        Activated += (_, _) => Record("activated");
        Deactivated += (_, _) => Record("deactivated");
        LocationChanged += (_, _) => Record("location");
        DpiChanged += (_, _) => Record("dpi-changed");
        StateChanged += (_, _) =>
        {
            if (controller.State.Activity == PetActivity.Idle)
                animator.ShowIdle(controller.State.Stage, WindowState != WindowState.Minimized);
            Record("window-state");
        };
        Closed += (_, _) =>
        {
            closing = true;
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= DisplaySettingsChanged;
            lifetime.Cancel();
            animator.StopMotion();
            Record("closed");
        };
    }

    private async void Hatch_Click(object sender, RoutedEventArgs e) =>
        await RunReactionAsync(PetActivity.Hatching, controller.TryHatch, animator.HatchAsync);

    private async void Feed_Click(object sender, RoutedEventArgs e) =>
        await RunReactionAsync(PetActivity.Eating, controller.TryFeed, animator.FeedAsync);

    private async void Play_Click(object sender, RoutedEventArgs e) =>
        await RunReactionAsync(PetActivity.Playing, controller.TryPlay, animator.PlayAsync);

    private async Task RunReactionAsync(PetActivity activity, Func<bool> accept,
        Func<CancellationToken, Task> animate)
    {
        if (!accept()) { Record($"{activity}-ignored"); return; }
        if (activity == PetActivity.Hatching) SaveProgress();
        UpdateActions();
        Record($"{activity}-accepted");
        try { await animate(lifetime.Token); }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested) { }
        finally
        {
            controller.CompleteReaction(activity);
            if (!closing)
            {
                animator.ShowIdle(controller.State.Stage, WindowState != WindowState.Minimized);
                UpdateActions();
                Record($"{activity}-completed");
            }
            else Record($"{activity}-cancelled");
        }
    }

    private void UpdateActions()
    {
        var state = controller.State;
        var idle = state.Activity == PetActivity.Idle;
        HatchButton.IsEnabled = idle && state.Stage == PetStage.Egg;
        FeedButton.IsEnabled = PlayButton.IsEnabled = idle && state.Stage == PetStage.Baby;
        StatusText.ToolTip = saveMessage;
        StatusText.Text = state.Activity switch
        {
            PetActivity.Hatching => "Hatching…",
            PetActivity.Eating => "Baby · Eating…",
            PetActivity.Playing => "Baby · Playing!",
            _ => saveMessage ?? (state.Stage == PetStage.Egg ? "Egg · Ready to hatch" : "Baby · Idle")
        };
    }

    private void Placeholder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState != MouseButtonState.Pressed) return;
        e.Handled = true;
        Record("drag-start");
        // Native WPF move loop handles capture and release, with no timer or polling.
        DragMove();
        var position = WindowPlacement.Capture(this);
        WindowPlacement.Restore(this, position.X, position.Y);
        SaveProgress();
        Record("drag-end");
    }

    private void Topmost_Changed(object sender, RoutedEventArgs e)
    {
        Topmost = TopmostToggle.IsChecked == true;
        Record("topmost-toggle");
        if (ready) SaveProgress();
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
        WindowPlacement.Restore(this);
        SaveProgress();
        Record("reset-position");
    }

    private void DisplaySettingsChanged(object? sender, EventArgs e) => Dispatcher.BeginInvoke(() =>
    {
        if (!ready || closing || WindowState == WindowState.Minimized) return;
        var position = WindowPlacement.Capture(this);
        WindowPlacement.Restore(this, position.X, position.Y);
        SaveProgress();
        Record("display-recovery");
    });

    private void SaveProgress()
    {
        if (!ready || closing) return;
        var position = WindowPlacement.Capture(this);
        saveMessage = saveStore.Save(new(1, controller.State.Stage, position.X, position.Y, Topmost));
        UpdateActions();
        Record(saveMessage is null ? "saved" : "save-failed");
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
