using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Input.Platform;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Material3.Avalonia;
using Material3.Avalonia.Colors;
using Material3.Avalonia.Icons;

namespace Material3.Gallery.UI;

public partial class MainView : UserControl
{
    // Responsive breakpoint: >= 900 px behaves like the original desktop layout
    // (inline nav rail always open); below it the nav collapses into an overlay
    // pane driven by the hamburger button.
    private const double CompactWidthThreshold = 900;

    private static readonly Random s_random = new();

    private WindowNotificationManager? _notifications;
    private int _repeatCount;
    private int _spinnerValue = 5;
    private bool _isCompact;

    public MainView()
    {
        InitializeComponent();

        AutoCompleteDemo.ItemsSource = new[]
        {
            "Apple", "Apricot", "Banana", "Blueberry", "Cherry",
            "Grape", "Mango", "Orange", "Peach", "Strawberry",
        };

        SetupTable();
        BuildColorSwatches();
        IconVariantCombo.SelectedIndex = 0;
        UpdateIconItems();

        SectionsList.SelectedIndex = 0;
        VariantCombo.SelectedIndex = 0;

        SetupTimePickerDial();
    }

    protected override void OnAttachedToVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        // The notification host needs the TopLevel, which only exists once attached.
        if (TopLevel.GetTopLevel(this) is { } topLevel)
        {
            _notifications = new WindowNotificationManager(topLevel)
            {
                Position = NotificationPosition.BottomRight,
                MaxItems = 4,
            };
        }
    }

    private static new MaterialTheme? Theme =>
        global::Avalonia.Application.Current?.Styles.OfType<MaterialTheme>().FirstOrDefault();

    // ---- Responsive layout ----

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        var compact = e.NewSize.Width < CompactWidthThreshold;
        if (compact == _isCompact)
            return;

        _isCompact = compact;
        if (compact)
        {
            // Narrow / touch: overlay pane closed by default, hamburger visible,
            // Variant/Contrast hidden (implementation choice: keep only
            // Light/Dark/Random seed so the top bar fits a phone width).
            NavSplit.DisplayMode = SplitViewDisplayMode.Overlay;
            NavSplit.IsPaneOpen = false;
            MenuButton.IsVisible = true;
            VariantPanel.IsVisible = false;
            ContrastPanel.IsVisible = false;
        }
        else
        {
            // Wide: original layout — fixed inline nav rail, no hamburger.
            NavSplit.DisplayMode = SplitViewDisplayMode.Inline;
            NavSplit.IsPaneOpen = true;
            MenuButton.IsVisible = false;
            VariantPanel.IsVisible = true;
            ContrastPanel.IsVisible = true;
        }
    }

    private void OnMenuClick(object? sender, RoutedEventArgs e) =>
        NavSplit.IsPaneOpen = !NavSplit.IsPaneOpen;

    // ---- Top bar ----

    private void OnLightClick(object? sender, RoutedEventArgs e)
    {
        if (global::Avalonia.Application.Current is { } app)
            app.RequestedThemeVariant = ThemeVariant.Light;
    }

    private void OnDarkClick(object? sender, RoutedEventArgs e)
    {
        if (global::Avalonia.Application.Current is { } app)
            app.RequestedThemeVariant = ThemeVariant.Dark;
    }

    private void OnSeedClick(object? sender, RoutedEventArgs e)
    {
        if (Theme is { } theme)
        {
            theme.SeedColor = Color.FromRgb(
                (byte)s_random.Next(256),
                (byte)s_random.Next(256),
                (byte)s_random.Next(256));
        }
    }

    private void OnCircularRandomClick(object? sender, RoutedEventArgs e)
        => CircularDemo.Value = s_random.Next(0, 101);

    private void OnWavyRandomClick(object? sender, RoutedEventArgs e)
        => WavyDemo.Value = s_random.Next(0, 101);

    private void OnVariantChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (Theme is { } theme
            && VariantCombo.SelectedItem is ComboBoxItem { Content: string name }
            && Enum.TryParse<SchemeVariant>(name, out var variant))
        {
            theme.SchemeVariant = variant;
        }
    }

    private void OnContrastChanged(object? sender, RoutedEventArgs e)
    {
        if (Theme is { } theme)
            theme.ContrastLevel = ContrastSlider.Value;
    }

    // ---- Navigation ----

    private void OnSectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ContentScroll is null)
            return;

        var sections = new Control[]
        {
            M3LayoutSection, M3ComponentsSection, ButtonsSection, GroupsSection,
            SelectionSection, TextInputsSection, ComboSection, ListsSection,
            TabsSection, RangeSection, LoadingSection, MenusSection,
            DateTimeSection, TableSection, OverlaysSection,
            TypographySection, ColorsSection, MiscSection, IconsSection,
        };

        var index = SectionsList.SelectedIndex;
        for (var i = 0; i < sections.Length; i++)
            sections[i].IsVisible = i == index;

        ContentScroll.ScrollToHome();

        // On narrow screens the overlay pane auto-closes after picking a section.
        if (_isCompact)
            NavSplit.IsPaneOpen = false;
    }

    // ---- Material Symbols section ----

    private void OnIconSearchChanged(object? sender, TextChangedEventArgs e) => UpdateIconItems();

    private void OnIconVariantChanged(object? sender, SelectionChangedEventArgs e) => UpdateIconItems();

    private void UpdateIconItems()
    {
        if (IconRepeater is null || IconCountText is null)
            return;

        var query = IconSearchBox?.Text?.Trim();
        var filled = IconVariantCombo?.SelectedIndex == 1;
        var symbols = string.IsNullOrEmpty(query)
            ? MaterialSymbolCatalog.All
            : MaterialSymbolCatalog.All
                .Where(symbol => symbol.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                                 || symbol.PropertyName.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        var items = symbols.Select(symbol => new MaterialSymbolGalleryItem(symbol, filled)).ToArray();

        IconRepeater.ItemsSource = items;
        IconCountText.Text = $"{items.Length:N0} of {MaterialSymbolCatalog.All.Count:N0} icons";
    }

    private async void OnCopyIconNameClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string apiName }
            && TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard)
        {
            await clipboard.SetTextAsync(apiName);
        }
    }

    // ---- Buttons section ----

    private void OnRepeatClick(object? sender, RoutedEventArgs e)
    {
        _repeatCount++;
        RepeatDemo.Content = $"Hold me ({_repeatCount})";
    }

    private void OnSplitButtonClick(object? sender, RoutedEventArgs e)
    {
        _notifications?.Show(new Notification("SplitButton", "Primary action invoked."));
    }

    private void OnSpin(object? sender, SpinEventArgs e)
    {
        _spinnerValue += e.Direction == SpinDirection.Increase ? 1 : -1;
        SpinnerValue.Text = _spinnerValue.ToString();
    }

    // ---- Tabs section ----

    private void OnCarouselPrev(object? sender, RoutedEventArgs e) => CarouselDemo.Previous();

    private void OnCarouselNext(object? sender, RoutedEventArgs e) => CarouselDemo.Next();

    // ---- Date & time section ----

    private void SetupTimePickerDial()
    {
        Dial24h.IsCheckedChanged += (_, _) => PaneDemo.Is24Hour = Dial24h.IsChecked == true;
        PaneDemo.SelectedTimeChanged += (_, _) => UpdateDialValueText();
        UpdateDialValueText();
    }

    private void UpdateDialValueText() =>
        DialValue.Text = $"{PaneDemo.SelectedHour:00}:{PaneDemo.SelectedMinute:00}";

    // ---- Table section ----

    private record Country(string Name, string Region, int Population, int Area);

    private static global::Avalonia.Controls.Templates.FuncDataTemplate<Country> CountryCell(
        Func<Country, string> selector, TextAlignment alignment = TextAlignment.Left) =>
        new((country, _) => new TextBlock
        {
            Text = country is null ? string.Empty : selector(country),
            TextAlignment = alignment,
            VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center,
        });

    private void SetupTable()
    {
        // AOT-safe: FuncDataTemplate cell templates instead of reflection bindings.
        TableDemo.Columns = new AvaloniaList<TableViewColumn>
        {
            new()
            {
                Header = "Name",
                CellTemplate = CountryCell(c => c.Name),
                Width = new GridLength(2, GridUnitType.Star),
            },
            new()
            {
                Header = "Region",
                CellTemplate = CountryCell(c => c.Region),
                Width = new GridLength(2, GridUnitType.Star),
            },
            new()
            {
                Header = "Population",
                CellTemplate = CountryCell(c => c.Population.ToString("N0"), TextAlignment.Right),
                Width = new GridLength(1, GridUnitType.Star),
                HorizontalContentAlignment = HorizontalAlignment.Right,
            },
            new()
            {
                Header = "Area (km²)",
                CellTemplate = CountryCell(c => c.Area.ToString("N0"), TextAlignment.Right),
                Width = new GridLength(1, GridUnitType.Star),
                HorizontalContentAlignment = HorizontalAlignment.Right,
            },
        };

        TableDemo.ItemsSource = new[]
        {
            new Country("China", "Asia", 1_412_000_000, 9_596_961),
            new Country("India", "Asia", 1_408_000_000, 3_287_263),
            new Country("United States", "Americas", 333_000_000, 9_833_517),
            new Country("Indonesia", "Asia", 273_000_000, 1_904_569),
            new Country("Brazil", "Americas", 214_000_000, 8_515_767),
            new Country("Nigeria", "Africa", 213_000_000, 923_768),
            new Country("Japan", "Asia", 125_000_000, 377_975),
            new Country("Germany", "Europe", 83_000_000, 357_114),
            new Country("France", "Europe", 67_000_000, 643_801),
            new Country("Australia", "Oceania", 26_000_000, 7_692_024),
        };
    }

    // ---- Overlays section ----

    private void OnNotifyInfo(object? sender, RoutedEventArgs e) =>
        _notifications?.Show(new Notification("Information", "This is an informational message.", NotificationType.Information));

    private void OnNotifySuccess(object? sender, RoutedEventArgs e) =>
        _notifications?.Show(new Notification("Success", "The operation completed successfully.", NotificationType.Success));

    private void OnNotifyWarning(object? sender, RoutedEventArgs e) =>
        _notifications?.Show(new Notification("Warning", "Something needs your attention.", NotificationType.Warning));

    private void OnNotifyError(object? sender, RoutedEventArgs e) =>
        _notifications?.Show(new Notification("Error", "Something went wrong.", NotificationType.Error));

    private async void OnShowDialog(object? sender, RoutedEventArgs e)
    {
        // Modal dialog windows require an owner Window. On single-view platforms
        // (Android/iOS/Browser) there is no Window; implementation choice: show a
        // notification explaining the demo is desktop-only instead of an overlay
        // re-implementation.
        if (TopLevel.GetTopLevel(this) is not Window owner)
        {
            _notifications?.Show(new Notification(
                "Dialog", "Dialog windows are a desktop-only demo.", NotificationType.Information));
            return;
        }

        // Borderless M3 basic dialog: 28dp corner container on a transparent window.
        var dialog = new Window
        {
            Title = "Dialog",
            Width = 400,
            SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            WindowDecorations = global::Avalonia.Controls.WindowDecorations.None,
            TransparencyLevelHint = [WindowTransparencyLevel.Transparent],
            Background = Brushes.Transparent,
        };

        var okButton = new Button { Content = "OK" };
        okButton.Classes.Add("text");
        okButton.Click += (_, _) => dialog.Close();
        var cancelButton = new Button { Content = "Cancel" };
        cancelButton.Classes.Add("text");
        cancelButton.Click += (_, _) => dialog.Close();

        var title = new TextBlock { Text = "Dialog title" };
        title.Classes.Add("headline-small");
        var body = new TextBlock
        {
            Text = "This is a modal Material 3 basic dialog: borderless window, " +
                   "28dp corner container, elevation level 3.",
            TextWrapping = TextWrapping.Wrap,
        };
        body.Classes.Add("body-medium");
        body.Bind(TextBlock.ForegroundProperty, dialog.GetResourceObservable("Md3OnSurfaceVariantBrush"));

        var actions = new StackPanel
        {
            Orientation = global::Avalonia.Layout.Orientation.Horizontal,
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Right,
            Children = { cancelButton, okButton },
        };

        var container = new Border
        {
            CornerRadius = new global::Avalonia.CornerRadius(28),
            Margin = new global::Avalonia.Thickness(24), // buffer so the shadow is not clipped
            Child = new StackPanel
            {
                Margin = new global::Avalonia.Thickness(24),
                Spacing = 16,
                Children = { title, body, actions },
            },
        };
        container.Bind(Border.BackgroundProperty, dialog.GetResourceObservable("Md3SurfaceContainerHighBrush"));
        if (dialog.TryFindResource("Md3Elevation3", out var shadow) && shadow is BoxShadows boxShadows)
            container.BoxShadow = boxShadows;

        dialog.Content = container;

        // Allow dragging the borderless dialog by its surface.
        container.PointerPressed += (_, args) =>
        {
            if (args.GetCurrentPoint(container).Properties.IsLeftButtonPressed)
                dialog.BeginMoveDrag(args);
        };

        await dialog.ShowDialog(owner);
    }

    // ---- M3 Components / Layout sections ----

    private void OnInputChipRemoved(object? sender, EventArgs e) =>
        _notifications?.Show(new Notification("InputChip", "Chip removed."));

    private void OnOpenBottomSheet(object? sender, RoutedEventArgs e) =>
        SheetDemo.IsOpen = true;

    private void OnShowSnackbar(object? sender, RoutedEventArgs e) =>
        SnackbarHostDemo.Show("Saved", "UNDO");

    // ---- Misc section ----

    private void OnValidatedTextChanged(object? sender, TextChangedEventArgs e)
    {
        var text = ValidatedBox.Text;
        if (string.IsNullOrEmpty(text))
        {
            DataValidationErrors.ClearErrors(ValidatedBox);
        }
        else if (!int.TryParse(text, out var value) || value is < 0 or > 100)
        {
            DataValidationErrors.SetError(ValidatedBox, new DataValidationException("Enter an integer between 0 and 100."));
        }
        else
        {
            DataValidationErrors.ClearErrors(ValidatedBox);
        }
    }

    // ---- Colors section ----

    private void BuildColorSwatches()
    {
        (string Role, string Foreground)[] roles =
        {
            ("Primary", "OnPrimary"),
            ("OnPrimary", "Primary"),
            ("PrimaryContainer", "OnPrimaryContainer"),
            ("OnPrimaryContainer", "PrimaryContainer"),
            ("Secondary", "OnSecondary"),
            ("OnSecondary", "Secondary"),
            ("SecondaryContainer", "OnSecondaryContainer"),
            ("OnSecondaryContainer", "SecondaryContainer"),
            ("Tertiary", "OnTertiary"),
            ("OnTertiary", "Tertiary"),
            ("TertiaryContainer", "OnTertiaryContainer"),
            ("OnTertiaryContainer", "TertiaryContainer"),
            ("Error", "OnError"),
            ("OnError", "Error"),
            ("ErrorContainer", "OnErrorContainer"),
            ("OnErrorContainer", "ErrorContainer"),
            ("Surface", "OnSurface"),
            ("SurfaceDim", "OnSurface"),
            ("SurfaceBright", "OnSurface"),
            ("SurfaceContainerLowest", "OnSurface"),
            ("SurfaceContainerLow", "OnSurface"),
            ("SurfaceContainer", "OnSurface"),
            ("SurfaceContainerHigh", "OnSurface"),
            ("SurfaceContainerHighest", "OnSurface"),
            ("OnSurface", "Surface"),
            ("OnSurfaceVariant", "Surface"),
            ("Outline", "Surface"),
            ("OutlineVariant", "OnSurface"),
            ("InverseSurface", "InverseOnSurface"),
            ("InverseOnSurface", "InverseSurface"),
            ("InversePrimary", "OnPrimaryContainer"),
            ("SurfaceTint", "OnPrimary"),
        };

        foreach (var (role, foreground) in roles)
        {
            var label = new TextBlock
            {
                Text = role,
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new global::Avalonia.Thickness(10, 8),
            };
            label.Classes.Add("label-medium");
            label.Bind(TextBlock.ForegroundProperty, label.GetResourceObservable($"Md3{foreground}Brush"));

            var swatch = new Border
            {
                Width = 168,
                Height = 72,
                CornerRadius = new global::Avalonia.CornerRadius(8),
                Child = label,
            };
            swatch.Bind(Border.BackgroundProperty, swatch.GetResourceObservable($"Md3{role}Brush"));
            swatch.Bind(Border.BorderBrushProperty, swatch.GetResourceObservable("Md3OutlineVariantBrush"));
            swatch.BorderThickness = new global::Avalonia.Thickness(1);

            ColorsPanel.Children.Add(swatch);
        }
    }
}
