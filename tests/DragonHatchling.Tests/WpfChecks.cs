#if WINDOWS
using DragonHatchling.Desktop;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

internal static class WpfChecks
{
    public static void Run()
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
}
#endif
