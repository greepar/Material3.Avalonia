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

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont();
}
