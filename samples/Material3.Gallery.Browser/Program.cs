using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Browser;
using Material3.Gallery.UI;

[assembly: SupportedOSPlatform("browser")]

namespace Material3.Gallery.Browser;

internal static class Program
{
    public static Task Main(string[] args) => BuildAvaloniaApp()
        .WithInterFont()
        .StartBrowserAppAsync("out");

    internal static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>();
}