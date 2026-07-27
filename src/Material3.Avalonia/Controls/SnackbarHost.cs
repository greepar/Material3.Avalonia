// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;

namespace Material3.Avalonia.Controls;

/// <summary>
/// Hosts transient <see cref="Snackbar"/>s over the user interface placed in
/// <see cref="ContentControl.Content"/>. Call <see cref="Show"/> to display a message;
/// a new call replaces the current snackbar, and the snackbar auto-dismisses after the
/// given duration (4 seconds by default).
/// </summary>
[TemplatePart(PartSnackbarSlot, typeof(ContentControl))]
public class SnackbarHost : ContentControl
{
    public const string PartSnackbarSlot = "PART_SnackbarSlot";

    private ContentControl? _slot;
    private DispatcherTimer? _timer;
    private Action? _onAction;
    private Snackbar? _current;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _slot = e.NameScope.Find<ContentControl>(PartSnackbarSlot);
    }

    /// <summary>
    /// Shows a snackbar with the given message, replacing any currently shown snackbar.
    /// </summary>
    /// <param name="message">The message text.</param>
    /// <param name="actionText">Optional action label; when set, an action button is shown.</param>
    /// <param name="duration">Auto-dismiss delay; defaults to 4 seconds.</param>
    /// <param name="onAction">Callback invoked when the action button is clicked.</param>
    public void Show(string message, string? actionText = null, TimeSpan? duration = null, Action? onAction = null)
    {
        if (_slot is null)
        {
            return;
        }

        _timer?.Stop();
        _timer = null;

        if (_current is not null)
        {
            _current.ActionClicked -= OnSnackbarAction;
        }

        _onAction = onAction;
        _current = new Snackbar { Message = message, ActionText = actionText };
        _current.ActionClicked += OnSnackbarAction;

        _slot.Content = _current;
        _slot.Classes.Set("open", true);

        _timer = new DispatcherTimer { Interval = duration ?? TimeSpan.FromSeconds(4) };
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        _timer?.Stop();
        _timer = null;
        Dismiss();
    }

    private void OnSnackbarAction(object? sender, EventArgs e)
    {
        _onAction?.Invoke();
        _timer?.Stop();
        _timer = null;
        Dismiss();
    }

    private void Dismiss()
    {
        if (_current is not null)
        {
            _current.ActionClicked -= OnSnackbarAction;
            _current = null;
        }

        _onAction = null;
        if (_slot is not null)
        {
            _slot.Classes.Set("open", false);
            _slot.Content = null;
        }
    }
}
