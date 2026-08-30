using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Material3.Avalonia.Controls;

/// <summary>
/// Material 3 time picker input pane (Android style): hour/minute display chips,
/// an AM/PM selector (12-hour mode) and a <see cref="TimePickerDial"/>. Selecting an
/// hour on the dial automatically advances to minute selection. Supports a keyboard
/// input mode ("Enter time") with hour/minute text boxes, toggled via
/// <see cref="IsInputMode"/>, plus a title and Cancel/OK action row.
/// </summary>
[TemplatePart(PartHourButton, typeof(ToggleButton))]
[TemplatePart(PartMinuteButton, typeof(ToggleButton))]
[TemplatePart(PartAmButton, typeof(ToggleButton))]
[TemplatePart(PartPmButton, typeof(ToggleButton))]
[TemplatePart(PartDial, typeof(TimePickerDial))]
[TemplatePart(PartHourTextBox, typeof(TextBox))]
[TemplatePart(PartMinuteTextBox, typeof(TextBox))]
[TemplatePart(PartModeToggleButton, typeof(Button))]
[TemplatePart(PartCancelButton, typeof(Button))]
[TemplatePart(PartOkButton, typeof(Button))]
[TemplatePart(PartTitleText, typeof(TextBlock))]
public class TimePickerPane : TemplatedControl
{
    public const string PartHourButton = "PART_HourButton";
    public const string PartMinuteButton = "PART_MinuteButton";
    public const string PartAmButton = "PART_AmButton";
    public const string PartPmButton = "PART_PmButton";
    public const string PartDial = "PART_Dial";
    public const string PartHourTextBox = "PART_HourTextBox";
    public const string PartMinuteTextBox = "PART_MinuteTextBox";
    public const string PartModeToggleButton = "PART_ModeToggleButton";
    public const string PartCancelButton = "PART_CancelButton";
    public const string PartOkButton = "PART_OkButton";
    public const string PartTitleText = "PART_TitleText";

    public static readonly StyledProperty<int> SelectedHourProperty =
        AvaloniaProperty.Register<TimePickerPane, int>(nameof(SelectedHour),
            defaultBindingMode: BindingMode.TwoWay,
            coerce: static (_, v) => Math.Clamp(v, 0, 23));

    public static readonly StyledProperty<int> SelectedMinuteProperty =
        AvaloniaProperty.Register<TimePickerPane, int>(nameof(SelectedMinute),
            defaultBindingMode: BindingMode.TwoWay,
            coerce: static (_, v) => Math.Clamp(v, 0, 59));

    public static readonly StyledProperty<TimeSpan> SelectedTimeProperty =
        AvaloniaProperty.Register<TimePickerPane, TimeSpan>(nameof(SelectedTime),
            defaultBindingMode: BindingMode.TwoWay,
            coerce: static (_, v) => new TimeSpan(
                Math.Clamp(v.Hours, 0, 23), Math.Clamp(v.Minutes, 0, 59), 0));

    public static readonly StyledProperty<bool> Is24HourProperty =
        AvaloniaProperty.Register<TimePickerPane, bool>(nameof(Is24Hour));

    public static readonly StyledProperty<TimePickerDialMode> ModeProperty =
        AvaloniaProperty.Register<TimePickerPane, TimePickerDialMode>(nameof(Mode));

    public static readonly StyledProperty<bool> IsInputModeProperty =
        AvaloniaProperty.Register<TimePickerPane, bool>(nameof(IsInputMode),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<TimePickerPane, string?>(nameof(Title));

    private ToggleButton? _hourButton;
    private ToggleButton? _minuteButton;
    private ToggleButton? _amButton;
    private ToggleButton? _pmButton;
    private TimePickerDial? _dial;
    private TextBox? _hourTextBox;
    private TextBox? _minuteTextBox;
    private Button? _modeToggleButton;
    private Button? _cancelButton;
    private Button? _okButton;
    private TextBlock? _titleText;
    private bool _updating;
    private bool _synchronizingTime;

    /// <summary>Selected hour, 0-23.</summary>
    public int SelectedHour
    {
        get => GetValue(SelectedHourProperty);
        set => SetValue(SelectedHourProperty, value);
    }

    /// <summary>Selected minute, 0-59.</summary>
    public int SelectedMinute
    {
        get => GetValue(SelectedMinuteProperty);
        set => SetValue(SelectedMinuteProperty, value);
    }

    /// <summary>When true, hides the AM/PM selector and uses a 24-hour dial.</summary>
    public bool Is24Hour
    {
        get => GetValue(Is24HourProperty);
        set => SetValue(Is24HourProperty, value);
    }

    /// <summary>Which unit is currently being edited on the dial.</summary>
    public TimePickerDialMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    /// <summary>
    /// False (default) shows the clock dial ("Select time"); true shows keyboard
    /// input text boxes ("Enter time"), matching the Android M3 time picker modes.
    /// </summary>
    public bool IsInputMode
    {
        get => GetValue(IsInputModeProperty);
        set => SetValue(IsInputModeProperty, value);
    }

    /// <summary>
    /// Pane title. When null, "Select time" or "Enter time" is shown depending on
    /// <see cref="IsInputMode"/>.
    /// </summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Raised whenever the selected time changes.</summary>
    public event EventHandler? SelectedTimeChanged;

    /// <summary>Raised when the Cancel action button is clicked.</summary>
    public event EventHandler? Canceled;

    /// <summary>Raised when the OK action button is clicked.</summary>
    public event EventHandler? Confirmed;

    /// <summary>Selected time as a two-way bindable <see cref="TimeSpan"/>.</summary>
    public TimeSpan SelectedTime
    {
        get => GetValue(SelectedTimeProperty);
        set => SetValue(SelectedTimeProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_hourButton is not null)
            _hourButton.Click -= OnHourClick;
        if (_minuteButton is not null)
            _minuteButton.Click -= OnMinuteClick;
        if (_amButton is not null)
            _amButton.Click -= OnAmClick;
        if (_pmButton is not null)
            _pmButton.Click -= OnPmClick;
        if (_dial is not null)
            _dial.SelectionCommitted -= OnDialCommitted;
        if (_modeToggleButton is not null)
            _modeToggleButton.Click -= OnModeToggleClick;
        if (_cancelButton is not null)
            _cancelButton.Click -= OnCancelClick;
        if (_okButton is not null)
            _okButton.Click -= OnOkClick;
        DetachTextBox(_hourTextBox);
        DetachTextBox(_minuteTextBox);
        if (_hourTextBox is not null)
            _hourTextBox.TextChanged -= OnHourTextChanged;

        _hourButton = e.NameScope.Find<ToggleButton>(PartHourButton);
        _minuteButton = e.NameScope.Find<ToggleButton>(PartMinuteButton);
        _amButton = e.NameScope.Find<ToggleButton>(PartAmButton);
        _pmButton = e.NameScope.Find<ToggleButton>(PartPmButton);
        _dial = e.NameScope.Find<TimePickerDial>(PartDial);
        _hourTextBox = e.NameScope.Find<TextBox>(PartHourTextBox);
        _minuteTextBox = e.NameScope.Find<TextBox>(PartMinuteTextBox);
        _modeToggleButton = e.NameScope.Find<Button>(PartModeToggleButton);
        _cancelButton = e.NameScope.Find<Button>(PartCancelButton);
        _okButton = e.NameScope.Find<Button>(PartOkButton);
        _titleText = e.NameScope.Find<TextBlock>(PartTitleText);

        if (_hourButton is not null)
            _hourButton.Click += OnHourClick;
        if (_minuteButton is not null)
            _minuteButton.Click += OnMinuteClick;
        if (_amButton is not null)
            _amButton.Click += OnAmClick;
        if (_pmButton is not null)
            _pmButton.Click += OnPmClick;
        if (_dial is not null)
            _dial.SelectionCommitted += OnDialCommitted;
        if (_modeToggleButton is not null)
            _modeToggleButton.Click += OnModeToggleClick;
        if (_cancelButton is not null)
            _cancelButton.Click += OnCancelClick;
        if (_okButton is not null)
            _okButton.Click += OnOkClick;
        AttachTextBox(_hourTextBox);
        AttachTextBox(_minuteTextBox);
        if (_hourTextBox is not null)
            _hourTextBox.TextChanged += OnHourTextChanged;

        UpdateDisplay();
    }

    private void AttachTextBox(TextBox? box)
    {
        if (box is null)
            return;
        box.GotFocus += OnTextBoxGotFocus;
        box.LostFocus += OnTextBoxLostFocus;
        box.KeyDown += OnTextBoxKeyDown;
    }

    private void DetachTextBox(TextBox? box)
    {
        if (box is null)
            return;
        box.GotFocus -= OnTextBoxGotFocus;
        box.LostFocus -= OnTextBoxLostFocus;
        box.KeyDown -= OnTextBoxKeyDown;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectedTimeProperty)
        {
            if (_synchronizingTime)
                return;

            _synchronizingTime = true;
            try
            {
                SetCurrentValue(SelectedHourProperty, SelectedTime.Hours);
                SetCurrentValue(SelectedMinuteProperty, SelectedTime.Minutes);
            }
            finally
            {
                _synchronizingTime = false;
            }

            UpdateDisplay();
            SelectedTimeChanged?.Invoke(this, EventArgs.Empty);
        }
        else if (change.Property == SelectedHourProperty
                 || change.Property == SelectedMinuteProperty)
        {
            if (_synchronizingTime)
                return;

            _synchronizingTime = true;
            try
            {
                SetCurrentValue(SelectedTimeProperty, new TimeSpan(SelectedHour, SelectedMinute, 0));
            }
            finally
            {
                _synchronizingTime = false;
            }

            UpdateDisplay();
            SelectedTimeChanged?.Invoke(this, EventArgs.Empty);
        }
        else if (change.Property == ModeProperty
                 || change.Property == Is24HourProperty
                 || change.Property == IsInputModeProperty
                 || change.Property == TitleProperty)
        {
            UpdateDisplay();
        }
    }

    private void OnHourClick(object? sender, RoutedEventArgs e)
    {
        SetCurrentValue(ModeProperty, TimePickerDialMode.Hours);
        UpdateDisplay();
    }

    private void OnMinuteClick(object? sender, RoutedEventArgs e)
    {
        SetCurrentValue(ModeProperty, TimePickerDialMode.Minutes);
        UpdateDisplay();
    }

    private void OnAmClick(object? sender, RoutedEventArgs e)
    {
        if (SelectedHour >= 12)
            SetCurrentValue(SelectedHourProperty, SelectedHour - 12);
        UpdateDisplay();
    }

    private void OnPmClick(object? sender, RoutedEventArgs e)
    {
        if (SelectedHour < 12)
            SetCurrentValue(SelectedHourProperty, SelectedHour + 12);
        UpdateDisplay();
    }

    private void OnDialCommitted(object? sender, EventArgs e)
    {
        // Android behavior: committing an hour advances to minute selection.
        if (Mode == TimePickerDialMode.Hours)
            SetCurrentValue(ModeProperty, TimePickerDialMode.Minutes);
    }

    private void OnModeToggleClick(object? sender, RoutedEventArgs e)
    {
        SetCurrentValue(IsInputModeProperty, !IsInputMode);
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Canceled?.Invoke(this, EventArgs.Empty);
    }

    private void OnOkClick(object? sender, RoutedEventArgs e)
    {
        // Make sure any pending keyboard input is applied before confirming.
        CommitTextBox(_hourTextBox);
        CommitTextBox(_minuteTextBox);
        Confirmed?.Invoke(this, EventArgs.Empty);
    }

    private void OnTextBoxGotFocus(object? sender, RoutedEventArgs e)
    {
        (sender as TextBox)?.SelectAll();
    }

    private void OnTextBoxLostFocus(object? sender, RoutedEventArgs e)
    {
        CommitTextBox(sender as TextBox);
    }

    private void OnTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.Enter or Key.Return)
        {
            CommitTextBox(sender as TextBox);
            e.Handled = true;
        }
    }

    // Optional Android-like nicety: typing two digits in the hour box automatically
    // moves focus to the minute box.
    private void OnHourTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_updating || _hourTextBox is null || _minuteTextBox is null)
            return;
        var text = _hourTextBox.Text;
        if (_hourTextBox.IsFocused && text is { Length: >= 2 } && text.All(char.IsAsciiDigit))
        {
            CommitTextBox(_hourTextBox);
            _minuteTextBox.Focus();
            _minuteTextBox.SelectAll();
        }
    }

    /// <summary>
    /// Parses and applies the value from an input text box. Parseable values are
    /// clamped to the valid range (12h hours 1-12 keep the current AM/PM period);
    /// unparseable input reverts to the current value.
    /// </summary>
    private void CommitTextBox(TextBox? box)
    {
        if (box is null || (box != _hourTextBox && box != _minuteTextBox))
            return;

        if (int.TryParse(box.Text, out var value))
        {
            if (box == _hourTextBox)
            {
                if (Is24Hour)
                {
                    SetCurrentValue(SelectedHourProperty, Math.Clamp(value, 0, 23));
                }
                else
                {
                    // Clamp to 1-12, then map back to 0-23 keeping the AM/PM period.
                    var h12 = Math.Clamp(value, 1, 12) % 12;
                    var isPm = SelectedHour >= 12;
                    SetCurrentValue(SelectedHourProperty, isPm ? h12 + 12 : h12);
                }
            }
            else
            {
                SetCurrentValue(SelectedMinuteProperty, Math.Clamp(value, 0, 59));
            }
        }

        // Reformat valid input / revert invalid input.
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (_updating)
            return;
        _updating = true;
        try
        {
            var hour = SelectedHour;
            var displayHour = Is24Hour
                ? hour
                : hour % 12 == 0 ? 12 : hour % 12;
            var hourText = Is24Hour ? displayHour.ToString("00") : displayHour.ToString();

            if (_titleText is not null)
                _titleText.Text = Title ?? (IsInputMode ? "Enter time" : "Select time");

            if (_hourButton is not null)
            {
                _hourButton.Content = hourText;
                _hourButton.IsChecked = Mode == TimePickerDialMode.Hours;
            }

            if (_minuteButton is not null)
            {
                _minuteButton.Content = SelectedMinute.ToString("00");
                _minuteButton.IsChecked = Mode == TimePickerDialMode.Minutes;
            }

            if (_hourTextBox is not null)
                _hourTextBox.Text = hourText;
            if (_minuteTextBox is not null)
                _minuteTextBox.Text = SelectedMinute.ToString("00");

            if (_amButton is not null)
                _amButton.IsChecked = hour < 12;
            if (_pmButton is not null)
                _pmButton.IsChecked = hour >= 12;
        }
        finally
        {
            _updating = false;
        }
    }
}
