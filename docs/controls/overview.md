# Controls Overview

Every built-in Avalonia control has an implicit Material 3 `ControlTheme` — you use the standard controls and they just look right. No `Theme=` assignments needed.

Covered (80+): `Button` family, `CheckBox`, `RadioButton`, `ToggleSwitch`, `Slider`, `ProgressBar`, `TextBox`, `AutoCompleteBox`, `NumericUpDown`, `ComboBox`, `ListBox`, `TreeView`, `TabControl`, `Menu`/`ContextMenu`/flyouts, `Calendar`, `DatePicker`, `TimePicker`, `TableView`, `Expander`, `SplitView`, `NotificationCard`, window chrome, pages, and more.

Material-specific APIs include `RangeSlider`, `CircularProgressIndicator`, `WavyProgressBar`, `LoadingIndicator`, chips, FABs and FAB menus, segmented and expressive button groups, cards, sheets, search, snackbar, dialog panes, and time-picker controls. See [Sliders](./sliders) for the complete lower/upper value API.

## Button style classes

The default `Button` is **filled**. Add a class for the other M3 variants:

```xml
<Button Content="Filled" />
<Button Classes="elevated" Content="Elevated" />
<Button Classes="tonal"    Content="Tonal" />
<Button Classes="outlined" Content="Outlined" />
<Button Classes="text"     Content="Text" />
<Button Classes="danger"   Content="Danger" />
```

Interaction elevation follows Material Web: filled/tonal raise to level 1 on hover and settle back on press; elevated rests at 1, hovers at 2.

## Other classes

```xml
<Separator Classes="vertical" />
```

## Selection controls

```xml
<CheckBox Content="Agree" IsThreeState="True" />
<RadioButton GroupName="g" Content="Option A" />
<ToggleSwitch OnContent="On" OffContent="Off" />
```

All come with animated state changes (knob resize/recolor, check-mark scale-in) and circular state-layer ripples.

## Text fields

`TextBox`, `ComboBox`, `NumericUpDown`, date/time pickers all use the M3 **outlined field** look (8dp corners, 56dp min height, 2dp primary border when focused, error state via `DataValidationErrors`).

```xml
<TextBox Watermark="Label" />
<TextBox PasswordChar="●" Watermark="Password" />
<ComboBox PlaceholderText="Choose…">
    <ComboBoxItem>One</ComboBoxItem>
</ComboBox>
```

## Notifications

`WindowNotificationManager` + `NotificationCard` render as M3 cards with type-tinted containers and decelerate slide-in / sink-out animations:

```csharp
var manager = new WindowNotificationManager(TopLevel.GetTopLevel(this))
{
    Position = NotificationPosition.BottomRight,
};
manager.Show(new Notification("Saved", "Your changes are safe.", NotificationType.Success));
```

## Dialogs

For an M3 basic dialog use a borderless window with a 28dp container (see the gallery's `OnShowDialog` for a complete snippet):

```csharp
var dialog = new Window
{
    WindowDecorations = WindowDecorations.None,
    TransparencyLevelHint = [WindowTransparencyLevel.Transparent],
    Background = Brushes.Transparent,
    SizeToContent = SizeToContent.Height,
};
// content: Border { CornerRadius = 28, BoxShadow = Md3Elevation3, Margin = 24 }
```
