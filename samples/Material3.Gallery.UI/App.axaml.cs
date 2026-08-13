using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;

namespace Material3.Gallery.UI;

public class App : Application
{
    private TrayIcon? _trayIcon;
    private bool _isExiting;

    public static bool EnableDesktopTray { get; set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Desktop: host the shared MainView inside a window shell.
            var mainWindow = new MainWindow();
            desktop.MainWindow = mainWindow;
            if (EnableDesktopTray)
            {
                ConfigureTray(desktop, mainWindow);
            }
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            // Android / iOS / Browser: the shared view is the whole app surface.
            singleView.MainView = new MainView();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureTray(IClassicDesktopStyleApplicationLifetime desktop, Window mainWindow)
    {
        desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        var showGalleryItem = new NativeMenuItem("Show gallery");
        showGalleryItem.Click += (_, _) => ShowMainWindow(mainWindow);
        var darkThemeItem = new NativeMenuItem("Dark theme")
        {
            ToggleType = MenuItemToggleType.CheckBox,
            IsChecked = ActualThemeVariant == ThemeVariant.Dark,
        };
        darkThemeItem.Click += (_, _) =>
        {
            var useDarkTheme = ActualThemeVariant != ThemeVariant.Dark;
            RequestedThemeVariant = useDarkTheme ? ThemeVariant.Dark : ThemeVariant.Light;
            darkThemeItem.IsChecked = useDarkTheme;
        };
        var quitItem = new NativeMenuItem("Quit");
        quitItem.Click += (_, _) => Quit(desktop);
        var menu = new NativeMenu();
        menu.Items.Add(showGalleryItem);
        menu.Items.Add(darkThemeItem);
        menu.Items.Add(new NativeMenuItemSeparator());
        menu.Items.Add(quitItem);
        menu.NeedsUpdate += (_, _) =>
            darkThemeItem.IsChecked = ActualThemeVariant == ThemeVariant.Dark;

        _trayIcon = new TrayIcon
        {
            Icon = CreateTrayIcon(),
            ToolTipText = "Material 3 Gallery",
            Menu = menu,
            IsVisible = true,
        };
        TrayIcon.SetIcons(this, new TrayIcons { _trayIcon });

        mainWindow.Closing += (_, e) =>
        {
            if (_isExiting)
                return;
            e.Cancel = true;
            mainWindow.Hide();
        };
        desktop.Exit += (_, _) => _trayIcon?.Dispose();
    }

    private static WindowIcon CreateTrayIcon()
    {
        var bitmap = new RenderTargetBitmap(new PixelSize(32, 32));
        using var context = bitmap.CreateDrawingContext();
        context.DrawEllipse(new SolidColorBrush(Color.Parse("#6750A4")), null,
            new Point(16, 16), 15, 15);
        var white = Brushes.White;
        context.DrawEllipse(white, null, new Point(11, 16), 3, 3);
        context.DrawEllipse(white, null, new Point(16, 11), 3, 3);
        context.DrawEllipse(white, null, new Point(21, 16), 3, 3);
        context.DrawEllipse(white, null, new Point(16, 21), 3, 3);
        return new WindowIcon(bitmap);
    }

    private static void ShowMainWindow(Window mainWindow)
    {
        if (!mainWindow.IsVisible)
            mainWindow.Show();
        if (mainWindow.WindowState == WindowState.Minimized)
            mainWindow.WindowState = WindowState.Normal;
        mainWindow.Activate();
    }

    private void Quit(IClassicDesktopStyleApplicationLifetime desktop)
    {
        _isExiting = true;
        _trayIcon?.Dispose();
        desktop.Shutdown();
    }
}
