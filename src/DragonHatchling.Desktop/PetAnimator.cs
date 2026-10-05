using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DragonHatchling.Desktop;

public sealed class PetAnimator
{
    private readonly Image image;
    private readonly RotateTransform rotation = new();
    private readonly ScaleTransform scale = new(1, 1);
    private readonly BitmapImage egg = Load("egg.png");
    private readonly BitmapImage crackedEgg = Load("egg-cracked.png");
    private readonly BitmapImage baby = Load("baby.png");

    public PetAnimator(Image image)
    {
        this.image = image;
        var transforms = new TransformGroup();
        transforms.Children.Add(scale);
        transforms.Children.Add(rotation);
        image.RenderTransform = transforms;
        image.RenderTransformOrigin = new(0.5, 0.8);
        ShowIdle(PetStage.Egg);
    }

    public void ShowIdle(PetStage stage)
    {
        rotation.Angle = 0;
        scale.ScaleX = scale.ScaleY = 1;
        image.Source = stage == PetStage.Egg ? egg : baby;
    }

    // Finite UI-thread sequence, with cached frames and no permanent timer.
    public async Task HatchAsync(CancellationToken cancellation)
    {
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
        ShowIdle(PetStage.Baby);
        await Task.Delay(250, cancellation);
    }

    private static BitmapImage Load(string name)
    {
        var bitmap = new BitmapImage(new Uri($"pack://application:,,,/Assets/{name}"));
        bitmap.Freeze();
        return bitmap;
    }
}
