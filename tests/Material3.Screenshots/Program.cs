using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Collections;
using System.Reflection;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Material3.Avalonia;
using Material3.Avalonia.Colors;
using Material3.Avalonia.Controls;
using Material3.Avalonia.Icons;
using Material3.Avalonia.Primitives;
using Material3.Gallery.UI;

namespace Material3.Screenshots;

public class App : Application
{
    public override void Initialize()
    {
        Styles.Add(new MaterialTheme { SeedColor = Color.FromRgb(0x67, 0x50, 0xA4) });
    }
}

public static class Program
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
            .UseSkia()
            .WithInterFont();

    public static async Task<int> Main()
    {
        using var session = HeadlessUnitTestSession.StartNew(typeof(Program));
        try
        {
            await session.Dispatch(() =>
            {
                var home = MaterialSymbols.Home;
                Assert(ReferenceEquals(home, MaterialSymbols.Home),
                    "material-symbols: repeated access should return the cached geometry");
                Assert(home.Bounds.Width > 0 && home.Bounds.Height > 0,
                    "material-symbols: regular Home geometry should not be empty");
                Assert(home.Bounds.Width <= 24 && home.Bounds.Height <= 24,
                    $"material-symbols: Home should use 24dp optical geometry, got {home.Bounds}");
                var favoriteBounds = MaterialSymbolsFilled.Favorite.Bounds;
                Assert(favoriteBounds.Width > 0 && favoriteBounds.Height > 0,
                    "material-symbols: filled Favorite geometry should not be empty");
                Assert(MaterialSymbolCatalog.All.Count == 4007,
                    $"material-symbols: expected 4007 catalog entries, got {MaterialSymbolCatalog.All.Count}");
                Assert(MaterialSymbolCatalog.All is not MaterialSymbolInfo[],
                    "material-symbols: catalog should not expose its mutable backing array");

                CheckPublicApiGuards();

                RunControlValidationChecks();

                // ---- Scenario A: wide (1280x900) — inline nav rail, no hamburger ----
                {
                    var view = new MainView();
                    var window = new Window
                    {
                        Width = 1280,
                        Height = 900,
                        Title = "Gallery wide",
                        Content = view,
                    };
                    window.Show();
                    Pump(10, 50);

                    var split = view.FindControl<SplitView>("NavSplit")
                                ?? throw new InvalidOperationException("NavSplit not found");
                    var menu = view.FindControl<Button>("MenuButton")
                               ?? throw new InvalidOperationException("MenuButton not found");

                    Capture(window, "/tmp/m3-wide.png", out var pixels);
                    Console.WriteLine(
                        $"[wide] DisplayMode={split.DisplayMode} IsPaneOpen={split.IsPaneOpen} " +
                        $"HamburgerVisible={menu.IsVisible} nonBackgroundPixels={pixels}");

                    Assert(split.DisplayMode == SplitViewDisplayMode.Inline, "wide: expected Inline display mode");
                    Assert(split.IsPaneOpen, "wide: expected nav pane open");
                    Assert(!menu.IsVisible, "wide: expected hamburger hidden");
                    Assert(pixels > 500, $"wide: screenshot looks blank ({pixels} px)");

                    // Switch to the two new M3 sections to catch runtime template errors.
                    var sectionsList = view.FindControl<ListBox>("SectionsList")
                                       ?? throw new InvalidOperationException("SectionsList not found");
                    sectionsList.SelectedIndex = 1; // Badges & chips (M3ComponentsSection)
                    Pump(6, 50);
                    Capture(window, "/tmp/m3-gallery-components.png", out var compPixels);
                    Console.WriteLine($"[gallery-m3-components] nonBackgroundPixels={compPixels}");
                    Assert(compPixels > 500, $"gallery-m3-components: looks blank ({compPixels} px)");
                    var elevatedAssist = view.FindControl<AssistChip>("ElevatedAssistChip")
                                         ?? throw new InvalidOperationException("ElevatedAssistChip not found");
                    var mainChipGroup = view.FindControl<ChipGroup>("MainChipGroup")
                                        ?? throw new InvalidOperationException("MainChipGroup not found");
                    Assert(!elevatedAssist.ClipToBounds,
                        "gallery-m3-components: elevated chip must not clip its shadow");
                    Assert(!mainChipGroup.ClipToBounds,
                        "gallery-m3-components: chip group must not clip child shadows");
                    Assert(elevatedAssist.GetVisualChildren().First() is Panel { ClipToBounds: false },
                        "gallery-m3-components: chip template root must not clip its shadow");
                    var chipCommandExecutions = 0;
                    elevatedAssist.Command = new TestCommand(() => chipCommandExecutions++);
                    elevatedAssist.Focus();
                    window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
                    window.KeyRelease(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
                    Assert(chipCommandExecutions == 1,
                        $"gallery-m3-components: Enter should execute an AssistChip command once, got {chipCommandExecutions}");

                    sectionsList.SelectedIndex = 2; // Buttons (expressive family + FAB menus)
                    Pump(8, 50);
                    Capture(window, "/tmp/m3-gallery-buttons.png", out var buttonPixels);
                    Console.WriteLine($"[gallery-buttons] nonBackgroundPixels={buttonPixels}");
                    Assert(buttonPixels > 500, $"gallery-buttons: looks blank ({buttonPixels} px)");

                    var tonalSettingsIcon = view.FindControl<PathIcon>("TonalSettingsIcon")
                                            ?? throw new InvalidOperationException("TonalSettingsIcon not found");
                    var smallFabIcon = view.FindControl<PathIcon>("SmallExpressiveFabIcon")
                                       ?? throw new InvalidOperationException("SmallExpressiveFabIcon not found");
                    Assert(tonalSettingsIcon.Bounds.Width <= 24.1 && tonalSettingsIcon.Bounds.Height <= 24.1,
                        $"gallery-buttons: icon button glyph should fit 24dp, got {tonalSettingsIcon.Bounds.Size}");
                    Assert(smallFabIcon.Bounds.Width <= 24.1 && smallFabIcon.Bounds.Height <= 24.1,
                        $"gallery-buttons: small FAB glyph should fit 24dp, got {smallFabIcon.Bounds.Size}");

                    var expressiveToggle = view.FindControl<ToggleButton>("ExpressiveSelectedButton")
                                           ?? throw new InvalidOperationException("ExpressiveSelectedButton not found");
                    var fabMenu = view.FindControl<FabMenu>("ExpressiveFabMenu")
                                  ?? throw new InvalidOperationException("ExpressiveFabMenu not found");
                    var menuItem = view.FindControl<ExtendedFloatingActionButton>("SurfaceDocumentFab")
                                   ?? throw new InvalidOperationException("SurfaceDocumentFab not found");
                    Assert(expressiveToggle.IsChecked == true,
                        "gallery-buttons: expressive toggle should start selected");
                    Assert(expressiveToggle.CornerRadius.TopLeft == 12,
                        $"gallery-buttons: selected expressive corner should be 12, got {expressiveToggle.CornerRadius}");
                    Assert(expressiveToggle.Transitions?.OfType<CornerRadiusTransition>().Any(transition =>
                               transition.Property == ToggleButton.CornerRadiusProperty) == true,
                        "gallery-buttons: expressive toggle should have a corner-radius transition");
                    expressiveToggle.IsChecked = false;
                    Pump(2, 50);
                    Assert(expressiveToggle.CornerRadius.TopLeft > 12 && expressiveToggle.CornerRadius.TopLeft <= 20,
                        $"gallery-buttons: expressive toggle should animate toward a 20dp pill, got {expressiveToggle.CornerRadius}");
                    expressiveToggle.IsChecked = true;
                    Assert(fabMenu.IsOpen, "gallery-buttons: FAB menu should start open");
                    var primaryFab = fabMenu.GetVisualDescendants().OfType<FloatingActionButton>().First();
                    Assert(primaryFab.CornerRadius.TopLeft > 16 && primaryFab.CornerRadius.TopLeft <= 28,
                        $"gallery-buttons: open FAB menu radius should animate toward 28, got {primaryFab.CornerRadius}");
                    Assert(primaryFab.Transitions?.OfType<CornerRadiusTransition>().Any(transition =>
                               transition.Property == FloatingActionButton.CornerRadiusProperty) == true,
                        "gallery-buttons: primary FAB should have a corner-radius transition");
                    Assert(menuItem.CornerRadius.TopLeft > 100,
                        $"gallery-buttons: FAB menu item should use a full corner, got {menuItem.CornerRadius}");
                    var menuAction = fabMenu.Items[0] as Button
                                     ?? throw new InvalidOperationException("FAB menu action not found");
                    menuAction.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Pump(2, 50);
                    Assert(!fabMenu.IsOpen, "gallery-buttons: clicking an action should close the FAB menu");

                    sectionsList.SelectedIndex = 0; // App bars & layout (M3LayoutSection)
                    Pump(6, 50);
                    Capture(window, "/tmp/m3-gallery-layout.png", out var layoutPixels);
                    Console.WriteLine($"[gallery-m3-layout] nonBackgroundPixels={layoutPixels}");
                    Assert(layoutPixels > 500, $"gallery-m3-layout: looks blank ({layoutPixels} px)");
                    var floatingToolbar = view.FindControl<Material3.Avalonia.Controls.Toolbar>("FloatingToolbar")
                                          ?? throw new InvalidOperationException("FloatingToolbar not found");
                    Assert(!floatingToolbar.ClipToBounds,
                        "gallery-m3-layout: floating toolbar must not clip its elevation shadow");

                    sectionsList.SelectedIndex = 7; // Lists & settings
                    Pump(4, 50);
                    var firstListItem = view.GetVisualDescendants().OfType<ListBoxItem>()
                        .First(item => item is not NavigationRailItem);
                    Assert(firstListItem.CornerRadius.TopLeft == 8,
                        $"options: ListBoxItem radius should be 8dp, got {firstListItem.CornerRadius}");
                    Assert(firstListItem.Margin == new Thickness(12, 4) && firstListItem.MinHeight == 40,
                        $"options: ListBoxItem highlight should be compact, got margin {firstListItem.Margin} and height {firstListItem.MinHeight}");
                    var firstTreeItem = view.GetVisualDescendants().OfType<TreeViewItem>().First();
                    Assert(firstTreeItem.CornerRadius.TopLeft == 8,
                        $"options: TreeViewItem radius should be 8dp, got {firstTreeItem.CornerRadius}");
                    Assert(firstTreeItem.Margin == new Thickness(12, 4),
                        $"options: TreeViewItem highlight should be inset, got margin {firstTreeItem.Margin}");

                    sectionsList.SelectedIndex = 10; // Loading & progress (LoadingSection)
                    Pump(6, 50);
                    Capture(window, "/tmp/m3-nav-loading.png", out var navLoadingPixels);
                    Console.WriteLine($"[nav-loading] nonBackgroundPixels={navLoadingPixels}");
                    Assert(navLoadingPixels > 500, $"nav-loading: looks blank ({navLoadingPixels} px)");

                    sectionsList.SelectedIndex = 3; // Button groups (GroupsSection)
                    Pump(6, 50);
                    Capture(window, "/tmp/m3-nav-groups.png", out var navGroupsPixels);
                    Console.WriteLine($"[nav-groups] nonBackgroundPixels={navGroupsPixels}");
                    Assert(navGroupsPixels > 500, $"nav-groups: looks blank ({navGroupsPixels} px)");

                    sectionsList.SelectedIndex = 4; // Selection
                    Pump(4, 50);
                    var selectionCheckBox = view.FindControl<CheckBox>("SelectionUncheckedBox")
                                            ?? throw new InvalidOperationException("SelectionUncheckedBox not found");
                    Assert(selectionCheckBox.Bounds.Width >= 359,
                        $"selection: checkbox row should expose a wide hit target, got {selectionCheckBox.Bounds.Width:F1}");
                    var selectionRight = selectionCheckBox.TranslatePoint(
                                             new Point(selectionCheckBox.Bounds.Width - 4, selectionCheckBox.Bounds.Height / 2), window)
                                         ?? throw new InvalidOperationException("Selection CheckBox TranslatePoint failed");
                    window.MouseDown(selectionRight, MouseButton.Left);
                    window.MouseUp(selectionRight, MouseButton.Left);
                    Pump(2, 50);
                    Assert(selectionCheckBox.IsChecked == true,
                        "selection: clicking the empty right side should toggle the checkbox");
                    Capture(window, "/tmp/m3-selection.png", out var selectionPixels);
                    Assert(selectionPixels > 500, $"selection: screenshot looks blank ({selectionPixels} px)");

                    sectionsList.SelectedIndex = 9; // Sliders & range (RangeSection)
                    Pump(6, 50);
                    Capture(window, "/tmp/m3-nav-range.png", out var navRangePixels);
                    Console.WriteLine($"[nav-range] nonBackgroundPixels={navRangePixels}");
                    Assert(navRangePixels > 500, $"nav-range: looks blank ({navRangePixels} px)");

                    var signedRange = view.FindControl<RangeSlider>("SignedRangeSlider")
                                       ?? throw new InvalidOperationException("SignedRangeSlider not found");
                    var rangeIndicators = signedRange.GetVisualDescendants().OfType<Border>()
                        .Where(border => border.Name is "PART_LowerIndicator" or "PART_UpperIndicator")
                        .ToArray();
                    Assert(rangeIndicators.Length == 2 && rangeIndicators.All(indicator => !indicator.IsVisible),
                        "nav-range: value indicators should be hidden when the slider is idle");
                    var lowerChanges = 0;
                    signedRange.LowerValueChanged += (_, _) => lowerChanges++;
                    signedRange.LowerValue = -32;
                    Assert(signedRange.LowerValue == -30,
                        $"nav-range: expected tick snapping to -30, got {signedRange.LowerValue}");
                    Assert(lowerChanges == 1, $"nav-range: expected one lower-value event, got {lowerChanges}");
                    signedRange.UpperValue = 120;
                    Assert(signedRange.UpperValue == 100,
                        $"nav-range: expected upper value clamped to 100, got {signedRange.UpperValue}");

                    sectionsList.SelectedIndex = 11; // Menus
                    Pump(6, 50);
                    var fileMenuItem = view.FindControl<MenuItem>("FileMenuItem")
                                       ?? throw new InvalidOperationException("FileMenuItem not found");
                    var editMenuItem = view.FindControl<MenuItem>("EditMenuItem")
                                       ?? throw new InvalidOperationException("EditMenuItem not found");
                    fileMenuItem.IsSubMenuOpen = true;
                    Pump(3, 50);
                    var fileSeparator = fileMenuItem.ContainerFromIndex(3);
                    Assert(fileSeparator?.Bounds.Height >= 25,
                        $"menus: separator should reserve vertical space, got {fileSeparator?.Bounds.Height:F1}");
                    var editCenter = editMenuItem.TranslatePoint(
                                         new Point(editMenuItem.Bounds.Width / 2, editMenuItem.Bounds.Height / 2), window)
                                     ?? throw new InvalidOperationException("Edit menu TranslatePoint failed");
                    window.MouseMove(editCenter);
                    Pump(2, 50);
                    Assert(editMenuItem.IsSubMenuOpen,
                        "menus: moving from an open File menu to Edit should switch directly to Edit");
                    Assert(!fileMenuItem.IsSubMenuOpen,
                        "menus: switching to Edit should close File");
                    Capture(window, "/tmp/m3-menu-separator.png", out var menuPixels);
                    Assert(menuPixels > 500, $"menus: screenshot looks blank ({menuPixels} px)");
                    editMenuItem.IsSubMenuOpen = false;

                    sectionsList.SelectedIndex = 12; // Date & time
                    Pump(6, 50);
                    var datePicker = view.GetVisualDescendants().OfType<DatePicker>().First();
                    var datePopup = datePicker.GetVisualDescendants().OfType<Popup>().First();
                    datePopup.IsOpen = true;
                    Pump(2, 50);
                    var dateHighlight = datePopup.Child!.GetVisualDescendants().OfType<Border>()
                        .First(border => border.Name == "HighlightRect");
                    Assert(dateHighlight.CornerRadius.TopLeft == 8,
                        $"date-picker: selected row radius should be 8dp, got {dateHighlight.CornerRadius}");
                    datePopup.IsOpen = false;

                    var timePicker = view.GetVisualDescendants().OfType<TimePicker>().First();
                    var timePopup = timePicker.GetVisualDescendants().OfType<Popup>().First();
                    timePopup.IsOpen = true;
                    Pump(2, 50);
                    var timeHighlight = timePopup.Child!.GetVisualDescendants().OfType<Border>()
                        .First(border => border.Name == "HighlightRect");
                    Assert(timeHighlight.CornerRadius.TopLeft == 8,
                        $"time-picker: selected row radius should be 8dp, got {timeHighlight.CornerRadius}");
                    timePopup.IsOpen = false;

                    var calendar = view.FindControl<Calendar>("DemoCalendar")
                                   ?? throw new InvalidOperationException("DemoCalendar not found");
                    calendar.DisplayDate = new DateTime(2026, 9, 17);
                    calendar.SelectedDate = new DateTime(2026, 9, 17);
                    Pump(6, 50);
                    var selectedDay = calendar.GetVisualDescendants().OfType<CalendarDayButton>()
                        .First(button => button.IsVisible && Equals(button.Content?.ToString(), "17"));
                    var selectedDayRoot = selectedDay.GetVisualDescendants().OfType<Border>()
                        .First(border => border.Name == "Root");
                    Assert(selectedDay.CornerRadius.TopLeft == 20,
                        $"calendar: day radius should be an exact 20dp circle, got {selectedDay.CornerRadius}");
                    Assert(!selectedDayRoot.ClipToBounds,
                        "calendar: selected day root should not use a rounded clip that aliases its edge");
                    selectedDay.Focus();
                    Pump(2, 50);
                    var dayFocusRing = selectedDay.GetVisualDescendants().OfType<Border>()
                        .First(border => border.Name == "PART_FocusRing");
                    Assert(!dayFocusRing.IsVisible,
                        "calendar: selected day should not draw a second jagged focus ring");
                    Capture(window, "/tmp/m3-calendar.png", out var calendarPixels);
                    Assert(calendarPixels > 500, $"calendar: screenshot looks blank ({calendarPixels} px)");

                    sectionsList.SelectedIndex = 13; // Table
                    Pump(6, 50);
                    var table = view.FindControl<TableView>("TableDemo")
                                ?? throw new InvalidOperationException("TableDemo not found");
                    var firstHeader = table.GetVisualDescendants().OfType<TableViewColumnHeader>().First();
                    var headerStateLayer = firstHeader.GetVisualDescendants().OfType<Border>()
                        .First(border => border.Name == "PART_StateLayer");
                    Assert(headerStateLayer.Bounds.Height >= 34,
                        $"table: header hover layer should cover the full header height, got {headerStateLayer.Bounds.Height:F1}");
                    var headerBottom = firstHeader.TranslatePoint(
                                           new Point(firstHeader.Bounds.Width / 2, firstHeader.Bounds.Height - 3), window)
                                       ?? throw new InvalidOperationException("Table header TranslatePoint failed");
                    window.MouseMove(headerBottom);
                    Pump(2, 50);
                    Assert(headerStateLayer.Opacity > 0,
                        "table: hovering below the header text should activate the full header state layer");

                    var firstRow = table.GetVisualDescendants().OfType<TableViewRow>().First();
                    var rowStateLayer = firstRow.GetVisualDescendants().OfType<Border>()
                        .First(border => border.Name == "PART_StateLayer");
                    Assert(rowStateLayer.Bounds.Width >= firstRow.Bounds.Width - 1,
                        $"table: row hover layer should span the row ({rowStateLayer.Bounds.Width:F1}/{firstRow.Bounds.Width:F1})");
                    var rowRight = firstRow.TranslatePoint(
                                       new Point(firstRow.Bounds.Width - 48, firstRow.Bounds.Height / 2), window)
                                   ?? throw new InvalidOperationException("Table row TranslatePoint failed");
                    window.MouseMove(rowRight);
                    Pump(2, 50);
                    Assert(rowStateLayer.Opacity > 0,
                        "table: hovering the empty right side should activate the whole row state layer");
                    Capture(window, "/tmp/m3-table-hover.png", out var tablePixels);
                    Assert(tablePixels > 500, $"table: screenshot looks blank ({tablePixels} px)");

                    sectionsList.SelectedIndex = 18; // Material Symbols virtualized gallery
                    Pump(10, 50);
                    var iconRepeater = view.FindControl<ItemsRepeater>("IconRepeater")
                                       ?? throw new InvalidOperationException("IconRepeater not found");
                    var iconItems = iconRepeater.ItemsSource?.Cast<MaterialSymbolGalleryItem>().ToArray()
                                    ?? throw new InvalidOperationException("IconRepeater items not found");
                    var realizedIcons = iconRepeater.GetVisualChildren().Count();
                    Console.WriteLine($"[icons] items={iconItems.Length} realized={realizedIcons}");
                    Assert(iconItems.Length == 4007,
                        $"icons: expected 4007 items, got {iconItems.Length}");
                    Assert(realizedIcons > 0 && realizedIcons < 100,
                        $"icons: virtualization should realize fewer than 100 cards, got {realizedIcons}");

                    var iconSearch = view.FindControl<TextBox>("IconSearchBox")
                                     ?? throw new InvalidOperationException("IconSearchBox not found");
                    iconSearch.Text = "home";
                    Pump(4, 50);
                    iconItems = iconRepeater.ItemsSource!.Cast<MaterialSymbolGalleryItem>().ToArray();
                    Assert(iconItems.Length > 0 && iconItems.Length < 4007,
                        $"icons: search should filter the catalog, got {iconItems.Length} items");

                    var iconVariant = view.FindControl<ComboBox>("IconVariantCombo")
                                      ?? throw new InvalidOperationException("IconVariantCombo not found");
                    iconVariant.SelectedIndex = 1;
                    Pump(4, 50);
                    iconItems = iconRepeater.ItemsSource!.Cast<MaterialSymbolGalleryItem>().ToArray();
                    Assert(iconItems.All(item => item.ApiName.StartsWith("MaterialSymbolsFilled.", StringComparison.Ordinal)),
                        "icons: Filled selection should expose MaterialSymbolsFilled API names");
                    Capture(window, "/tmp/m3-icons.png", out var iconPixels);
                    Assert(iconPixels > 500, $"icons: screenshot looks blank ({iconPixels} px)");

                    window.Close();
                }

                // ---- Scenario B: narrow (420x800) — overlay closed, hamburger visible ----
                {
                    var view = new MainView();
                    var window = new Window
                    {
                        Width = 420,
                        Height = 800,
                        Title = "Gallery narrow",
                        Content = view,
                    };
                    window.Show();
                    Pump(10, 50);

                    var split = view.FindControl<SplitView>("NavSplit")
                                ?? throw new InvalidOperationException("NavSplit not found");
                    var menu = view.FindControl<Button>("MenuButton")
                               ?? throw new InvalidOperationException("MenuButton not found");

                    Capture(window, "/tmp/m3-narrow.png", out var pixels);
                    Console.WriteLine(
                        $"[narrow] DisplayMode={split.DisplayMode} IsPaneOpen={split.IsPaneOpen} " +
                        $"HamburgerVisible={menu.IsVisible} nonBackgroundPixels={pixels}");

                    Assert(split.DisplayMode == SplitViewDisplayMode.Overlay, "narrow: expected Overlay display mode");
                    Assert(!split.IsPaneOpen, "narrow: expected nav pane closed");
                    Assert(menu.IsVisible, "narrow: expected hamburger visible");
                    Assert(pixels > 500, $"narrow: screenshot looks blank ({pixels} px)");

                    // ---- Scenario C: click the hamburger — overlay drawer opens ----
                    menu.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Pump(10, 50);

                    Capture(window, "/tmp/m3-narrow-open.png", out var openPixels);
                    Console.WriteLine(
                        $"[narrow-open] DisplayMode={split.DisplayMode} IsPaneOpen={split.IsPaneOpen} " +
                        $"nonBackgroundPixels={openPixels}");

                    Assert(split.IsPaneOpen, "narrow-open: expected drawer open after hamburger click");
                    Assert(openPixels > 500, $"narrow-open: screenshot looks blank ({openPixels} px)");

                    window.Close();
                }

                // ---- Scenario D: M3 components (1100x1400) — direct control construction ----
                {
                    var panel = BuildM3ComponentsPanel();
                    var window = new Window
                    {
                        Width = 1100,
                        Height = 1400,
                        Title = "M3 components",
                        Content = new ScrollViewer { Content = panel },
                    };
                    window.Show();
                    Pump(10, 50);

                    Capture(window, "/tmp/m3-components.png", out var pixels);
                    Console.WriteLine($"[m3-components] nonBackgroundPixels={pixels}");
                    Assert(pixels > 500, $"m3-components: screenshot looks blank ({pixels} px)");

                    window.Close();
                }

                // ---- Scenario E: LoadingIndicator frozen at various morph phases ----
                {
                    var row = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 16,
                        Margin = new Thickness(24),
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Left,
                    };
                    foreach (var phase in new[] { 0.0, 1.0, 2.5, 4.0, 5.5 })
                        row.Children.Add(new LoadingIndicator
                        {
                            DebugSegmentOverride = phase,
                            Width = 96,
                            Height = 96,
                        });
                    row.Children.Add(new LoadingIndicator
                    {
                        Variant = LoadingIndicatorVariant.Contained,
                        DebugSegmentOverride = 3.0,
                        Width = 96,
                        Height = 96,
                    });

                    var window = new Window
                    {
                        Width = 760,
                        Height = 160,
                        Title = "LoadingIndicator",
                        Content = row,
                    };
                    window.Show();
                    Pump(6, 50);

                    Capture(window, "/tmp/m3-loading.png", out var pixels);
                    Console.WriteLine($"[m3-loading] nonBackgroundPixels={pixels}");
                    Assert(pixels > 500, $"m3-loading: screenshot looks blank ({pixels} px)");

                    window.Close();
                }

                // ---- Scenario E2: CircularProgressIndicator plain vs wavy ring ----
                {
                    var row = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 24,
                        Margin = new Thickness(24),
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Left,
                    };
                    row.Children.Add(new CircularProgressIndicator
                    {
                        Minimum = 0, Maximum = 100, Value = 65, Width = 96, Height = 96,
                    });
                    row.Children.Add(new CircularProgressIndicator
                    {
                        IsWavy = true, Minimum = 0, Maximum = 100, Value = 70, Width = 96, Height = 96,
                    });
                    row.Children.Add(new CircularProgressIndicator
                    {
                        IsWavy = true, Minimum = 0, Maximum = 100, Value = 97, Width = 96, Height = 96,
                    });
                    row.Children.Add(new CircularProgressIndicator
                    {
                        IsWavy = true, IsIndeterminate = true, Width = 96, Height = 96,
                    });

                    var window = new Window
                    {
                        Width = 560,
                        Height = 160,
                        Title = "Wavy ring",
                        Content = row,
                    };
                    window.Show();
                    Pump(14, 50); // let the 0.5s value animation settle at the target values

                    Capture(window, "/tmp/m3-wavy-ring.png", out var pixels);
                    Console.WriteLine($"[m3-wavy-ring] nonBackgroundPixels={pixels}");
                    Assert(pixels > 500, $"m3-wavy-ring: screenshot looks blank ({pixels} px)");

                    window.Close();
                }

                // ---- Scenario F: ButtonGroup Standard press interaction (M3 Expressive) ----
                {
                    var left = new Button { Content = "Left" };
                    var middle = new Button { Content = "Middle" };
                    var right = new Button { Content = "Right" };
                    var group = new ButtonGroup
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(24),
                    };
                    group.Items.Add(left);
                    group.Items.Add(middle);
                    group.Items.Add(right);

                    var window = new Window
                    {
                        Width = 520,
                        Height = 140,
                        Title = "ButtonGroup press",
                        Content = group,
                    };
                    window.Show();
                    Pump(6, 50);

                    var initLeft = left.Bounds.Width;
                    var initMiddle = middle.Bounds.Width;
                    var initRight = right.Bounds.Width;
                    Console.WriteLine(
                        $"[group-press] initial widths L={initLeft:F2} M={initMiddle:F2} R={initRight:F2}");

                    // Press (and hold) the middle button, then pump ~400ms so the
                    // spring interpolation settles.
                    var center = middle.TranslatePoint(
                                     new Point(middle.Bounds.Width / 2, middle.Bounds.Height / 2), window)
                                 ?? throw new InvalidOperationException("TranslatePoint failed");
                    window.MouseDown(center, MouseButton.Left);
                    Pump(9, 50);

                    var pressedLeft = left.Bounds.Width;
                    var pressedMiddle = middle.Bounds.Width;
                    var pressedRight = right.Bounds.Width;
                    Console.WriteLine(
                        $"[group-press] pressed widths L={pressedLeft:F2} M={pressedMiddle:F2} R={pressedRight:F2} " +
                        $"(dL={pressedLeft - initLeft:F2} dM={pressedMiddle - initMiddle:F2} dR={pressedRight - initRight:F2})");

                    Capture(window, "/tmp/m3-group-press.png", out var pressPixels);
                    Console.WriteLine($"[group-press] nonBackgroundPixels={pressPixels}");

                    Assert(middle.IsPressed, "group-press: expected middle button pressed");
                    Assert(pressedMiddle > initMiddle,
                        $"group-press: expected middle growth, got {pressedMiddle - initMiddle:F2}");
                    Assert(pressedLeft <= initLeft,
                        $"group-press: expected left squeezed ({initLeft:F2} -> {pressedLeft:F2})");
                    Assert(pressedRight <= initRight,
                        $"group-press: expected right squeezed ({initRight:F2} -> {pressedRight:F2})");
                    Assert(pressPixels > 500, $"group-press: screenshot looks blank ({pressPixels} px)");

                    window.MouseUp(center, MouseButton.Left);
                    Pump(9, 50);
                    Console.WriteLine(
                        $"[group-press] released widths L={left.Bounds.Width:F2} M={middle.Bounds.Width:F2} R={right.Bounds.Width:F2}");
                    Assert(!middle.IsPressed, "group-press: expected middle button released");

                    window.Close();
                }

                // ---- Scenario G: Standard ButtonGroup right-edge press interaction ----
                {
                    var left = new Button { Content = "One" };
                    var middle = new Button { Content = "Two" };
                    var right = new Button { Content = "Three" };
                    var clicks = 0;
                    right.Click += (_, _) => clicks++;
                    var group = new ButtonGroup
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(24),
                    };
                    group.Items.Add(left);
                    group.Items.Add(middle);
                    group.Items.Add(right);

                    var window = new Window
                    {
                        Width = 420,
                        Height = 140,
                        Title = "Standard ButtonGroup right press",
                        Content = group,
                    };
                    window.Show();
                    Pump(6, 50);

                    var center = right.TranslatePoint(
                                     new Point(right.Bounds.Width / 2, right.Bounds.Height / 2), window)
                                 ?? throw new InvalidOperationException("TranslatePoint failed");
                    window.MouseDown(center, MouseButton.Left);
                    Pump(9, 50);
                    var middlePresenter = middle.GetVisualDescendants().OfType<ContentPresenter>()
                        .First(presenter => presenter.Name == "PART_ContentPresenter");
                    var middleContent = middlePresenter.GetVisualChildren().OfType<Control>().First();
                    Assert(middleContent.Bounds.Width >= middleContent.DesiredSize.Width - 0.5,
                        $"standard-edge-press: middle label should not be clipped ({middleContent.Bounds.Width:F1}/{middleContent.DesiredSize.Width:F1})");
                    Capture(window, "/tmp/m3-standard-group-right-press.png", out var standardEdgePixels);
                    Assert(standardEdgePixels > 500,
                        $"standard-edge-press: screenshot looks blank ({standardEdgePixels} px)");
                    window.MouseUp(center, MouseButton.Left);

                    for (var i = 0; i < 3; i++)
                    {
                        center = right.TranslatePoint(
                                     new Point(right.Bounds.Width / 2, right.Bounds.Height / 2), window)
                                 ?? throw new InvalidOperationException("TranslatePoint failed");
                        window.MouseDown(center, MouseButton.Left);
                        window.MouseUp(center, MouseButton.Left);
                    }
                    Assert(clicks == 4,
                        $"standard-edge-press: rapid clicks should not queue or disappear, got {clicks}");
                    window.Close();
                }

                // ---- Scenario H: Connected ButtonGroup right-edge press interaction ----
                {
                    var left = new Button { Content = "One" };
                    var middle = new Button { Content = "Two" };
                    var right = new Button { Content = "Three" };
                    var group = new ButtonGroup
                    {
                        Variant = ButtonGroupVariant.Connected,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(24),
                    };
                    group.Items.Add(left);
                    group.Items.Add(middle);
                    group.Items.Add(right);

                    var window = new Window
                    {
                        Width = 420,
                        Height = 140,
                        Title = "Connected ButtonGroup right press",
                        Content = group,
                    };
                    window.Show();
                    Pump(6, 50);

                    var initialLeft = left.Bounds.Width;
                    var initialMiddle = middle.Bounds.Width;
                    var initialRight = right.Bounds.Width;
                    Assert(Math.Abs(right.CornerRadius.TopLeft - 8) < 0.1,
                        $"connected-group-press: right button should start with an 8dp inner corner, got {right.CornerRadius}");
                    var center = right.TranslatePoint(
                                     new Point(right.Bounds.Width / 2, right.Bounds.Height / 2), window)
                                 ?? throw new InvalidOperationException("TranslatePoint failed");
                    window.MouseDown(center, MouseButton.Left);
                    Pump(9, 50);

                    Assert(right.IsPressed, "connected-group-press: expected right button pressed");
                    var pressedInnerCorner = right.CornerRadius.TopLeft;
                    Assert(pressedInnerCorner > 8 && right.CornerRadius.BottomLeft > 8,
                        $"connected-group-press: pressed inner corners should animate toward full rounding, got {right.CornerRadius}");
                    Assert(right.Transitions?.OfType<CornerRadiusTransition>().Any(transition =>
                               transition.Property == Button.CornerRadiusProperty
                               && transition.Duration == TimeSpan.FromMilliseconds(200)) == true,
                        "connected-group-press: expected a 200ms corner-radius transition");
                    Assert(Math.Abs(right.Bounds.Width - initialRight) < 1,
                        $"connected-group-press: right width should stay stable ({initialRight:F2} -> {right.Bounds.Width:F2})");
                    Assert(Math.Abs(middle.Bounds.Width - initialMiddle) < 1,
                        $"connected-group-press: middle width should stay stable ({initialMiddle:F2} -> {middle.Bounds.Width:F2})");
                    Assert(Math.Abs(left.Bounds.Width - initialLeft) < 1,
                        $"connected-group-press: left width should stay stable ({initialLeft:F2} -> {left.Bounds.Width:F2})");
                    Capture(window, "/tmp/m3-connected-group-right-press.png", out var connectedPixels);
                    Assert(connectedPixels > 500,
                        $"connected-group-press: screenshot looks blank ({connectedPixels} px)");

                    window.MouseUp(center, MouseButton.Left);
                    Pump(9, 50);
                    Assert(!right.IsPressed, "connected-group-press: expected right button released");
                    Assert(Math.Abs(right.Bounds.Width - initialRight) < 1,
                        "connected-group-press: expected right width restored after release");
                    window.Close();
                }

                // ---- Scenario I: keyboard and closed-overlay accessibility ----
                {
                    var switchSetting = new SwitchSettingItem { Headline = "Switch" };
                    var checkBoxSetting = new CheckBoxSettingItem { Headline = "Checkbox" };
                    var expandableSetting = new ExpandableSettingItem
                    {
                        Headline = "Expandable",
                        Content = new TextBlock { Text = "Details" },
                    };
                    var bottomSheetButton = new Button { Content = "Bottom action" };
                    var sideSheetButton = new Button { Content = "Side action" };
                    var bottomSheet = new BottomSheet { Content = bottomSheetButton };
                    var sideSheet = new SideSheet { Content = sideSheetButton };
                    var fabAction = new ExtendedFloatingActionButton { Content = "Action" };
                    var fabMenu = new FabMenu { Icon = MakeIcon(PlusPath) };
                    fabMenu.Items.Add(fabAction);

                    var root = new Grid();
                    root.Children.Add(new StackPanel
                    {
                        Children =
                        {
                            switchSetting,
                            checkBoxSetting,
                            expandableSetting,
                            fabMenu,
                        },
                    });
                    root.Children.Add(bottomSheet);
                    root.Children.Add(sideSheet);
                    var window = new Window { Width = 480, Height = 480, Content = root };
                    window.Show();
                    Pump(4, 20);

                    Assert(switchSetting.Focusable && checkBoxSetting.Focusable && expandableSetting.Focusable,
                        "accessibility: interactive setting rows must be keyboard-focusable");
                    switchSetting.Focus(NavigationMethod.Tab);
                    Pump(1, 20);
                    var settingContainer = switchSetting.GetVisualDescendants().OfType<Border>()
                        .First(border => border.Name == "PART_Container");
                    Assert(settingContainer.BorderThickness == new Thickness(2),
                        "accessibility: keyboard-focused setting rows must show a focus indicator");
                    switchSetting.RaiseEvent(new KeyEventArgs
                        { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Space });
                    checkBoxSetting.RaiseEvent(new KeyEventArgs
                        { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Enter });
                    expandableSetting.RaiseEvent(new KeyEventArgs
                        { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Space });
                    Assert(switchSetting.IsChecked && checkBoxSetting.IsChecked && expandableSetting.IsExpanded,
                        "accessibility: Space/Enter must operate interactive setting rows");
                    Assert(!bottomSheet.GetVisualDescendants().OfType<Panel>()
                               .First(panel => panel.Name == "PART_Root").IsVisible
                           && !sideSheet.GetVisualDescendants().OfType<Panel>()
                               .First(panel => panel.Name == "PART_Root").IsVisible,
                        "sheets: closed template roots must not remain visible");
                    bottomSheetButton.Focus();
                    sideSheetButton.Focus();
                    Assert(!bottomSheetButton.IsFocused && !sideSheetButton.IsFocused,
                        "sheets: closed content must not remain focusable through tab navigation");
                    fabAction.Focus();
                    var actionsPresenter = fabMenu.GetVisualDescendants().OfType<ItemsPresenter>()
                        .First(presenter => presenter.Name == "PART_ItemsPresenter");
                    Assert(!actionsPresenter.IsVisible && !fabAction.IsFocused,
                        "fab-menu: closed actions must not remain visible or focusable through tab navigation");

                    bottomSheet.IsOpen = true;
                    Pump(1, 20);
                    bottomSheetButton.RaiseEvent(new KeyEventArgs
                        { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Escape });
                    Assert(!bottomSheet.IsOpen, "bottom-sheet: Escape from content must close the sheet");

                    sideSheet.IsOpen = true;
                    Pump(1, 20);
                    sideSheetButton.RaiseEvent(new KeyEventArgs
                        { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Escape });
                    Assert(!sideSheet.IsOpen, "side-sheet: Escape from content must close the sheet");

                    fabMenu.IsOpen = true;
                    Pump(1, 20);
                    Assert(fabAction.IsEffectivelyVisible, "fab-menu: actions must be visible while open");
                    fabAction.Focus();
                    fabMenu.IsOpen = false;
                    Pump(1, 20);
                    var primaryFab = fabMenu.GetVisualDescendants().OfType<FloatingActionButton>().First();
                    Assert(primaryFab.IsFocused,
                        "fab-menu: closing must return focus to the primary FAB");
                    Assert(!actionsPresenter.IsVisible && !fabAction.IsFocused,
                        "fab-menu: actions must become non-focusable and invisible when closed");
                    window.Close();
                }

                // ---- Scenario J: focus indicators and ProgressBar text contract ----
                {
                    var navigationBarItem = new NavigationBarItem { Label = "Home", Icon = MakeIcon(HomePath) };
                    var navigationRailItem = new NavigationRailItem { Label = "Search", Icon = MakeIcon(SearchPath) };
                    var segmentedButton = new SegmentedButton { Content = "Day" };
                    var rangeSlider = new RangeSlider
                    {
                        Width = 320,
                        LowerValue = 25,
                        UpperValue = 75,
                    };
                    var horizontalProgress = new ProgressBar
                    {
                        Width = 240,
                        Height = 24,
                        Minimum = 20,
                        Maximum = 120,
                        Value = 70,
                        ShowProgressText = true,
                        ProgressTextFormat = "{0:0} of {3:0} ({1:0}%)",
                    };
                    var verticalProgress = new ProgressBar
                    {
                        Width = 24,
                        Height = 240,
                        Orientation = Orientation.Vertical,
                        Value = 25,
                        ShowProgressText = true,
                        ProgressTextFormat = "{1:0}%",
                    };
                    var root = new StackPanel
                    {
                        Margin = new Thickness(16),
                        Spacing = 8,
                        Children =
                        {
                            navigationBarItem,
                            navigationRailItem,
                            segmentedButton,
                            rangeSlider,
                            horizontalProgress,
                            verticalProgress,
                        },
                    };
                    var window = new Window { Width = 420, Height = 720, Content = root };
                    window.Show();
                    Pump(4, 20);

                    foreach (var control in new Control[] { navigationBarItem, navigationRailItem, segmentedButton })
                    {
                        control.Focus(NavigationMethod.Tab);
                        Pump(1, 20);
                        var focusRing = control.GetVisualDescendants().OfType<Border>()
                            .First(border => border.Name == "PART_FocusRing");
                        Assert(focusRing.IsVisible && focusRing.BorderThickness == new Thickness(2),
                            $"focus-visible: {control.GetType().Name} must show the Material 2dp focus ring");
                    }

                    var thumbs = rangeSlider.GetVisualDescendants().OfType<Thumb>().ToArray();
                    Assert(thumbs.Length == 2, $"range-slider focus: expected two thumbs, got {thumbs.Length}");
                    foreach (var thumb in thumbs)
                    {
                        thumb.Focus(NavigationMethod.Tab);
                        Pump(1, 20);
                        var focusRing = thumb.GetVisualDescendants().OfType<Border>()
                            .First(border => border.Name == "PART_FocusRing");
                        Assert(focusRing.IsVisible && focusRing.BorderThickness == new Thickness(2),
                            $"range-slider focus: {thumb.Name} must show the Material 2dp focus ring");
                    }

                    var horizontalText = horizontalProgress.GetVisualDescendants().OfType<TextBlock>().Single();
                    var verticalText = verticalProgress.GetVisualDescendants().OfType<TextBlock>().Single();
                    Assert(horizontalText.Text == "70 of 120 (50%)",
                        $"progress text: custom value/percentage/maximum format was '{horizontalText.Text}'");
                    Assert(verticalText.Text == "25%",
                        $"progress text: vertical percentage format was '{verticalText.Text}'");
                    var verticalTextPresenter = verticalProgress.GetVisualDescendants()
                        .OfType<LayoutTransformControl>()
                        .First(control => control.Name == "PART_ProgressTextPresenter");
                    Assert(verticalTextPresenter.LayoutTransform is RotateTransform { Angle: 90 },
                        "progress text: vertical orientation must rotate the text presenter by 90 degrees");
                    Capture(window, "/tmp/m3-focus-progress.png", out var focusProgressPixels);
                    Assert(focusProgressPixels > 500,
                        $"focus-progress: screenshot looks blank ({focusProgressPixels} px)");
                    window.Close();
                }

                RunRequestedControlRegressions();

                void RunRequestedControlRegressions()
                {
                    // ---- Card activation requires a primary sequence and supports keyboard focus ----
                    {
                    var card = new Card
                    {
                        IsClickable = true,
                        Width = 180,
                        Height = 80,
                        Margin = new Thickness(24),
                        Content = new TextBlock { Text = "Clickable card" },
                    };
                    var other = new Button { Content = "Other" };
                    var clicks = 0;
                    card.Clicked += (_, _) => clicks++;
                    var window = new Window
                    {
                        Width = 260,
                        Height = 190,
                        Content = new StackPanel { Children = { card, other } },
                    };
                    window.Show();
                    Pump(4, 20);

                    Assert(card.Focusable, "card: clickable cards must be keyboard-focusable");
                    var center = card.TranslatePoint(
                                     new Point(card.Bounds.Width / 2, card.Bounds.Height / 2), window)
                                 ?? throw new InvalidOperationException("Card TranslatePoint failed");
                    window.MouseUp(center, MouseButton.Left);
                    window.MouseDown(center, MouseButton.Right);
                    window.MouseUp(center, MouseButton.Right);
                    Assert(clicks == 0,
                        $"card: release-only and secondary clicks must not activate, got {clicks}");

                    window.MouseDown(center, MouseButton.Left);
                    window.MouseUp(center, MouseButton.Left);
                    Assert(clicks == 1, $"card: primary press/release should activate once, got {clicks}");

                    other.Focus();
                    card.Focus(NavigationMethod.Tab);
                    Pump(1, 20);
                    var focusRing = card.GetVisualDescendants().OfType<Border>()
                        .First(border => border.Name == "PART_FocusRing");
                    Assert(focusRing.IsVisible,
                        "card: keyboard-focused clickable cards must show a focus indicator");
                    window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
                    window.KeyRelease(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
                    window.KeyPress(Key.Space, RawInputModifiers.None, PhysicalKey.Space, null);
                    window.KeyRelease(Key.Space, RawInputModifiers.None, PhysicalKey.Space, null);
                    Assert(clicks == 3, $"card: Enter and Space should each activate once, got {clicks}");
                    window.Close();
                    }

                    // ---- TimePickerDial 12-hour keyboard wrapping preserves AM/PM ----
                    {
                    var dial = new TimePickerDial { Mode = TimePickerDialMode.Hours };
                    void Press(Key key) => dial.RaiseEvent(new KeyEventArgs
                        { RoutedEvent = InputElement.KeyDownEvent, Key = key });

                    dial.SelectedHour = 11;
                    Press(Key.Right);
                    Assert(dial.SelectedHour == 0,
                        $"time-dial: 11 AM + 1 should wrap to 12 AM, got {dial.SelectedHour}");
                    Press(Key.Left);
                    Assert(dial.SelectedHour == 11,
                        $"time-dial: 12 AM - 1 should wrap to 11 AM, got {dial.SelectedHour}");
                    dial.SelectedHour = 23;
                    Press(Key.Right);
                    Assert(dial.SelectedHour == 12,
                        $"time-dial: 11 PM + 1 should wrap to 12 PM, got {dial.SelectedHour}");
                    Press(Key.Left);
                    Assert(dial.SelectedHour == 23,
                        $"time-dial: 12 PM - 1 should wrap to 11 PM, got {dial.SelectedHour}");
                    dial.Is24Hour = true;
                    Press(Key.Right);
                    Assert(dial.SelectedHour == 0,
                        $"time-dial: 24-hour mode should still cross midnight, got {dial.SelectedHour}");
                    }

                    // ---- TimePickerPane synchronizes SelectedTime as one two-way batch ----
                    {
                    var values = new TimeValues { SelectedTime = new TimeSpan(9, 15, 0) };
                    var pane = new TimePickerPane();
                    pane.Bind(TimePickerPane.SelectedTimeProperty, new Binding(nameof(TimeValues.SelectedTime))
                    {
                        Source = values,
                    });
                    Assert(pane.SelectedHour == 9 && pane.SelectedMinute == 15,
                        $"time-pane: binding should initialize component values, got {pane.SelectedTime}");

                    var changes = 0;
                    var observed = TimeSpan.MinValue;
                    pane.SelectedTimeChanged += (_, _) =>
                    {
                        changes++;
                        observed = pane.SelectedTime;
                    };
                    values.SelectedTime = new TimeSpan(17, 42, 37);
                    Assert(pane.SelectedTime == new TimeSpan(17, 42, 0)
                           && pane.SelectedHour == 17 && pane.SelectedMinute == 42,
                        $"time-pane: SelectedTime should synchronize final hour/minute values, got {pane.SelectedTime}");
                    Assert(changes == 1 && observed == new TimeSpan(17, 42, 0),
                        $"time-pane: setting SelectedTime should raise one final event, got {changes} at {observed}");

                    pane.SelectedMinute = 43;
                    Assert(values.SelectedTime == new TimeSpan(17, 43, 0),
                        $"time-pane: default binding must update the source, got {values.SelectedTime}");
                    Assert(changes == 2,
                        $"time-pane: component updates should raise one event, got {changes}");
                    }

                    // ---- ButtonGroup preserves consumer property sources ----
                    {
                    var values = new ButtonValues
                    {
                        Width = 116,
                        Padding = new Thickness(22, 3),
                        CornerRadius = new CornerRadius(6),
                    };
                    var left = new Button
                    {
                        Content = "Local",
                        Width = 108,
                        Padding = new Thickness(19, 2),
                        CornerRadius = new CornerRadius(5),
                    };
                    var middle = new Button { Content = "Bound" };
                    middle.Bind(Layoutable.WidthProperty, values.GetObservable(ButtonValues.WidthProperty));
                    middle.Bind(Button.PaddingProperty, values.GetObservable(ButtonValues.PaddingProperty));
                    middle.Bind(Button.CornerRadiusProperty, values.GetObservable(ButtonValues.CornerRadiusProperty));
                    var right = new Button { Content = "Right" };
                    var group = new ButtonGroup
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(24),
                    };
                    group.Items.Add(left);
                    group.Items.Add(middle);
                    group.Items.Add(right);

                    var window = new Window
                    {
                        Width = 500,
                        Height = 140,
                        Title = "ButtonGroup property preservation",
                        Content = group,
                    };
                    window.Show();
                    Pump(6, 50);

                    var center = middle.TranslatePoint(
                                     new Point(middle.Bounds.Width / 2, middle.Bounds.Height / 2), window)
                                 ?? throw new InvalidOperationException("TranslatePoint failed");
                    window.MouseDown(center, MouseButton.Left);
                    Pump(3, 50);
                    group.Variant = ButtonGroupVariant.Connected;
                    window.MouseUp(center, MouseButton.Left);
                    Pump(9, 50);
                    group.Variant = ButtonGroupVariant.Standard;
                    Pump(2, 50);

                    Assert(Math.Abs(left.Width - 108) < 0.1 && left.Padding == new Thickness(19, 2)
                           && left.CornerRadius == new CornerRadius(5),
                        $"group-values: local values were not restored ({left.Width}, {left.Padding}, {left.CornerRadius})");
                    Assert(Math.Abs(middle.Width - 116) < 0.1 && middle.Padding == new Thickness(22, 3)
                           && middle.CornerRadius == new CornerRadius(6),
                        $"group-values: bound values were not restored ({middle.Width}, {middle.Padding}, {middle.CornerRadius})");

                    values.Width = 124;
                    values.Padding = new Thickness(24, 4);
                    values.CornerRadius = new CornerRadius(7);
                    Pump(5, 50);
                    Assert(Math.Abs(middle.Width - 124) < 0.1 && middle.Padding == new Thickness(24, 4)
                           && middle.GetBaseValue(Button.CornerRadiusProperty).Value == new CornerRadius(7)
                           && Math.Abs(middle.CornerRadius.TopLeft - 7) < 0.2,
                        $"group-values: bindings must remain live after animation and variant changes ({middle.Width}, {middle.Padding}, {middle.CornerRadius})");

                    center = middle.TranslatePoint(
                                 new Point(middle.Bounds.Width / 2, middle.Bounds.Height / 2), window)
                             ?? throw new InvalidOperationException("TranslatePoint failed");
                    window.MouseDown(center, MouseButton.Left);
                    Pump(3, 50);
                    group.Items.Remove(middle);
                    Pump(2, 20);
                    Assert(Math.Abs(middle.Width - 124) < 0.1 && middle.Padding == new Thickness(24, 4)
                           && middle.CornerRadius == new CornerRadius(7),
                        "group-values: container clearing must release all temporary property overrides");
                    values.Width = 132;
                    values.Padding = new Thickness(26, 5);
                    values.CornerRadius = new CornerRadius(9);
                    Pump(5, 50);
                    Assert(Math.Abs(middle.Width - 132) < 0.1 && middle.Padding == new Thickness(26, 5)
                            && middle.GetBaseValue(Button.CornerRadiusProperty).Value == new CornerRadius(9),
                        $"group-values: bindings must remain live after container clearing ({middle.Width}, {middle.Padding}, {middle.CornerRadius}, base={middle.GetBaseValue(Button.CornerRadiusProperty).Value})");
                    window.MouseUp(center, MouseButton.Left);
                    window.Close();
                }

                    // ---- SegmentedButtonGroup normalizes Single selection ----
                    {
                    var first = new SegmentedButton { Content = "First", IsChecked = true };
                    var second = new SegmentedButton { Content = "Second", IsChecked = true };
                    var third = new SegmentedButton { Content = "Third", IsChecked = true };
                    var group = new SegmentedButtonGroup
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(24),
                    };
                    group.Items.Add(first);
                    group.Items.Add(second);
                    group.Items.Add(third);
                    var window = new Window
                    {
                        Width = 420,
                        Height = 140,
                        Title = "Segmented selection normalization",
                        Content = group,
                    };
                    window.Show();
                    Pump(4, 50);
                    Assert(first.IsChecked == true && second.IsChecked == false && third.IsChecked == false,
                        "segmented-single: initially checked children should deterministically keep the first item");

                    group.SelectionMode = SegmentedSelectionMode.Multiple;
                    second.IsChecked = true;
                    third.IsChecked = true;
                    group.SelectionMode = SegmentedSelectionMode.Single;
                    Assert(first.IsChecked == true && second.IsChecked == false && third.IsChecked == false,
                        "segmented-single: Multiple to Single should deterministically keep the first checked item");
                    window.Close();
                    }
                }
            }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("RENDER FAILED:");
            Console.Error.WriteLine(ex);
            Environment.Exit(1);
        }

        Console.WriteLine("OK");
        Environment.Exit(0);
        return 0;
    }

    private const string HeartPath =
        "M12,21.35L10.55,20.03C5.4,15.36 2,12.28 2,8.5C2,5.42 4.42,3 7.5,3C9.24,3 10.91,3.81 12,5.09C13.09,3.81 14.76,3 16.5,3C19.58,3 22,5.42 22,8.5C22,12.28 18.6,15.36 13.45,20.03L12,21.35Z";
    private const string PlusPath = "M19,13H13V19H11V13H5V11H11V5H13V11H19V13Z";
    private const string HomePath = "M10,20V14H14V20H19V12H22L12,3L2,12H5V20H10Z";
    private const string SearchPath =
        "M9.5,3A6.5,6.5 0 0,1 16,9.5C16,11.11 15.41,12.59 14.44,13.73L14.71,14H15.5L20.5,19L19,20.5L14,15.5V14.71L13.73,14.44C12.59,15.41 11.11,16 9.5,16A6.5,6.5 0 0,1 3,9.5A6.5,6.5 0 0,1 9.5,3M9.5,5C7,5 5,7 5,9.5C5,12 7,14 9.5,14C12,14 14,12 14,9.5C14,7 12,5 9.5,5Z";
    private const string BellPath =
        "M21,19V20H3V19L5,17V11C5,7.9 7.03,5.17 10,4.29C10,4.19 10,4.1 10,4A2,2 0 0,1 12,2A2,2 0 0,1 14,4C14,4.1 14,4.19 14,4.29C16.97,5.17 19,7.9 19,11V17L21,19M14,21A2,2 0 0,1 12,23A2,2 0 0,1 10,21";

    private static PathIcon MakeIcon(string data) => new() { Data = StreamGeometry.Parse(data) };

    private static void RunControlValidationChecks()
    {
        var badge = new Badge { Value = -1, MaxValue = -2 };
        Assert(badge.Value == 0 && badge.MaxValue == 0 && badge.DisplayText == "0",
            $"badge: negative counts should coerce to zero, got {badge.Value}/{badge.MaxValue}/{badge.DisplayText}");

        AssertThrows<ArgumentException>(() => new Avatar { Size = double.NaN },
            "avatar: NaN Size should be rejected");
        AssertThrows<ArgumentException>(() => new Avatar { Size = 0 },
            "avatar: non-positive Size should be rejected");

        AssertThrows<ArgumentException>(() => new CircularProgressIndicator { StrokeWidth = 0 },
            "circular-progress: non-positive StrokeWidth should be rejected");
        AssertThrows<ArgumentException>(() => new CircularProgressIndicator { TrackGap = -1 },
            "circular-progress: negative TrackGap should be rejected");
        AssertThrows<ArgumentException>(() => new CircularProgressIndicator { WaveCount = double.PositiveInfinity },
            "circular-progress: infinite WaveCount should be rejected");
        AssertThrows<ArgumentException>(() => new WavyProgressBar { Amplitude = -1 },
            "wavy-progress: negative Amplitude should be rejected");
        AssertThrows<ArgumentException>(() => new WavyProgressBar { Wavelength = 0 },
            "wavy-progress: non-positive Wavelength should be rejected");

        var range = new RangeSlider();
        range.LowerValue = 90;
        Assert(range.LowerValue == 80 && range.LowerValue <= range.UpperValue,
            $"range-slider: LowerValue must not exceed UpperValue, got {range.LowerValue}/{range.UpperValue}");
        range.UpperValue = 10;
        Assert(range.UpperValue == range.LowerValue,
            $"range-slider: UpperValue must not cross LowerValue, got {range.LowerValue}/{range.UpperValue}");
        range.Minimum = 150;
        Assert(range.Minimum == range.Maximum && range.LowerValue == range.UpperValue && range.LowerValue == range.Minimum,
            $"range-slider: incoherent bounds should coerce coherently, got {range.Minimum}/{range.LowerValue}/{range.UpperValue}/{range.Maximum}");
        AssertThrows<ArgumentException>(() => range.LowerValue = double.NaN,
            "range-slider: NaN values should be rejected");
        AssertThrows<ArgumentException>(() => range.TickFrequency = 0,
            "range-slider: non-positive TickFrequency should be rejected");
        AssertThrows<ArgumentException>(() => range.SmallChange = 0,
            "range-slider: non-positive SmallChange should be rejected");

        var host = new SnackbarHost();
        host.Show("queued", duration: TimeSpan.FromSeconds(5));
        var window = new Window { Content = host };
        window.Show();
        Pump(2, 20);
        var slot = host.GetVisualDescendants().OfType<ContentControl>()
            .First(control => control.Name == SnackbarHost.PartSnackbarSlot);
        Assert(slot.Content is Snackbar { Message: "queued" },
            "snackbar-host: Show before template application should display after attachment");
        window.Content = null;
        Pump(1, 20);
        Assert(slot.Content is null && !slot.Classes.Contains("open"),
            "snackbar-host: detach should clear content and open state");
        window.Close();
        AssertThrows<ArgumentOutOfRangeException>(() => host.Show("invalid", duration: TimeSpan.Zero),
            "snackbar-host: non-positive duration should be rejected");
    }

    private static TextBlock Caption(string text)
    {
        var block = new TextBlock { Text = text };
        block.Classes.Add("title-small");
        return block;
    }

    private sealed class TestCommand(Action execute) : ICommand
    {
        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => execute();

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }
    }

    private sealed class ButtonValues : AvaloniaObject
    {
        public static readonly StyledProperty<double> WidthProperty =
            AvaloniaProperty.Register<ButtonValues, double>(nameof(Width));

        public static readonly StyledProperty<Thickness> PaddingProperty =
            AvaloniaProperty.Register<ButtonValues, Thickness>(nameof(Padding));

        public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
            AvaloniaProperty.Register<ButtonValues, CornerRadius>(nameof(CornerRadius));

        public double Width
        {
            get => GetValue(WidthProperty);
            set => SetValue(WidthProperty, value);
        }

        public Thickness Padding
        {
            get => GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }

        public CornerRadius CornerRadius
        {
            get => GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
    }

    private sealed class TimeValues : AvaloniaObject
    {
        public static readonly StyledProperty<TimeSpan> SelectedTimeProperty =
            AvaloniaProperty.Register<TimeValues, TimeSpan>(nameof(SelectedTime));

        public TimeSpan SelectedTime
        {
            get => GetValue(SelectedTimeProperty);
            set => SetValue(SelectedTimeProperty, value);
        }
    }

    private static StackPanel BuildM3ComponentsPanel()
    {
        var panel = new StackPanel
        {
            Margin = new Thickness(24),
            Spacing = 12,
            HorizontalAlignment = HorizontalAlignment.Left,
        };

        // ChipGroup with 4 chip kinds
        panel.Children.Add(Caption("Chips"));
        var chips = new ChipGroup { Spacing = 8 };
        chips.Items.Add(new AssistChip { Content = "Assist", Icon = MakeIcon(BellPath) });
        chips.Items.Add(new FilterChip { Content = "Filter", IsChecked = true });
        chips.Items.Add(new InputChip { Content = "Input", IsRemovable = true });
        chips.Items.Add(new SuggestionChip { Content = "Suggestion" });
        panel.Children.Add(chips);

        // IconButton variants
        panel.Children.Add(Caption("IconButton variants"));
        var iconButtons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        foreach (var variant in Enum.GetValues<IconButtonVariant>())
            iconButtons.Children.Add(new IconButton { Variant = variant, Content = MakeIcon(HeartPath) });
        panel.Children.Add(iconButtons);

        // IconToggleButton variants (2 checked)
        panel.Children.Add(Caption("IconToggleButton variants"));
        var toggles = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        var toggleIndex = 0;
        foreach (var variant in Enum.GetValues<IconButtonVariant>())
        {
            toggles.Children.Add(new IconToggleButton
            {
                Variant = variant,
                Content = MakeIcon(HeartPath),
                IsChecked = toggleIndex++ % 2 == 0,
            });
        }
        panel.Children.Add(toggles);

        // FABs
        panel.Children.Add(Caption("Floating action buttons"));
        var fabs = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 16,
            VerticalAlignment = VerticalAlignment.Center,
        };
        fabs.Children.Add(new FloatingActionButton { Size = FabSize.Small, Content = MakeIcon(PlusPath) });
        fabs.Children.Add(new FloatingActionButton { Size = FabSize.Medium, Content = MakeIcon(PlusPath) });
        fabs.Children.Add(new FloatingActionButton { Size = FabSize.Large, Content = MakeIcon(PlusPath) });
        fabs.Children.Add(new ExtendedFloatingActionButton { Content = "Compose", Icon = MakeIcon(PlusPath) });
        panel.Children.Add(fabs);

        // SegmentedButtonGroup, 3 segments, 1 checked
        panel.Children.Add(Caption("SegmentedButtonGroup"));
        var segments = new SegmentedButtonGroup { HorizontalAlignment = HorizontalAlignment.Left };
        segments.Items.Add(new SegmentedButton { Content = "Day", IsChecked = true });
        segments.Items.Add(new SegmentedButton { Content = "Week" });
        segments.Items.Add(new SegmentedButton { Content = "Month" });
        panel.Children.Add(segments);

        // Avatars + BadgedBox
        panel.Children.Add(Caption("Avatar / BadgedBox"));
        var avatars = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 16,
            VerticalAlignment = VerticalAlignment.Center,
        };
        avatars.Children.Add(new Avatar { Text = "Circle", Variant = AvatarVariant.Circle });
        avatars.Children.Add(new Avatar { Text = "Rounded", Variant = AvatarVariant.Rounded });
        avatars.Children.Add(new Avatar { Text = "Square", Variant = AvatarVariant.Square });
        avatars.Children.Add(new BadgedBox
        {
            Badge = new Badge { Value = 150, MaxValue = 99 },
            Content = new IconButton { Content = MakeIcon(BellPath) },
        });
        panel.Children.Add(avatars);

        // RangeSlider
        panel.Children.Add(Caption("RangeSlider"));
        panel.Children.Add(new RangeSlider
        {
            Minimum = 0,
            Maximum = 100,
            LowerValue = 20,
            UpperValue = 80,
            Width = 360,
            HorizontalAlignment = HorizontalAlignment.Left,
        });

        // SearchBar
        panel.Children.Add(Caption("SearchBar"));
        panel.Children.Add(new SearchBar
        {
            Watermark = "Search components",
            Width = 360,
            HorizontalAlignment = HorizontalAlignment.Left,
        });

        // Cards
        panel.Children.Add(Caption("Card variants"));
        var cards = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 16 };
        foreach (var variant in Enum.GetValues<CardVariant>())
        {
            cards.Children.Add(new Card
            {
                Variant = variant,
                Width = 160,
                Height = 100,
                Content = new TextBlock { Text = $"{variant} card", Margin = new Thickness(16) },
            });
        }
        panel.Children.Add(cards);

        // NavigationBar
        panel.Children.Add(Caption("NavigationBar"));
        var navBar = new NavigationBar { Width = 480, HorizontalAlignment = HorizontalAlignment.Left };
        navBar.Items.Add(new NavigationBarItem { Label = "Home", Icon = MakeIcon(HomePath) });
        navBar.Items.Add(new NavigationBarItem { Label = "Favorites", Icon = MakeIcon(HeartPath) });
        navBar.Items.Add(new NavigationBarItem { Label = "Search", Icon = MakeIcon(SearchPath) });
        navBar.SelectedIndex = 0;
        panel.Children.Add(navBar);

        // SettingItem + SwitchSettingItem
        panel.Children.Add(Caption("Setting items"));
        var settings = new StackPanel { Width = 480, HorizontalAlignment = HorizontalAlignment.Left };
        settings.Children.Add(new SettingItem
        {
            Headline = "Account",
            SupportingText = "Manage your account details",
            Icon = MakeIcon(HomePath),
        });
        settings.Children.Add(new SwitchSettingItem
        {
            Headline = "Notifications",
            SupportingText = "Enable push notifications",
            IsChecked = true,
        });
        panel.Children.Add(settings);

        // Snackbar
        panel.Children.Add(Caption("Snackbar"));
        panel.Children.Add(new Snackbar
        {
            Message = "Photo saved to album",
            ActionText = "UNDO",
            HorizontalAlignment = HorizontalAlignment.Left,
        });

        // DialogPane
        panel.Children.Add(Caption("DialogPane"));
        var dialogButtons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var cancel = new Button { Content = "Cancel" };
        cancel.Classes.Add("text");
        var ok = new Button { Content = "OK" };
        ok.Classes.Add("text");
        dialogButtons.Children.Add(cancel);
        dialogButtons.Children.Add(ok);
        panel.Children.Add(new DialogPane
        {
            Title = "Dialog title",
            Width = 360,
            HorizontalAlignment = HorizontalAlignment.Left,
            Content = new TextBlock
            {
                Text = "A dialog is a modal window that appears in front of app content.",
                TextWrapping = TextWrapping.Wrap,
            },
            Buttons = dialogButtons,
        });

        return panel;
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }

    private static void CheckPublicApiGuards()
    {
        var whitePoint = ColorUtils.WhitePointD65();
        whitePoint[0] = 0;
        Assert(ColorUtils.WhitePointD65()[0] == 95.047,
            "colors: D65 white point should not expose mutable static storage");

        var suppliedWhitePoint = ColorUtils.WhitePointD65();
        var viewingConditions = ViewingConditions.Make(suppliedWhitePoint);
        var expectedRgbD = viewingConditions.RgbD[0];
        suppliedWhitePoint[0] = 1;
        var exposedRgbD = viewingConditions.RgbD;
        exposedRgbD[0] = 0;
        Assert(viewingConditions.RgbD[0] == expectedRgbD,
            "colors: viewing conditions should isolate supplied and exposed arrays");

        var palette = TonalPalette.FromInt(0xff6750a4);
        AssertThrows<ArgumentOutOfRangeException>(() => palette.Tone(-1),
            "colors: negative tones should be rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => palette.Tone(101),
            "colors: tones above 100 should be rejected");
        var tones = new uint[64];
        Parallel.For(0, tones.Length, i => tones[i] = palette.Tone(40));
        Assert(tones.All(tone => tone == tones[0]),
            "colors: concurrent tone requests should return a stable cached value");

        var theme = new MaterialTheme();
        AssertThrows<ArgumentException>(() => theme.ContrastLevel = double.NaN,
            "theme: non-finite contrast should be rejected");
        AssertThrows<ArgumentException>(() => theme.ContrastLevel = 1.01,
            "theme: contrast above 1 should be rejected");
        AssertThrows<ArgumentException>(() => theme.SchemeVariant = (SchemeVariant)int.MaxValue,
            "theme: undefined scheme variants should be rejected");

        var rippleHost = new RippleHost();
        AssertThrows<ArgumentException>(() => rippleHost.RippleOpacity = double.PositiveInfinity,
            "ripple: non-finite opacity should be rejected");
        AssertThrows<ArgumentException>(() => rippleHost.RippleOpacity = -0.01,
            "ripple: opacity below 0 should be rejected");
        AssertThrows<ArgumentException>(() => rippleHost.RippleOpacity = 1.01,
            "ripple: opacity above 1 should be rejected");

        CheckRippleLifecycle(rippleHost);
    }

    private static void CheckRippleLifecycle(RippleHost rippleHost)
    {
        rippleHost.Arrange(new Rect(0, 0, 40, 40));
        var source = new Button();
        typeof(RippleHost).GetField("_source", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(rippleHost, source);
        var keyDown = typeof(RippleHost).GetMethod("OnSourceKeyDown", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var keyArgs = new KeyEventArgs { Key = Key.Space };
        keyDown.Invoke(rippleHost, [source, keyArgs]);
        keyDown.Invoke(rippleHost, [source, keyArgs]);

        var ripples = (IList)typeof(RippleHost).GetField("_ripples", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(rippleHost)!;
        Assert(ripples.Count == 1, "ripple: key auto-repeat should not start repeated ripples");

        rippleHost.IsRippleEnabled = false;
        var released = ripples[0]!.GetType().GetField("Released")!.GetValue(ripples[0]);
        Assert(released is not null, "ripple: disabling should release active ripples");
    }

    private static void AssertThrows<TException>(Action action, string message)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return;
        }

        throw new InvalidOperationException(message);
    }

    private static void Pump(int iterations, int sleepMs)
    {
        for (var i = 0; i < iterations; i++)
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(sleepMs);
        }
    }

    private static void Capture(Window window, string path, out int nonBackgroundPixels)
    {
        window.InvalidateVisual();
        Dispatcher.UIThread.RunJobs();

        var frame = window.CaptureRenderedFrame()
                    ?? throw new InvalidOperationException("CaptureRenderedFrame returned null");
        frame.Save(path, PngBitmapEncoderOptions.Default);
        nonBackgroundPixels = CountNonBackgroundPixels(frame);
        Console.WriteLine($"Saved {path}");
    }

    private static int CountNonBackgroundPixels(WriteableBitmap bitmap)
    {
        using var fb = bitmap.Lock();
        var width = fb.Size.Width;
        var height = fb.Size.Height;
        var row = new int[width];

        Marshal.Copy(fb.Address, row, 0, 1);
        var background = row[0];

        var count = 0;
        for (var y = 0; y < height; y++)
        {
            Marshal.Copy(fb.Address + y * fb.RowBytes, row, 0, width);
            for (var x = 0; x < width; x++)
            {
                if (row[x] != background)
                    count++;
            }
        }

        return count;
    }
}
