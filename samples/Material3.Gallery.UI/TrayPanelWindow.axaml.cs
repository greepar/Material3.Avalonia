using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;

namespace Material3.Gallery.UI;

public partial class TrayPanelWindow : Window
{
    public TrayPanelWindow()
    {
        InitializeComponent();
        Opened += (_, _) => PositionNearWorkingArea();
        DarkThemeSwitch.PropertyChanged += (_, e) =>
        {
            if (e.Property == ToggleSwitch.IsCheckedProperty && Application.Current is { } app)
            {
                app.RequestedThemeVariant = DarkThemeSwitch.IsChecked == true
                    ? ThemeVariant.Dark
                    : ThemeVariant.Light;
            }
        };
    }

    public event EventHandler? ShowGalleryRequested;
    public event EventHandler? QuitRequested;

    public void SyncTheme()
    {
        DarkThemeSwitch.IsChecked = Application.Current?.ActualThemeVariant == ThemeVariant.Dark;
    }

    private void PositionNearWorkingArea()
    {
        if (Screens.Primary is not { } screen)
            return;

        var area = screen.WorkingArea;
        Position = new PixelPoint(area.Right - (int)Width - 20, area.Bottom - (int)Height - 20);
    }

    private void OnHideClick(object? sender, RoutedEventArgs e) => Hide();

    private void OnShowGalleryClick(object? sender, RoutedEventArgs e)
    {
        Hide();
        ShowGalleryRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnQuitClick(object? sender, RoutedEventArgs e) =>
        QuitRequested?.Invoke(this, EventArgs.Empty);

}
