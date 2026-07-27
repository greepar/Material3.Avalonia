using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Material3.Gallery.UI;

public class App : Application
{
    private TrayIcon? _trayIcon;
    private TrayPanelWindow? _trayPanel;
    private bool _isExiting;

    public static bool EnableDesktopTray { get; set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
#if DEBUG
        this.AttachDeveloperTools();
#endif
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
        _trayPanel = new TrayPanelWindow();
        _trayPanel.ShowGalleryRequested += (_, _) => ShowMainWindow(mainWindow);
        _trayPanel.QuitRequested += (_, _) => Quit(desktop);

        var showPanelItem = new NativeMenuItem("Open Material panel");
        showPanelItem.Click += (_, _) => ShowTrayPanel();
        var showGalleryItem = new NativeMenuItem("Show gallery");
        showGalleryItem.Click += (_, _) => ShowMainWindow(mainWindow);
        var quitItem = new NativeMenuItem("Quit");
        quitItem.Click += (_, _) => Quit(desktop);
        var menu = new NativeMenu();
        menu.Items.Add(showPanelItem);
        menu.Items.Add(showGalleryItem);
        menu.Items.Add(new NativeMenuItemSeparator());
        menu.Items.Add(quitItem);

        _trayIcon = new TrayIcon
        {
            Icon = CreateTrayIcon(),
            ToolTipText = "Material 3 Gallery",
            Menu = menu,
            IsVisible = true,
        };
        _trayIcon.Clicked += (_, _) => ShowTrayPanel();
        TrayIcon.SetIcons(this, new TrayIcons { _trayIcon });

        mainWindow.Closing += (_, e) =>
        {
            if (_isExiting)
                return;
            e.Cancel = true;
            mainWindow.Hide();
            ShowTrayPanel();
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

    private void ShowTrayPanel()
    {
        if (_trayPanel is null)
            return;
        _trayPanel.SyncTheme();
        if (!_trayPanel.IsVisible)
            _trayPanel.Show();
        _trayPanel.Activate();
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
        _trayPanel?.Close();
        _trayIcon?.Dispose();
        desktop.Shutdown();
    }
}
