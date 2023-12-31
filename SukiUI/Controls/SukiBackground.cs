using System;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using SukiUI.Utilities.Background;

namespace SukiUI.Controls;

public class SukiBackground : Image, IDisposable
{
    private const int ImageWidth = 100;
    private const int ImageHeight = 100;
    private const float AnimFps = 5;

    private readonly WriteableBitmap _bmp = new(new PixelSize(ImageWidth, ImageHeight), new Vector(96, 96),
        PixelFormats.Bgra8888);

    /// <summary>
    /// Quickly and easily assign a generator either for testing, or in future allow dev-defined generators...
    /// </summary>
    private readonly ISukiBackgroundRenderer _renderer = new FastNoiseBackgroundRenderer();
    
    private static readonly Timer _animationTick = new(1000 / AnimFps) { AutoReset = true }; // 1 fps

    private bool _animationEnabled = false;

    private readonly SukiTheme _theme;

    public SukiBackground()
    {
        Source = _bmp;
        Stretch = Stretch.UniformToFill;
        _animationTick.Elapsed += (_, _) => _renderer.Render(_bmp);
        _theme = SukiTheme.GetInstance();
    }

    public override void EndInit()
    {
        base.EndInit();

        _theme.OnColorThemeChanged += theme =>
        {
            _renderer.UpdateValues(theme, Dispatcher.UIThread.Invoke(() => _theme.ActiveBaseTheme));
            _renderer.Render(_bmp);
        };
        _theme.OnBaseThemeChanged += baseTheme =>
        {
            _renderer.UpdateValues(_theme.ActiveColorTheme, baseTheme);
            _renderer.Render(_bmp);
        };

        _renderer.UpdateValues(_theme.ActiveColorTheme, _theme.ActiveBaseTheme);
        _renderer.Render(_bmp);
        
        if(_animationEnabled) _animationTick.Start();
    }

    public void SetAnimationEnabled(bool value)
    {
        if (_animationEnabled == value) return;
        _animationEnabled = value;
        if(_animationEnabled) _animationTick.Start();
        else _animationTick.Stop();
    }

    public void Dispose()
    {
        _bmp.Dispose();
    }
}