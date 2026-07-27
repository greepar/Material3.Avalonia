using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using Material3.Gallery.UI;

namespace Material3.Gallery.Android;

[Application]
public class GalleryApplication : AvaloniaAndroidApplication<App>
{
    protected GalleryApplication(nint javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer)
    {
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        var result = base.CustomizeAppBuilder(builder).WithInterFont();
#if DEBUG
        result = result.WithDeveloperTools();
#endif
        return result;
    }
}

[Activity(
    Label = "Material 3 Gallery",
    Theme = "@style/AppTheme",
    MainLauncher = true,
    Exported = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
}
