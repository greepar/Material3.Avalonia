using Avalonia;
using Material3.Gallery.UI;

namespace Material3.Gallery;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        App.EnableDesktopTray = true;
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        var builder = AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont();

        if (OperatingSystem.IsWindows())
        {
            builder = builder.With(new Win32PlatformOptions
            {
                RenderingMode =
                [
                    Win32RenderingMode.AngleEgl,
                    Win32RenderingMode.Software,
                ],
            });
        }

        if (OperatingSystem.IsLinux())
        {
            builder = builder.With(new X11PlatformOptions
            {
                RenderingMode =
                [
                    X11RenderingMode.Glx,
                    X11RenderingMode.Software,
                ],
            });
        }

        if (OperatingSystem.IsMacOS())
        {
            builder = builder.With(new AvaloniaNativePlatformOptions
            {
                RenderingMode =
                [
                    AvaloniaNativeRenderingMode.OpenGl,
                    AvaloniaNativeRenderingMode.Software,
                ],
            });
        }

        return builder;
    }
}
