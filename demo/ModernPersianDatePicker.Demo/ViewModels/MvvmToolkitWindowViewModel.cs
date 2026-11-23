using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ModernPersianDatePicker.Demo.ViewModels;

public partial class MvvmToolkitWindowViewModel : ObservableObject
{
    public MvvmToolkitWindowViewModel()
    {
        // Respect the active calendar type (Auto → Gregorian on non-fa/ps cultures).
        BasicDatePicker.SelectedDate = PersianCalendarHelper.TodayForCalendarType(BasicDatePicker.CalendarType);
        WeeklyHolidayDatePicker.WeeklyHolidays = new[] { DayOfWeek.Friday, DayOfWeek.Saturday };
        // Two mid-week days (Tue & Wed) of the current month in the active calendar.
        SpecificDatePicker.Holidays = PersianCalendarHelper.GetMidWeekDaysOfCurrentMonth(SpecificDatePicker.CalendarType);
    }

    public ToolkitDatePickerConfig BasicDatePicker { get; } = new();
    public ToolkitDatePickerConfig EnglishDatePicker { get; } = new() { Language = CalendarLanguage.English, FlowDirection = "LTR", DisplayFormat = "short" };
    public ToolkitDatePickerConfig CustomDatePicker { get; } = new() { DisplayFormat = "long" };
    public ToolkitDatePickerConfig GreenAccentDatePicker { get; } = new() { AccentBrush = "#FF4CAF50" };
    public ToolkitDatePickerConfig PurpleAccentDatePicker { get; } = new() { AccentBrush = "#FF9C27B0" };
    public ToolkitDatePickerConfig EditableDatePicker { get; } = new() { IsEditable = true };
    public ToolkitDatePickerConfig ThemedDatePicker { get; } = new() { Theme = "System" };
    public ToolkitDatePickerConfig WeeklyHolidayDatePicker { get; } = new();
    public ToolkitDatePickerConfig SpecificDatePicker { get; } = new();
    public ToolkitDatePickerConfig CustomHolidayBrushDatePicker { get; } = new() { HolidayBrush = "#FFFF8800" };
    public ToolkitDatePickerConfig TimePickerDatePicker { get; } = new() { IsTimePickerEnabled = true };
    public ToolkitDatePickerConfig RangeDatePicker { get; } = new() { IsRangeMode = true, IsTimePickerEnabled = true };

    [ObservableProperty] private string _basicDateText = "Selected: ";
    [ObservableProperty] private string _englishDateText = "Selected: ";
    [ObservableProperty] private string _customDateText = "Selected: ";
    [ObservableProperty] private string _accentDateText = "Selected: ";
    [ObservableProperty] private string _editableDateText = "Selected: ";
    [ObservableProperty] private string _themedDateText = "Selected: ";
    [ObservableProperty] private string _weeklyHolidayText = "Selected: ";
    [ObservableProperty] private string _specificHolidayText = "Selected: ";
    [ObservableProperty] private string _customHolidayBrushText = "Selected: ";
    [ObservableProperty] private string _timePickerDateText = "Selected: ";
    [ObservableProperty] private string _rangeDateText = "Selected: ";
    [ObservableProperty] private string _invalidAction = "SetToNull";

    [RelayCommand]
    private void ChangeCalendarType(string? type)
    {
        var calType = type switch
        {
            "Persian" => CalendarType.Persian,
            "Gregorian" => CalendarType.Gregorian,
            _ => CalendarType.Auto,
        };
        ApplyToAll(c => c.CalendarType = calType);

        // Holiday settings are now automatically managed by the control
        // based on CalendarType (Persian=Friday, Gregorian=Saturday+Sunday)
    }

    [RelayCommand]
    private void ChangeLanguage(string? lang)
    {
        var language = lang switch
        {
            "English" => CalendarLanguage.English,
            "Arabic" => CalendarLanguage.Arabic,
            "Kurdish" => CalendarLanguage.Kurdish,
            _ => CalendarLanguage.Farsi,
        };
        ApplyToAll(c => c.Language = language);
    }

    [RelayCommand]
    private void Theme(string? theme)
    {
        if (Application.Current == null) return;
        Application.Current.RequestedThemeVariant = theme switch
        {
            "Light" => ThemeVariant.Light,
            "Dark" => ThemeVariant.Dark,
            _ => ThemeVariant.Default,
        };
    }

    [RelayCommand]
    private void SetToToday() => CustomDatePicker.SelectedDate = PersianCalendarHelper.TodayForCalendarType(CustomDatePicker.CalendarType);

    [RelayCommand]
    private void Clear() => CustomDatePicker.SelectedDate = null;

    [RelayCommand]
    private void ShowGregorian()
    {
        if (CustomDatePicker.SelectedDate.HasValue)
        {
            var pd = CustomDatePicker.SelectedDate.Value;
            var d = CustomDatePicker.CalendarType == CalendarType.Gregorian
                ? pd.ToDateTimeAsGregorian()
                : pd.ToDateTime();
            CustomDateText = $"Gregorian: {d:yyyy/MM/dd} ({d:MMMM dd, yyyy})";
        }
        else
        {
            CustomDateText = "Gregorian: (no date selected)";
        }
    }

    partial void OnInvalidActionChanged(string value)
    {
        EditableDatePicker.InvalidValueAction = value switch
        {
            "SetToToday" => InvalidValueAction.SetToToday,
            "Keep" => InvalidValueAction.Keep,
            _ => InvalidValueAction.SetToNull,
        };
    }

    private void ApplyToAll(Action<ToolkitDatePickerConfig> action)
    {
        action(BasicDatePicker);
        action(EnglishDatePicker);
        action(CustomDatePicker);
        action(GreenAccentDatePicker);
        action(PurpleAccentDatePicker);
        action(EditableDatePicker);
        action(ThemedDatePicker);
        action(WeeklyHolidayDatePicker);
        action(SpecificDatePicker);
        action(CustomHolidayBrushDatePicker);
        action(TimePickerDatePicker);
        action(RangeDatePicker);
    }

    public void OnBasicDateChanged(PersianDate? date) => BasicDateText = date.HasValue ? $"Selected: {BasicDatePicker.FormatForDisplay(date.Value, "long")}" : "Selected: (none)";
    public void OnEnglishDateChanged(PersianDate? date) => EnglishDateText = date.HasValue ? $"Selected: {EnglishDatePicker.FormatForDisplay(date.Value, "short")}" : "Selected: (none)";
    public void OnCustomDateChanged(PersianDate? date) => CustomDateText = date.HasValue ? $"Selected: {CustomDatePicker.FormatForDisplay(date.Value, "long")}" : "Selected: (none)";
    public void OnAccentDateChanged(PersianDate? date) => AccentDateText = date.HasValue ? $"Selected: {GreenAccentDatePicker.FormatForDisplay(date.Value, "long")}" : "Selected: (none)";
    public void OnEditableDateChanged(PersianDate? date) => EditableDateText = date.HasValue ? $"Selected: {EditableDatePicker.FormatForDisplay(date.Value, "long")}" : "Selected: (none)";
    public void OnThemedDateChanged(PersianDate? date) => ThemedDateText = date.HasValue ? $"Selected: {ThemedDatePicker.FormatForDisplay(date.Value, "long")}" : "Selected: (none)";
    public void OnWeeklyHolidayDateChanged(PersianDate? date) => WeeklyHolidayText = date.HasValue ? $"Selected: {WeeklyHolidayDatePicker.FormatForDisplay(date.Value, "long")}" : "Selected: (none)";
    public void OnSpecificHolidayDateChanged(PersianDate? date) => SpecificHolidayText = date.HasValue ? $"Selected: {SpecificDatePicker.FormatForDisplay(date.Value, "long")}" : "Selected: (none)";
    public void OnCustomHolidayBrushDateChanged(PersianDate? date) => CustomHolidayBrushText = date.HasValue ? $"Selected: {CustomHolidayBrushDatePicker.FormatForDisplay(date.Value, "long")}" : "Selected: (none)";

    public void OnTimePickerDateChanged(PersianDate? date)
    {
        var dp = TimePickerDatePicker;
        TimePickerDateText = date.HasValue
            ? $"Selected: {dp.FormatForDisplay(date.Value, "long")} {dp.SelectedTime:hh\\:mm\\:ss}"
            : "Selected: (none)";
    }

    public void OnRangeDateSelected(PersianDate? start, PersianDate? end,
        TimeSpan startTime, TimeSpan endTime)
    {
        if (start.HasValue && end.HasValue)
        {
            var dp = RangeDatePicker;
            string s = dp.FormatForDisplay(start.Value, "short");
            string e = dp.FormatForDisplay(end.Value, "short");
            if (dp.IsTimePickerEnabled)
            {
                RangeDateText = $"Range: {s} {startTime.Hours:D2}:{startTime.Minutes:D2}:{startTime.Seconds:D2}" +
                    $" ~ {e} {endTime.Hours:D2}:{endTime.Minutes:D2}:{endTime.Seconds:D2}";
            }
            else
            {
                RangeDateText = $"Range: {s} ~ {e}";
            }
        }
        else if (start.HasValue)
            RangeDateText = $"Start: {RangeDatePicker.FormatForDisplay(start.Value, "short")} (click end date)";
        else
            RangeDateText = "Selected: (none)";
    }
}

public partial class ToolkitDatePickerConfig : ObservableObject
{
    [ObservableProperty] private PersianDate? _selectedDate;
    [ObservableProperty] private string _displayFormat = "long";
    [ObservableProperty] private CalendarLanguage _language = CalendarLanguage.Farsi;
    [ObservableProperty] private bool _isEditable;
    [ObservableProperty] private InvalidValueAction _invalidValueAction = InvalidValueAction.SetToNull;
    [ObservableProperty] private string? _watermark;
    [ObservableProperty] private string? _accentBrush;
    [ObservableProperty] private string? _theme;
    [ObservableProperty] private string? _holidayBrush;
    [ObservableProperty] private IReadOnlyList<DayOfWeek>? _weeklyHolidays;
    [ObservableProperty] private IReadOnlyList<PersianDate>? _holidays;
    [ObservableProperty] private bool _isRangeMode;
    [ObservableProperty] private bool _isTimePickerEnabled;
    [ObservableProperty] private PersianDate? _rangeStart;
    [ObservableProperty] private PersianDate? _rangeEnd;
    [ObservableProperty] private TimeSpan _selectedTime = TimeSpan.Zero;
    [ObservableProperty] private TimeSpan _rangeStartTime = TimeSpan.Zero;
    [ObservableProperty] private TimeSpan _rangeEndTime = new(23, 59, 59);
    [ObservableProperty] private CalendarType _calendarType = CalendarType.Auto;
    [ObservableProperty] private string? _flowDirection;

    /// <summary>
    /// Formats a date using the active calendar's day/month names and the picker's
    /// resolved localization. Mirrors ModernPersianDatePicker.FormatSelectedDate for the
    /// MVVM-toolkit code path where the view-model only has the config object, not the
    /// live control instance.
    /// </summary>
    public string FormatForDisplay(PersianDate date, string format = "long")
    {
        CalendarType effectiveType = CalendarType;
        if (effectiveType == CalendarType.Auto)
        {
            var culture = System.Globalization.CultureInfo.CurrentUICulture;
            effectiveType = culture.TwoLetterISOLanguageName is "fa" or "ps"
                ? CalendarType.Persian : CalendarType.Gregorian;
        }

        ICalendarLocalization loc = Language switch
        {
            CalendarLanguage.English => new Localizations.EnglishLocalization(),
            CalendarLanguage.Arabic => new Localizations.ArabicLocalization(),
            CalendarLanguage.Kurdish => new Localizations.KurdishLocalization(),
            _ => new Localizations.FarsiLocalization()
        };

        return PersianCalendarHelper.FormatPersianDate(date, format, loc, effectiveType);
    }

    public event Action<ToolkitDatePickerConfig, PersianDate?>? SelectedDateChanged;
    public event Action<ToolkitDatePickerConfig, TimeSpan>? SelectedTimeChanged;
    public event Action<ToolkitDatePickerConfig, PersianDate?, PersianDate?>? DateRangeSelected;
    public event Action<ToolkitDatePickerConfig>? RangeTimeChanged;

    partial void OnSelectedDateChanged(PersianDate? value)
    {
        SelectedDateChanged?.Invoke(this, value);
    }

    partial void OnSelectedTimeChanged(TimeSpan value)
    {
        SelectedTimeChanged?.Invoke(this, value);
    }

    partial void OnRangeStartChanged(PersianDate? value)
    {
        DateRangeSelected?.Invoke(this, value, RangeEnd);
    }

    partial void OnRangeEndChanged(PersianDate? value)
    {
        DateRangeSelected?.Invoke(this, RangeStart, value);
    }

    partial void OnRangeStartTimeChanged(TimeSpan value) => RangeTimeChanged?.Invoke(this);
    partial void OnRangeEndTimeChanged(TimeSpan value) => RangeTimeChanged?.Invoke(this);
}
