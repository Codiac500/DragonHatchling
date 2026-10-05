#if WINDOWS
using DragonHatchling.Desktop;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

internal static class WpfChecks
{
    public static void Run(int enduranceMinutes = 0)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            var directory = Path.Combine(Path.GetTempPath(), "DragonHatchling-WPF-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
                foreach (var action in new[] { "Hatch", "Feed", "Play" })
                {
                    var path = Path.Combine(directory, action + ".json");
                    var store = new SaveStore(path);
                    store.Load();
                    if (action != "Hatch") store.Save(new(1, PetStage.Baby, 100, 100, false));
                    var window = new MainWindow(path);
                    window.Show();
                    var frame = new DispatcherFrame();
                    var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
                    var phase = 0;
                    Exception? reactionFailure = null;
                    timer.Tick += (_, _) =>
                    {
                        try
                        {
                            if (phase++ == 0)
                            {
                                ((Button)window.FindName(action + "Button")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                                if (new SaveStore(path).Load().Data?.Stage != PetStage.Baby)
                                    throw new Exception("Baby must be saved before animation");
                            }
                            else if (phase == 2)
                            {
                                var status = ((TextBlock)window.FindName("StatusText")).Text;
                                if (!status.Contains(action == "Hatch" ? "Hatching" : action == "Feed" ? "Eating" : "Playing"))
                                    throw new Exception("Expected active reaction before Exit");
                                FindExit(window).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                            }
                            else
                            {
                                if (window.IsVisible) throw new Exception("Exit did not close reaction window");
                                var reopened = new MainWindow(path);
                                reopened.Show();
                                if (((TextBlock)reopened.FindName("StatusText")).Text != "Baby · Idle")
                                    throw new Exception("Reaction restart must be Baby/Idle");
                                reopened.Close();
                                timer.Stop(); frame.Continue = false;
                            }
                        }
                        catch (Exception e) { reactionFailure = e; window.Close(); timer.Stop(); frame.Continue = false; }
                    };
                    timer.Start();
                    Dispatcher.PushFrame(frame);
                    if (reactionFailure is not null) throw reactionFailure;
                }
                if (enduranceMinutes > 0) RunEndurance(directory, enduranceMinutes);
                app.Shutdown();
            }
            catch (Exception e) { failure = e; }
            finally { Directory.Delete(directory, true); }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start(); thread.Join();
        if (failure is not null) throw new Exception("WPF integration failed", failure);
        Console.WriteLine("PASS: actual WPF Exit button at ~100ms during Hatch/Feed/Play; each reopened Baby/Idle.");
    }

    private static Button FindExit(MainWindow window)
    {
        var toggle = (CheckBox)window.FindName("TopmostToggle");
        var panel = (StackPanel)toggle.Parent;
        return ((StackPanel)panel.Children[3]).Children.OfType<Button>().Single(b => Equals(b.Content, "Exit"));
    }

    // Real production windows and dispatcher/animation cleanup, without OS mouse injection.
    private static void RunEndurance(string directory, int minutes)
    {
        var path = Path.Combine(directory, "endurance.json");
        var window = new MainWindow(path);
        window.Show();
        var clock = System.Diagnostics.Stopwatch.StartNew();
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        Exception? failure = null;
        var reaction = "Hatch";
        var started = TimeSpan.Zero;
        var next = TimeSpan.Zero;
        var busy = false;
        var completed = 0;
        timer.Tick += (_, _) =>
        {
            try
            {
                var hatch = (Button)window.FindName("HatchButton");
                var feed = (Button)window.FindName("FeedButton");
                var play = (Button)window.FindName("PlayButton");
                var status = ((TextBlock)window.FindName("StatusText")).Text;
                if (busy)
                {
                    if (status == "Baby · Idle")
                    {
                        if (hatch.IsEnabled || !feed.IsEnabled || !play.IsEnabled)
                            throw new Exception("Completed reaction did not restore controls.");
                        busy = false;
                        completed++;
                        next = clock.Elapsed + TimeSpan.FromSeconds(10);
                        reaction = reaction == "Feed" ? "Play" : "Feed";
                    }
                    else if (clock.Elapsed - started > TimeSpan.FromSeconds(5))
                        throw new Exception("Reaction stuck for more than five seconds.");
                    return;
                }
                if (clock.Elapsed >= TimeSpan.FromMinutes(minutes))
                {
                    FindExit(window).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    if (window.IsVisible) throw new Exception("Exit failed after endurance session.");
                    var reopened = new MainWindow(path);
                    reopened.Show();
                    if (((TextBlock)reopened.FindName("StatusText")).Text != "Baby · Idle")
                        throw new Exception("Endurance restart did not restore Baby/Idle.");
                    reopened.Close();
                    timer.Stop(); frame.Continue = false;
                    Console.WriteLine($"PASS: {clock.Elapsed.TotalSeconds:F1}s WPF endurance, {completed} completed reactions, controls restored, Exit/restart. OS input/focus/drag not tested.");
                    return;
                }
                if (clock.Elapsed < next) return;
                ((Button)window.FindName(reaction + "Button")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                if (hatch.IsEnabled || feed.IsEnabled || play.IsEnabled)
                    throw new Exception("Interaction controls remained enabled while busy.");
                // Routed events deliberately bypass disabled controls to probe the busy guards.
                for (var i = 0; i < 10; i++)
                    foreach (var button in new[] { hatch, feed, play })
                        button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                var toggle = (CheckBox)window.FindName("TopmostToggle");
                toggle.IsChecked = toggle.IsChecked != true;
                var panel = (StackPanel)toggle.Parent;
                ((Button)((StackPanel)panel.Children[3]).Children[0]).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                busy = true; started = clock.Elapsed;
            }
            catch (Exception e)
            {
                failure = e; window.Close(); timer.Stop(); frame.Continue = false;
            }
        };
        timer.Start();
        Dispatcher.PushFrame(frame);
        if (failure is not null) throw new Exception("WPF endurance failed", failure);
    }
}
#endif
