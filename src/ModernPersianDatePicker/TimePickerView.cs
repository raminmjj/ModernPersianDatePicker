using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace ModernPersianDatePicker;

/// <summary>
/// Time picker view with +/- spinners for Hour, Minute, and Second.
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

    private TextBlock? _hourText;
    private TextBlock? _minuteText;
    private TextBlock? _secondText;
    private RepeatButton? _hourUp;
    private RepeatButton? _hourDown;
    private RepeatButton? _minuteUp;
    private RepeatButton? _minuteDown;
    private RepeatButton? _secondUp;
    private RepeatButton? _secondDown;

    static TimePickerView()
    {
        HourProperty.Changed.AddClassHandler<TimePickerView>((x, _) => x.UpdateDisplay());
        MinuteProperty.Changed.AddClassHandler<TimePickerView>((x, _) => x.UpdateDisplay());
        SecondProperty.Changed.AddClassHandler<TimePickerView>((x, _) => x.UpdateDisplay());
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
        DetachButtons();
        base.OnApplyTemplate(e);

        _hourText = e.NameScope.Find<TextBlock>("PART_HourText");
        _minuteText = e.NameScope.Find<TextBlock>("PART_MinuteText");
        _secondText = e.NameScope.Find<TextBlock>("PART_SecondText");

        _hourUp = e.NameScope.Find<RepeatButton>("PART_HourUp");
        _hourDown = e.NameScope.Find<RepeatButton>("PART_HourDown");
        _minuteUp = e.NameScope.Find<RepeatButton>("PART_MinuteUp");
        _minuteDown = e.NameScope.Find<RepeatButton>("PART_MinuteDown");
        _secondUp = e.NameScope.Find<RepeatButton>("PART_SecondUp");
        _secondDown = e.NameScope.Find<RepeatButton>("PART_SecondDown");

        AttachButtons();
        UpdateDisplay();
    }

    private void DetachButtons()
    {
        if (_hourUp != null) _hourUp.Click -= OnHourUp_Click;
        if (_hourDown != null) _hourDown.Click -= OnHourDown_Click;
        if (_minuteUp != null) _minuteUp.Click -= OnMinuteUp_Click;
        if (_minuteDown != null) _minuteDown.Click -= OnMinuteDown_Click;
        if (_secondUp != null) _secondUp.Click -= OnSecondUp_Click;
        if (_secondDown != null) _secondDown.Click -= OnSecondDown_Click;
    }

    private void AttachButtons()
    {
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

    private void UpdateDisplay()
    {
        if (_hourText != null) _hourText.Text = Hour.ToString("D2");
        if (_minuteText != null) _minuteText.Text = Minute.ToString("D2");
        if (_secondText != null) _secondText.Text = Second.ToString("D2");

        TimeChanged?.Invoke(this, EventArgs.Empty);
    }
}
