using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace ModernPersianDatePicker;

/// <summary>
/// Time picker view with Hour/Minute/Second spinners for selecting a time component.
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

    private NumericUpDown? _hourSpinner;
    private NumericUpDown? _minuteSpinner;
    private NumericUpDown? _secondSpinner;

    static TimePickerView()
    {
        HourProperty.Changed.AddClassHandler<TimePickerView>((x, _) => x.OnTimeComponentChanged());
        MinuteProperty.Changed.AddClassHandler<TimePickerView>((x, _) => x.OnTimeComponentChanged());
        SecondProperty.Changed.AddClassHandler<TimePickerView>((x, _) => x.OnTimeComponentChanged());
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
        if (_hourSpinner != null)
            _hourSpinner.ValueChanged -= OnSpinner_ValueChanged;
        if (_minuteSpinner != null)
            _minuteSpinner.ValueChanged -= OnSpinner_ValueChanged;
        if (_secondSpinner != null)
            _secondSpinner.ValueChanged -= OnSpinner_ValueChanged;

        _hourSpinner = e.NameScope.Find<NumericUpDown>("PART_HourSpinner");
        _minuteSpinner = e.NameScope.Find<NumericUpDown>("PART_MinuteSpinner");
        _secondSpinner = e.NameScope.Find<NumericUpDown>("PART_SecondSpinner");

        if (_hourSpinner != null)
        {
            _hourSpinner.Value = Hour;
            _hourSpinner.ValueChanged += OnSpinner_ValueChanged;
        }
        if (_minuteSpinner != null)
        {
            _minuteSpinner.Value = Minute;
            _minuteSpinner.ValueChanged += OnSpinner_ValueChanged;
        }
        if (_secondSpinner != null)
        {
            _secondSpinner.Value = Second;
            _secondSpinner.ValueChanged += OnSpinner_ValueChanged;
        }
    }

    private void OnSpinner_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (_hourSpinner != null && _hourSpinner.Value.HasValue)
            Hour = (int)_hourSpinner.Value.Value;
        if (_minuteSpinner != null && _minuteSpinner.Value.HasValue)
            Minute = (int)_minuteSpinner.Value.Value;
        if (_secondSpinner != null && _secondSpinner.Value.HasValue)
            Second = (int)_secondSpinner.Value.Value;
    }

    private void OnTimeComponentChanged()
    {
        if (_hourSpinner != null)
            _hourSpinner.Value = Hour;
        if (_minuteSpinner != null)
            _minuteSpinner.Value = Minute;
        if (_secondSpinner != null)
            _secondSpinner.Value = Second;

        TimeChanged?.Invoke(this, EventArgs.Empty);
    }
}
