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
        return base.CustomizeAppBuilder(builder).WithInterFont();
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
