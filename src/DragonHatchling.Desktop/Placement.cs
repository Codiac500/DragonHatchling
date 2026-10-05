namespace DragonHatchling.Desktop;

public readonly record struct WorkArea(int Left, int Top, int Right, int Bottom);

public static class Placement
{
    public static (int X, int Y) Recover(double? x, double? y, int width, int height,
        IReadOnlyList<WorkArea> areas, int primary)
    {
        var valid = x.HasValue && y.HasValue && double.IsFinite(x.Value) && double.IsFinite(y.Value)
            && Math.Abs(x.Value) <= 1_000_000 && Math.Abs(y.Value) <= 1_000_000;
        // A saved origin on a removed monitor falls back to primary; partially clipped windows clamp.
        var selected = valid ? areas.ToList().FindIndex(a => x >= a.Left && x < a.Right && y >= a.Top && y < a.Bottom) : -1;
        var area = areas[selected >= 0 ? selected : primary];
        var targetX = selected >= 0 ? x!.Value : area.Left + Math.Max(0, (area.Right - area.Left - width) / 2);
        var targetY = selected >= 0 ? y!.Value : area.Top + Math.Max(0, (area.Bottom - area.Top - height) / 2);
        return ((int)Math.Clamp(targetX, area.Left, Math.Max(area.Left, area.Right - width)),
            (int)Math.Clamp(targetY, area.Top, Math.Max(area.Top, area.Bottom - height)));
    }
}
