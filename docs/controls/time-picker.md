# Time Picker (Dial & Pane)

Two Android-style Material 3 time selection components in `Material3.Avalonia.Controls`, alongside the themed built-in `TimePicker`.

## TimePickerPane — complete picker

The full M3 time picker: hour/minute display chips, AM/PM segment, clock dial, keyboard-input mode, and Cancel/OK actions.

```xml
<m3c:TimePickerPane x:Name="Picker"
                    SelectedHour="14" SelectedMinute="30"
                    Is24Hour="False" />
```

```csharp
Picker.Confirmed += (_, _) => UseTime(Picker.SelectedTime);   // TimeSpan
Picker.Canceled  += (_, _) => Close();
Picker.SelectedTimeChanged += (_, _) => Preview(Picker.SelectedTime);
```

| Property | Type | Default | |
|---|---|---|---|
| `SelectedHour` | `int` | `0` | 0–23, two-way |
| `SelectedMinute` | `int` | `0` | 0–59, two-way |
| `SelectedTime` | `TimeSpan` | `00:00` | Two-way styled property; updates hour and minute as one notification batch |
| `Is24Hour` | `bool` | `false` | Hides AM/PM, dial gets a double ring |
| `IsInputMode` | `bool` | `false` | `false` = "Select time" dial · `true` = "Enter time" text boxes |
| `Title` | `string?` | `null` | `null` shows the mode-appropriate default title |

Behavior matching Android: committing an hour on the dial auto-advances to minutes; the bottom-left icon button toggles dial/keyboard mode; in input mode typing two digits in the hour box jumps to minutes.

## TimePickerDial — just the clock face

Use the dial standalone when you build your own chrome:

```xml
<m3c:TimePickerDial Mode="Hours" Is24Hour="True"
                    SelectedHour="17" SelectedMinute="30" />
```

| Property / Event | Type | Notes |
|---|---|---|
| `Mode` | `TimePickerDialMode` | `Hours` or `Minutes` |
| `Is24Hour` | `bool` | Outer ring 1–12, inner ring 00/13–23 |
| `SelectedHour` / `SelectedMinute` | `int` | Two-way |
| `SelectorBrush` / `SelectedForeground` / `SecondaryForeground` | `IBrush?` | Themed to Primary / OnPrimary / OnSurfaceVariant |
| `SelectedTimeChanged` | event | Fires on every value change |
| `SelectionCommitted` | event | Fires on pointer release — hook this to auto-advance |

Interaction: dragging tracks the finger 1:1 (no tick snapping mid-drag) and settles to the nearest tick with a 150 ms decelerate animation on release; clicks animate along the shortest arc; arrow keys nudge ±1. In 12-hour mode, arrow-key wrapping stays within the current AM or PM half-day.

## Built-in TimePicker / DatePicker

The standard Avalonia `TimePicker`, `DatePicker`, `CalendarDatePicker` and `Calendar` are fully themed too (outlined fields, 28dp presenter dialogs, pill day cells), if you prefer the looping-list pickers.
