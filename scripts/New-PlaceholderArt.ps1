# Deterministic placeholder PNGs: 160x160 transparent canvas, anchor (80,140).
Add-Type -AssemblyName System.Drawing
$destination = Join-Path (Split-Path $PSScriptRoot -Parent) 'src/DragonHatchling.Desktop/Assets'
function Brush($color) { [System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml($color)) }
function Polygon($graphics, $brush, $coordinates) {
    $points = [System.Drawing.PointF[]]@()
    for ($i = 0; $i -lt $coordinates.Count; $i += 2) {
        $points += [System.Drawing.PointF]::new($coordinates[$i], $coordinates[$i + 1])
    }
    $graphics.FillPolygon($brush, $points)
}
$cream = Brush '#F7DFAB'
$spot = Brush '#DCA56D'
$green = Brush '#65C696'
$dark = Brush '#234C4A'
$light = Brush '#BDEBD3'
$wing = Brush '#9F86D9'
$white = Brush '#FFFFFF'
$outline = [System.Drawing.Pen]::new([System.Drawing.ColorTranslator]::FromHtml('#234C4A'), 4)
try {
    foreach ($name in @('egg', 'egg-cracked', 'baby', 'baby-eating', 'baby-playing')) {
        $bitmap = [System.Drawing.Bitmap]::new(160, 160)
        $g = [System.Drawing.Graphics]::FromImage($bitmap)
        try {
            $g.Clear([System.Drawing.Color]::Transparent)
            $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
            if ($name -like 'egg*') {
                $g.FillEllipse($cream, 36, 22, 88, 118)
                $g.DrawEllipse($outline, 36, 22, 88, 118)
                $g.FillEllipse($spot, 54, 56, 17, 22)
                $g.FillEllipse($spot, 88, 82, 19, 15)
                $g.FillEllipse($spot, 57, 108, 12, 12)
                if ($name -eq 'egg-cracked') {
                    $points = [System.Drawing.PointF[]]@(
                        [System.Drawing.PointF]::new(43, 70), [System.Drawing.PointF]::new(63, 82),
                        [System.Drawing.PointF]::new(74, 64), [System.Drawing.PointF]::new(87, 88),
                        [System.Drawing.PointF]::new(102, 69), [System.Drawing.PointF]::new(121, 80))
                    $g.DrawLines($outline, $points)
                }
            } else {
                Polygon $g $wing @(50, 94, 20, 62, 24, 116, 61, 113)
                Polygon $g $wing @(105, 94, 136, 62, 132, 116, 96, 113)
                Polygon $g $green @(106, 115, 149, 111, 136, 135, 99, 133)
                $g.FillEllipse($green, 49, 76, 65, 64)
                $g.FillEllipse($light, 64, 91, 35, 45)
                Polygon $g $cream @(51, 48, 52, 23, 70, 42)
                Polygon $g $cream @(94, 42, 111, 23, 111, 52)
                $g.FillEllipse($green, 42, 36, 77, 65)
                $g.DrawEllipse($outline, 42, 36, 77, 65)
                $g.FillEllipse($dark, 60, 58, 9, 15)
                $g.FillEllipse($dark, 94, 58, 9, 15)
                $g.FillEllipse($white, 62, 59, 3, 4)
                $g.FillEllipse($white, 96, 59, 3, 4)
                $g.DrawArc($outline, 70, 72, 23, 14, 0, 180)
                $g.FillEllipse($green, 43, 127, 27, 14)
                $g.FillEllipse($green, 96, 127, 27, 14)
                if ($name -eq 'baby-eating') {
                    $g.FillEllipse($dark, 72, 76, 20, 14)
                    $g.FillEllipse($spot, 66, 105, 30, 19)
                    $g.FillEllipse($cream, 70, 106, 22, 7)
                    $g.FillEllipse($green, 53, 111, 19, 12)
                    $g.FillEllipse($green, 92, 111, 19, 12)
                }
                if ($name -eq 'baby-playing') {
                    $g.FillRectangle($green, 58, 56, 14, 19)
                    $g.FillRectangle($green, 92, 56, 14, 19)
                    $g.DrawArc($outline, 59, 59, 12, 12, 180, 180)
                    $g.DrawArc($outline, 92, 59, 12, 12, 180, 180)
                    Polygon $g $cream @(22, 26, 25, 35, 34, 38, 25, 41, 22, 50, 19, 41, 10, 38, 19, 35)
                    Polygon $g $cream @(137, 40, 140, 49, 149, 52, 140, 55, 137, 64, 134, 55, 125, 52, 134, 49)
                }
            }
            $bitmap.Save((Join-Path $destination "$name.png"), [System.Drawing.Imaging.ImageFormat]::Png)
        } finally { $g.Dispose(); $bitmap.Dispose() }
    }
} finally {
    foreach ($resource in @($cream, $spot, $green, $dark, $light, $wing, $white, $outline)) { $resource.Dispose() }
}
