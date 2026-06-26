using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace ModernPersianDatePicker;

/// <summary>
/// Time picker view with +/- buttons and editable TextBoxes for Hour, Minute, and Second.
/// Displayed inside the DatePicker popup when <see cref="ModernPersianDatePicker.IsTimePickerEnabled"/> is true.
/// </summary>
public class TimePickerView : TemplatedControl
{
    public static readonly StyledProperty<int> HourProperty =
        AvaloniaProperty.Register<TimePickerView, int>(nameof(Hour),
            validate: v => v >= 0 && v <= 23);

    public static readonly StyledProperty<int> MinuteProperty =
        AvaloniaProperty.Register<TimePickerView, int>(nameof(Minute),
            validate: v => v >= 0 && v <= 59);

    public static readonly StyledProperty<int> SecondProperty =
        AvaloniaProperty.Register<TimePickerView, int>(nameof(Second),
            validate: v => v >= 0 && v <= 59);

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
        HourProperty.Changed.AddClassHandler<TimePickerView>((x, _) => x.SyncFromValue());
        MinuteProperty.Changed.AddClassHandler<TimePickerView>((x, _) => x.SyncFromValue());
        SecondProperty.Changed.AddClassHandler<TimePickerView>((x, _) => x.SyncFromValue());
    }

    public int Hour
    {
        get => GetValue(HourProperty);
        set => SetValue(HourProperty, value);
    }

    public int Minute
    {
        get => GetValue(MinuteProperty);
        set => SetValue(MinuteProperty, value);
    }

    public int Second
    {
        get => GetValue(SecondProperty);
        set => SetValue(SecondProperty, value);
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
    }

    private void DetachAll()
    {
        if (_hourBox != null) _hourBox.LostFocus -= OnBoxLostFocus;
        if (_minuteBox != null) _minuteBox.LostFocus -= OnBoxLostFocus;
        if (_secondBox != null) _secondBox.LostFocus -= OnBoxLostFocus;
        if (_hourUp != null) _hourUp.Click -= OnHourUp_Click;
        if (_hourDown != null) _hourDown.Click -= OnHourDown_Click;
        if (_minuteUp != null) _minuteUp.Click -= OnMinuteUp_Click;
        if (_minuteDown != null) _minuteDown.Click -= OnMinuteDown_Click;
        if (_secondUp != null) _secondUp.Click -= OnSecondUp_Click;
        if (_secondDown != null) _secondDown.Click -= OnSecondDown_Click;
    }

    private void AttachAll()
    {
        if (_hourBox != null) _hourBox.LostFocus += OnBoxLostFocus;
        if (_minuteBox != null) _minuteBox.LostFocus += OnBoxLostFocus;
        if (_secondBox != null) _secondBox.LostFocus += OnBoxLostFocus;
        if (_hourUp != null) _hourUp.Click += OnHourUp_Click;
        if (_hourDown != null) _hourDown.Click += OnHourDown_Click;
        if (_minuteUp != null) _minuteUp.Click += OnMinuteUp_Click;
        if (_minuteDown != null) _minuteDown.Click += OnMinuteDown_Click;
        if (_secondUp != null) _secondUp.Click += OnSecondUp_Click;
        if (_secondDown != null) _secondDown.Click += OnSecondDown_Click;
    }

    private void OnHourUp_Click(object? sender, RoutedEventArgs e) => Hour = (Hour + 1) % 24;
    private void OnHourDown_Click(object? sender, RoutedEventArgs e) => Hour = (Hour + 23) % 24;
    private void OnMinuteUp_Click(object? sender, RoutedEventArgs e) => Minute = (Minute + 1) % 60;
    private void OnMinuteDown_Click(object? sender, RoutedEventArgs e) => Minute = (Minute + 59) % 60;
    private void OnSecondUp_Click(object? sender, RoutedEventArgs e) => Second = (Second + 1) % 60;
    private void OnSecondDown_Click(object? sender, RoutedEventArgs e) => Second = (Second + 59) % 60;

    private void OnBoxLostFocus(object? sender, RoutedEventArgs e)
    {
        if (_isUpdatingText) return;

        if (sender == _hourBox && _hourBox != null)
            Hour = ClampParsed(_hourBox.Text, 0, 23);
        else if (sender == _minuteBox && _minuteBox != null)
            Minute = ClampParsed(_minuteBox.Text, 0, 59);
        else if (sender == _secondBox && _secondBox != null)
            Second = ClampParsed(_secondBox.Text, 0, 59);
    }

    private static int ClampParsed(string? text, int min, int max)
    {
        if (int.TryParse(text, out int val))
            return Math.Clamp(val, min, max);
        return min;
    }

    private void SyncFromValue()
    {
        _isUpdatingText = true;
        if (_hourBox != null) _hourBox.Text = Hour.ToString("D2");
        if (_minuteBox != null) _minuteBox.Text = Minute.ToString("D2");
        if (_secondBox != null) _secondBox.Text = Second.ToString("D2");
        _isUpdatingText = false;

        TimeChanged?.Invoke(this, EventArgs.Empty);
    }
}
