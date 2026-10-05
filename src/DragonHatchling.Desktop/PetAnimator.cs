using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;

namespace DragonHatchling.Desktop;

public sealed class PetAnimator
{
    private readonly Image image;
    private readonly RotateTransform rotation = new();
    private readonly ScaleTransform scale = new(1, 1);
    private readonly TranslateTransform translation = new();
    private readonly BitmapImage egg = Load("egg.png");
    private readonly BitmapImage crackedEgg = Load("egg-cracked.png");
    private readonly BitmapImage baby = Load("baby.png");
    private readonly BitmapImage eating = Load("baby-eating.png");
    private readonly BitmapImage playing = Load("baby-playing.png");

    public PetAnimator(Image image)
    {
        this.image = image;
        var transforms = new TransformGroup();
        transforms.Children.Add(scale);
        transforms.Children.Add(rotation);
        transforms.Children.Add(translation);
        image.RenderTransform = transforms;
        image.RenderTransformOrigin = new(0.5, 0.8);
        ShowIdle(PetStage.Egg);
    }

    public void ShowIdle(PetStage stage, bool animate = true)
    {
        StopMotion();
        image.Source = stage == PetStage.Egg ? egg : baby;
        if (!animate) return;
        var breathe = new DoubleAnimation(1, 1.025, TimeSpan.FromSeconds(1.4))
        {
            AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever
        };
        Timeline.SetDesiredFrameRate(breathe, 12);
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, breathe);
    }

    public void StopMotion()
    {
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
        rotation.Angle = translation.Y = 0;
        scale.ScaleX = scale.ScaleY = 1;
    }

    // Finite UI-thread sequence, with cached frames and no permanent timer.
    public async Task HatchAsync(CancellationToken cancellation)
    {
        StopMotion();
        for (var i = 0; i < 6; i++)
        {
            rotation.Angle = i % 2 == 0 ? -9 : 9;
            await Task.Delay(100, cancellation);
        }
        rotation.Angle = 0;
        image.Source = crackedEgg;
        await Task.Delay(650, cancellation);
        image.Source = baby;
        scale.ScaleX = scale.ScaleY = 0.75;
        await Task.Delay(200, cancellation);
        scale.ScaleX = scale.ScaleY = 1.08;
        await Task.Delay(200, cancellation);
        scale.ScaleX = scale.ScaleY = 1;
        await Task.Delay(250, cancellation);
    }

    public async Task FeedAsync(CancellationToken cancellation)
    {
        StopMotion();
        image.Source = eating;
        // Food and an open mouth, with small chewing squashes at the bottom anchor.
        for (var i = 0; i < 8; i++)
        {
            scale.ScaleX = i % 2 == 0 ? 1.04 : 1;
            scale.ScaleY = i % 2 == 0 ? 0.94 : 1;
            await Task.Delay(250, cancellation);
        }
    }

    public async Task PlayAsync(CancellationToken cancellation)
    {
        StopMotion();
        image.Source = playing;
        // Happy eyes and sparkles, with much larger upward hops and alternating tilt.
        for (var i = 0; i < 8; i++)
        {
            translation.Y = i % 2 == 0 ? -16 : 0;
            rotation.Angle = i % 2 == 0 ? -10 : 10;
            await Task.Delay(250, cancellation);
        }
    }

    private static BitmapImage Load(string name)
    {
        var bitmap = new BitmapImage(new Uri($"pack://application:,,,/DragonHatchling.Desktop;component/Assets/{name}"));
        bitmap.Freeze();
        return bitmap;
    }
}
