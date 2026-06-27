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
        BasicDatePicker.SelectedDate = PersianCalendarHelper.Today();
        WeeklyHolidayDatePicker.WeeklyHolidays = new[] { DayOfWeek.Friday, DayOfWeek.Saturday };
        SpecificDatePicker.Holidays = new[]
        {
            new PersianDate(1405, 1, 1, 1), // 1 Farvardin (Nowruz)
            new PersianDate(1405, 1, 13, 6),
        };
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
    public ToolkitDatePickerConfig RangeDatePicker { get; } = new() { IsRangeMode = true };
    public ToolkitDatePickerConfig LocalizedDatePicker { get; } = new();
    public ToolkitDatePickerConfig GregorianDatePicker { get; } = new() { Language = CalendarLanguage.English };

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
    [ObservableProperty] private string _localizedDateText = "Selected: ";
    [ObservableProperty] private string _gregorianDateText = "Selected: ";
    [ObservableProperty] private string _gregorianCalendarText = "Selected: ";
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
    private void LocalizedLanguage(string? lang)
    {
        LocalizedDatePicker.Language = lang switch
        {
            "English" => CalendarLanguage.English,
            "Arabic" => CalendarLanguage.Arabic,
            "Kurdish" => CalendarLanguage.Kurdish,
            _ => CalendarLanguage.Farsi,
        };
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
    private void SetToToday() => CustomDatePicker.SelectedDate = PersianCalendarHelper.Today();

    [RelayCommand]
    private void Clear() => CustomDatePicker.SelectedDate = null;

    [RelayCommand]
    private void ShowGregorian()
    {
        if (CustomDatePicker.SelectedDate.HasValue)
        {
            var d = CustomDatePicker.SelectedDate.Value.ToDateTime();
            GregorianDateText = $"Gregorian: {d:yyyy/MM/dd} ({d:MMMM dd, yyyy})";
        }
        else
        {
            GregorianDateText = "Gregorian: (no date selected)";
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
        action(LocalizedDatePicker);
        action(GregorianDatePicker);
    }

    public void OnBasicDateChanged(PersianDate? date) => BasicDateText = date.HasValue ? $"Selected: {date.Value.ToString("long")}" : "Selected: (none)";
    public void OnEnglishDateChanged(PersianDate? date) => EnglishDateText = date.HasValue ? $"Selected: {date.Value.ToString("short")}" : "Selected: (none)";
    public void OnCustomDateChanged(PersianDate? date) => CustomDateText = date.HasValue ? $"Selected: {date.Value.ToString("long")}" : "Selected: (none)";
    public void OnAccentDateChanged(PersianDate? date) => AccentDateText = date.HasValue ? $"Selected: {date.Value.ToString("long")}" : "Selected: (none)";
    public void OnEditableDateChanged(PersianDate? date) => EditableDateText = date.HasValue ? $"Selected: {date.Value.ToString("long")}" : "Selected: (none)";
    public void OnThemedDateChanged(PersianDate? date) => ThemedDateText = date.HasValue ? $"Selected: {date.Value.ToString("long")}" : "Selected: (none)";
    public void OnWeeklyHolidayDateChanged(PersianDate? date) => WeeklyHolidayText = date.HasValue ? $"Selected: {date.Value.ToString("long")}" : "Selected: (none)";
    public void OnSpecificHolidayDateChanged(PersianDate? date) => SpecificHolidayText = date.HasValue ? $"Selected: {date.Value.ToString("long")}" : "Selected: (none)";
    public void OnCustomHolidayBrushDateChanged(PersianDate? date) => CustomHolidayBrushText = date.HasValue ? $"Selected: {date.Value.ToString("long")}" : "Selected: (none)";

    public void OnTimePickerDateChanged(PersianDate? date)
    {
        var dp = TimePickerDatePicker;
        TimePickerDateText = date.HasValue
            ? $"Selected: {date.Value.ToString("long")} {dp.Hour:D2}:{dp.Minute:D2}:{dp.Second:D2}"
            : "Selected: (none)";
    }

    public void OnRangeDateSelected(PersianDate? start, PersianDate? end)
    {
        if (start.HasValue && end.HasValue)
            RangeDateText = $"Range: {start.Value.ToString("short")} ~ {end.Value.ToString("short")}";
        else if (start.HasValue)
            RangeDateText = $"Start: {start.Value.ToString("short")} (click end date)";
        else
            RangeDateText = "Selected: (none)";
    }

    public void OnLocalizedDateChanged(PersianDate? date) => LocalizedDateText = date.HasValue ? $"Selected: {date.Value.ToString("long")}" : "Selected: (none)";

    public void OnGregorianDateChanged(PersianDate? date)
    {
        if (date.HasValue)
        {
            var d = date.Value.ToDateTime();
            GregorianDateText = $"Selected: {d:yyyy/MM/dd} ({d:dddd, MMMM dd, yyyy})";
        }
        else
        {
            GregorianDateText = "Selected: (none)";
        }
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
    [ObservableProperty] private int _hour;
    [ObservableProperty] private int _minute;
    [ObservableProperty] private int _second;
    [ObservableProperty] private CalendarType _calendarType = CalendarType.Auto;
    [ObservableProperty] private string? _flowDirection;

    public event Action<ToolkitDatePickerConfig, PersianDate?>? SelectedDateChanged;

    partial void OnSelectedDateChanged(PersianDate? value)
    {
        SelectedDateChanged?.Invoke(this, value);
    }
}
