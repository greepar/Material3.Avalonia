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
        return base.CustomizeAppBuilder(builder).WithInterFont();
    }
}
