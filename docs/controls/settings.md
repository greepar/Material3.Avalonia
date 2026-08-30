# Settings Items

Settings rows live in `Material3.Avalonia.Controls`:

```xml
xmlns:m3c="using:Material3.Avalonia.Controls"
```

## Complete example

```xml
<m3c:Card Variant="Outlined"
            Width="480"
            ClipToBounds="True">
    <StackPanel>
        <m3c:SettingItem Headline="Account"
                         SupportingText="Manage your account details">
            <m3c:SettingItem.Trailing>
                <TextBlock Text="Signed in" />
            </m3c:SettingItem.Trailing>
        </m3c:SettingItem>

        <m3c:SwitchSettingItem Headline="Notifications"
                               SupportingText="Enable push notifications"
                               IsChecked="{Binding NotificationsEnabled}" />

        <m3c:CheckBoxSettingItem Headline="Auto-sync"
                                 IsChecked="{Binding AutoSyncEnabled}" />

        <m3c:ExpandableSettingItem Headline="Advanced"
                                   SupportingText="More options"
                                   IsExpanded="{Binding IsAdvancedOpen}">
            <StackPanel Margin="16,0,16,12" Spacing="8">
                <CheckBox Content="Developer mode" />
                <TextBox Watermark="Endpoint" />
            </StackPanel>
        </m3c:ExpandableSettingItem>
    </StackPanel>
</m3c:Card>
```

The card clips its contents in this example so row hover layers do not paint outside the rounded card.

## SettingItem

| Property | Type | Default | Description |
|---|---|---|---|
| `Icon` | `object?` | `null` | Optional leading 24dp content. |
| `Headline` | `string?` | `null` | Primary text. |
| `SupportingText` | `string?` | `null` | Optional wrapping secondary text. |
| `Trailing` | `object?` | `null` | Value label, chevron, switch, or other trailing content. |

Although `SettingItem` derives from `ContentControl`, the default template does not display inherited `Content`. Use `Icon` and `Trailing` property elements instead.

## SwitchSettingItem

`SwitchSettingItem` inherits `SettingItem` and supplies a trailing `ToggleSwitch`.

| Member | Type | Default | Description |
|---|---|---|---|
| `IsChecked` | `bool` | `false` | Two-way by default. |
| `IsCheckedChanged` | `event EventHandler` | - | Raised for user, binding, and programmatic changes. |

The whole row toggles on an unhandled left-pointer release. The embedded switch is a visual state indicator rather than an independent pointer target.

## CheckBoxSettingItem

`CheckBoxSettingItem` has the same `IsChecked` and `IsCheckedChanged` API, but displays a trailing checkbox. Its state is a non-nullable `bool`; indeterminate state is not supported.

## ExpandableSettingItem

`ExpandableSettingItem` is a separate `ContentControl`, not a subclass of `SettingItem`. It has `Icon`, `Headline`, and `SupportingText`, plus:

| Property | Type | Default | Description |
|---|---|---|---|
| `IsExpanded` | `bool` | `false` | Two-way by default; reveals direct child content. |

Direct child content is the expandable body. It does not have `Trailing`; the default template supplies a rotating chevron.

::: warning Keyboard and automation
The current settings rows implement pointer interaction but do not add explicit keyboard toggle/expand handling or custom automation peers. Verify keyboard and screen-reader behavior for accessibility-sensitive applications, and do not treat them as semantic replacements for built-in `CheckBox`, `ToggleSwitch`, or `Expander` without testing.
:::
