Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms

$size = 64
$bitmap = [Drawing.Bitmap]::new($size, $size, [Drawing.Imaging.PixelFormat]::Format32bppArgb)
$graphics = [Drawing.Graphics]::FromImage($bitmap)
$graphics.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.Clear([Drawing.Color]::Transparent)

$background = [Drawing.SolidBrush]::new([Drawing.Color]::FromArgb(37, 99, 235))
$graphics.FillRectangle($background, 3, 3, 58, 58)
$background.Dispose()

$white = [Drawing.SolidBrush]::new([Drawing.Color]::White)
$dark = [Drawing.SolidBrush]::new([Drawing.Color]::FromArgb(30, 41, 59))
$pointer = [Drawing.Point[]]@([Drawing.Point]::new(15, 10), [Drawing.Point]::new(15, 49), [Drawing.Point]::new(25, 40), [Drawing.Point]::new(34, 55), [Drawing.Point]::new(41, 50), [Drawing.Point]::new(31, 36), [Drawing.Point]::new(47, 36))
$graphics.FillPolygon($white, $pointer)
$graphics.DrawPolygon([Drawing.Pens]::DarkSlateGray, $pointer)
$graphics.FillEllipse($dark, 43, 12, 11, 11)
$graphics.FillEllipse($dark, 43, 27, 11, 11)
$graphics.FillEllipse($dark, 43, 42, 11, 11)
$white.Dispose(); $dark.Dispose(); $graphics.Dispose()

$handle = $bitmap.GetHicon()
$icon = [Drawing.Icon]::FromHandle($handle)
$output = Join-Path $PSScriptRoot 'app.ico'
$stream = [IO.File]::Open($output, [IO.FileMode]::Create)
$icon.Save($stream)
$stream.Dispose(); $icon.Dispose(); $bitmap.Dispose()
Write-Output $output
