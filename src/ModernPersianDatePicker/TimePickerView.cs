using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace ModernPersianDatePicker;

/// <summary>
/// Time picker view with +/- buttons and editable TextBoxes for Hour, Minute, and Second.
/// Displayed inside the DatePicker popup when <see cref="ModernPersianDatePicker.IsTimePickerEnabled"/> is true.
/// The selected time-of-day is exposed as a single <see cref="Time"/> property of type <see cref="TimeSpan"/>.
/// </summary>
public class TimePickerView : TemplatedControl
{
    /// <summary>
    /// Selected time-of-day. Defaults to 00:00:00. Values are clamped to the
    /// range [00:00:00, 23:59:59]; the days component of the input TimeSpan is ignored.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> TimeProperty =
        AvaloniaProperty.Register<TimePickerView, TimeSpan>(nameof(Time), TimeSpan.Zero,
            validate: t => t >= TimeSpan.Zero && t < TimeSpan.FromDays(1));

    public event EventHandler? TimeChanged;

    private TextBox? _hourBox;
    private TextBox? _minuteBox;
    private TextBox? _secondBox;
    private RepeatButton? _hourUp;
    private RepeatButton? _hourDown;
    private RepeatButton? _minuteUp;
    private RepeatButton? _minuteDown;
    private RepeatButton? _secondUp;
    private RepeatButton? _secondDown;
    private bool _isUpdatingText;

    static TimePickerView()
    {
        TimeProperty.Changed.AddClassHandler<TimePickerView>((x, _) => x.SyncFromValue());
    }

    /// <summary>
    /// Selected time-of-day. Always in the range [00:00:00, 23:59:59].
    /// </summary>
    public TimeSpan Time
    {
        get => GetValue(TimeProperty);
        set => SetValue(TimeProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        DetachAll();
        base.OnApplyTemplate(e);

        _hourBox = e.NameScope.Find<TextBox>("PART_HourBox");
        _minuteBox = e.NameScope.Find<TextBox>("PART_MinuteBox");
        _secondBox = e.NameScope.Find<TextBox>("PART_SecondBox");

        _hourUp = e.NameScope.Find<RepeatButton>("PART_HourUp");
        _hourDown = e.NameScope.Find<RepeatButton>("PART_HourDown");
        _minuteUp = e.NameScope.Find<RepeatButton>("PART_MinuteUp");
        _minuteDown = e.NameScope.Find<RepeatButton>("PART_MinuteDown");
        _secondUp = e.NameScope.Find<RepeatButton>("PART_SecondUp");
        _secondDown = e.NameScope.Find<RepeatButton>("PART_SecondDown");

        AttachAll();
        SyncFromValue();

        // Block wheel events on all elements except text boxes.
        // Text boxes handle their own wheel events (changing values).
        // Everything else (buttons, ":" separators) should block propagation.
        AttachRepeatButtonWheelHandlers();
        AttachSeparatorWheelHandlers();
    }

    private void AttachRepeatButtonWheelHandlers()
    {
        if (_hourUp != null) _hourUp.PointerWheelChanged += OnBlockWheel;
        if (_hourDown != null) _hourDown.PointerWheelChanged += OnBlockWheel;
        if (_minuteUp != null) _minuteUp.PointerWheelChanged += OnBlockWheel;
        if (_minuteDown != null) _minuteDown.PointerWheelChanged += OnBlockWheel;
        if (_secondUp != null) _secondUp.PointerWheelChanged += OnBlockWheel;
        if (_secondDown != null) _secondDown.PointerWheelChanged += OnBlockWheel;
    }

    private void AttachSeparatorWheelHandlers()
    {
        // Find the ":" TextBlocks by traversing the visual tree
        foreach (var child in VisualChildren)
        {
            if (child is Panel panel)
            {
                foreach (var panelChild in panel.Children)
                {
                    if (panelChild is TextBlock tb && tb.Text == ":")
                        tb.PointerWheelChanged += OnBlockWheel;
                }
            }
        }
    }

    private void DetachAll()
    {
        if (_hourBox != null) { _hourBox.LostFocus -= OnBoxLostFocus; _hourBox.PointerWheelChanged -= OnBoxPointerWheel; }
        if (_minuteBox != null) { _minuteBox.LostFocus -= OnBoxLostFocus; _minuteBox.PointerWheelChanged -= OnBoxPointerWheel; }
        if (_secondBox != null) { _secondBox.LostFocus -= OnBoxLostFocus; _secondBox.PointerWheelChanged -= OnBoxPointerWheel; }
        if (_hourUp != null) { _hourUp.Click -= OnHourUp_Click; _hourUp.PointerWheelChanged -= OnBlockWheel; }
        if (_hourDown != null) { _hourDown.Click -= OnHourDown_Click; _hourDown.PointerWheelChanged -= OnBlockWheel; }
        if (_minuteUp != null) { _minuteUp.Click -= OnMinuteUp_Click; _minuteUp.PointerWheelChanged -= OnBlockWheel; }
        if (_minuteDown != null) { _minuteDown.Click -= OnMinuteDown_Click; _minuteDown.PointerWheelChanged -= OnBlockWheel; }
        if (_secondUp != null) { _secondUp.Click -= OnSecondUp_Click; _secondUp.PointerWheelChanged -= OnBlockWheel; }
        if (_secondDown != null) { _secondDown.Click -= OnSecondDown_Click; _secondDown.PointerWheelChanged -= OnBlockWheel; }
    }

    private void AttachAll()
    {
        if (_hourBox != null) { _hourBox.LostFocus += OnBoxLostFocus; _hourBox.PointerWheelChanged += OnBoxPointerWheel; }
        if (_minuteBox != null) { _minuteBox.LostFocus += OnBoxLostFocus; _minuteBox.PointerWheelChanged += OnBoxPointerWheel; }
        if (_secondBox != null) { _secondBox.LostFocus += OnBoxLostFocus; _secondBox.PointerWheelChanged += OnBoxPointerWheel; }
        if (_hourUp != null) _hourUp.Click += OnHourUp_Click;
        if (_hourDown != null) _hourDown.Click += OnHourDown_Click;
        if (_minuteUp != null) _minuteUp.Click += OnMinuteUp_Click;
        if (_minuteDown != null) _minuteDown.Click += OnMinuteDown_Click;
        if (_secondUp != null) _secondUp.Click += OnSecondUp_Click;
        if (_secondDown != null) _secondDown.Click += OnSecondDown_Click;
    }

    private void OnBoxPointerWheel(object? sender, PointerWheelEventArgs e)
    {
        int delta = e.Delta.Y > 0 ? 1 : -1;

        if (sender == _hourBox)
            Time = WrapComponent(Time, hoursDelta: delta, hoursMod: 24);
        else if (sender == _minuteBox)
            Time = WrapComponent(Time, minutesDelta: delta, minutesMod: 60);
        else if (sender == _secondBox)
            Time = WrapComponent(Time, secondsDelta: delta, secondsMod: 60);

        e.Handled = true;
    }

    private void OnBlockWheel(object? sender, PointerWheelEventArgs e)
    {
        e.Handled = true;
    }

    private void OnHourUp_Click(object? sender, RoutedEventArgs e) => Time = WrapComponent(Time, hoursDelta: 1, hoursMod: 24);
    private void OnHourDown_Click(object? sender, RoutedEventArgs e) => Time = WrapComponent(Time, hoursDelta: -1, hoursMod: 24);
    private void OnMinuteUp_Click(object? sender, RoutedEventArgs e) => Time = WrapComponent(Time, minutesDelta: 1, minutesMod: 60);
    private void OnMinuteDown_Click(object? sender, RoutedEventArgs e) => Time = WrapComponent(Time, minutesDelta: -1, minutesMod: 60);
    private void OnSecondUp_Click(object? sender, RoutedEventArgs e) => Time = WrapComponent(Time, secondsDelta: 1, secondsMod: 60);
    private void OnSecondDown_Click(object? sender, RoutedEventArgs e) => Time = WrapComponent(Time, secondsDelta: -1, secondsMod: 60);

    private void OnBoxLostFocus(object? sender, RoutedEventArgs e)
    {
        if (_isUpdatingText) return;

        if (sender == _hourBox && _hourBox != null)
            Time = ReplaceHour(Time, ClampParsed(_hourBox.Text, 0, 23));
        else if (sender == _minuteBox && _minuteBox != null)
            Time = ReplaceMinute(Time, ClampParsed(_minuteBox.Text, 0, 59));
        else if (sender == _secondBox && _secondBox != null)
            Time = ReplaceSecond(Time, ClampParsed(_secondBox.Text, 0, 59));
    }

    private static int ClampParsed(string? text, int min, int max)
    {
        if (int.TryParse(text, out int val))
            return Math.Clamp(val, min, max);
        return min;
    }

    /// <summary>
    /// Wraps a single time component (hour/minute/second) by the given delta,
    /// keeping it inside its natural modulus (24h, 60m, 60s) without ever
    /// leaving the [00:00:00, 23:59:59] range.
    /// </summary>
    private static TimeSpan WrapComponent(TimeSpan t,
        int hoursDelta = 0, int hoursMod = 0,
        int minutesDelta = 0, int minutesMod = 0,
        int secondsDelta = 0, int secondsMod = 0)
    {
        int h = t.Hours;
        int m = t.Minutes;
        int s = t.Seconds;

        if (hoursMod != 0)
            h = ((h + hoursDelta) % hoursMod + hoursMod) % hoursMod;
        if (minutesMod != 0)
            m = ((m + minutesDelta) % minutesMod + minutesMod) % minutesMod;
        if (secondsMod != 0)
            s = ((s + secondsDelta) % secondsMod + secondsMod) % secondsMod;

        return new TimeSpan(h, m, s);
    }

    private static TimeSpan ReplaceHour(TimeSpan t, int hour) => new TimeSpan(hour, t.Minutes, t.Seconds);
    private static TimeSpan ReplaceMinute(TimeSpan t, int minute) => new TimeSpan(t.Hours, minute, t.Seconds);
    private static TimeSpan ReplaceSecond(TimeSpan t, int second) => new TimeSpan(t.Hours, t.Minutes, second);

    private void SyncFromValue()
    {
        _isUpdatingText = true;
        var t = Time;
        if (_hourBox != null) _hourBox.Text = t.Hours.ToString("D2");
        if (_minuteBox != null) _minuteBox.Text = t.Minutes.ToString("D2");
        if (_secondBox != null) _secondBox.Text = t.Seconds.ToString("D2");
        _isUpdatingText = false;

        TimeChanged?.Invoke(this, EventArgs.Empty);
    }
}
