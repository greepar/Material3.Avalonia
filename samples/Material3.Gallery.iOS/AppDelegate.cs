using Avalonia;
using Avalonia.iOS;
using Foundation;
using Material3.Gallery.UI;

namespace Material3.Gallery.iOS;

[Register("AppDelegate")]
public class AppDelegate : AvaloniaAppDelegate<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        var result = base.CustomizeAppBuilder(builder).WithInterFont();
#if DEBUG
        result = result.WithDeveloperTools();
#endif
        return result;
    }
}
