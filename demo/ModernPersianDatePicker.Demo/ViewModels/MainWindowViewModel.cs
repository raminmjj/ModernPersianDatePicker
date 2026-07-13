using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Styling;

namespace ModernPersianDatePicker.Demo.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public MainWindowViewModel()
    {
        // Respect the active calendar type (Auto → Gregorian on non-fa/ps cultures).
        BasicDatePicker.SelectedDate = PersianCalendarHelper.TodayForCalendarType(BasicDatePicker.CalendarType);
        WeeklyHolidayDatePicker.WeeklyHolidays = new[] { DayOfWeek.Friday, DayOfWeek.Saturday };
        // Two mid-week days (Tue & Wed) of the current month in the active calendar.
        SpecificDatePicker.Holidays = PersianCalendarHelper.GetMidWeekDaysOfCurrentMonth(SpecificDatePicker.CalendarType);

        CalendarTypeCommand = new RelayCommand<string>(OnCalendarTypeChanged);
        LanguageCommand = new RelayCommand<string>(OnLanguageChanged);
        ThemeCommand = new RelayCommand<string>(OnThemeChanged);
        SetToTodayCommand = new RelayCommand(SetToToday);
        ClearCommand = new RelayCommand(Clear);
        ShowGregorianCommand = new RelayCommand(ShowGregorian);
    }

    public DatePickerConfig BasicDatePicker { get; } = new();
    public DatePickerConfig EnglishDatePicker { get; } = new() { Language = CalendarLanguage.English, FlowDirection = "LTR", DisplayFormat = "short" };
    public DatePickerConfig CustomDatePicker { get; } = new() { DisplayFormat = "long" };
    public DatePickerConfig GreenAccentDatePicker { get; } = new() { AccentBrush = "#FF4CAF50" };
    public DatePickerConfig PurpleAccentDatePicker { get; } = new() { AccentBrush = "#FF9C27B0" };
    public DatePickerConfig EditableDatePicker { get; } = new() { IsEditable = true };
    public DatePickerConfig ThemedDatePicker { get; } = new() { Theme = "System" };
    public DatePickerConfig WeeklyHolidayDatePicker { get; } = new();
    public DatePickerConfig SpecificDatePicker { get; } = new();
    public DatePickerConfig CustomHolidayBrushDatePicker { get; } = new() { HolidayBrush = "#FFFF8800" };
    public DatePickerConfig TimePickerDatePicker { get; } = new() { IsTimePickerEnabled = true };
    public DatePickerConfig RangeDatePicker { get; } = new() { IsRangeMode = true, IsTimePickerEnabled = true };

    public ICommand CalendarTypeCommand { get; }
    public ICommand LanguageCommand { get; }
    public ICommand ThemeCommand { get; }
    public ICommand SetToTodayCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand ShowGregorianCommand { get; }

    private string _basicDateText = "Selected: ";
    public string BasicDateText { get => _basicDateText; set { _basicDateText = value; OnPropertyChanged(); } }

    private string _englishDateText = "Selected: ";
    public string EnglishDateText { get => _englishDateText; set { _englishDateText = value; OnPropertyChanged(); } }

    private string _customDateText = "Selected: ";
    public string CustomDateText { get => _customDateText; set { _customDateText = value; OnPropertyChanged(); } }

    private string _accentDateText = "Selected: ";
    public string AccentDateText { get => _accentDateText; set { _accentDateText = value; OnPropertyChanged(); } }

    private string _editableDateText = "Selected: ";
    public string EditableDateText { get => _editableDateText; set { _editableDateText = value; OnPropertyChanged(); } }

    private string _themedDateText = "Selected: ";
    public string ThemedDateText { get => _themedDateText; set { _themedDateText = value; OnPropertyChanged(); } }

    private string _weeklyHolidayText = "Selected: ";
    public string WeeklyHolidayText { get => _weeklyHolidayText; set { _weeklyHolidayText = value; OnPropertyChanged(); } }

    private string _specificHolidayText = "Selected: ";
    public string SpecificHolidayText { get => _specificHolidayText; set { _specificHolidayText = value; OnPropertyChanged(); } }

    private string _customHolidayBrushText = "Selected: ";
    public string CustomHolidayBrushText { get => _customHolidayBrushText; set { _customHolidayBrushText = value; OnPropertyChanged(); } }

    private string _timePickerDateText = "Selected: ";
    public string TimePickerDateText { get => _timePickerDateText; set { _timePickerDateText = value; OnPropertyChanged(); } }

    private string _rangeDateText = "Selected: ";
    public string RangeDateText { get => _rangeDateText; set { _rangeDateText = value; OnPropertyChanged(); } }

    private string _invalidAction = "SetToNull";
    public string InvalidAction
    {
        get => _invalidAction;
        set
        {
            _invalidAction = value;
            OnPropertyChanged();
            EditableDatePicker.InvalidValueAction = value switch
            {
                "SetToToday" => InvalidValueAction.SetToToday,
                "Keep" => InvalidValueAction.Keep,
                _ => InvalidValueAction.SetToNull,
            };
        }
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

    private void OnCalendarTypeChanged(string? type)
    {
        CalendarType calType = type switch
        {
            "Persian" => CalendarType.Persian,
            "Gregorian" => CalendarType.Gregorian,
            _ => CalendarType.Auto,
        };

        BasicDatePicker.CalendarType = calType;
        EnglishDatePicker.CalendarType = calType;
        CustomDatePicker.CalendarType = calType;
        GreenAccentDatePicker.CalendarType = calType;
        PurpleAccentDatePicker.CalendarType = calType;
        EditableDatePicker.CalendarType = calType;
        ThemedDatePicker.CalendarType = calType;
        WeeklyHolidayDatePicker.CalendarType = calType;
        SpecificDatePicker.CalendarType = calType;
        CustomHolidayBrushDatePicker.CalendarType = calType;
        TimePickerDatePicker.CalendarType = calType;
        RangeDatePicker.CalendarType = calType;

        // Holiday settings are now automatically managed by the control
        // based on CalendarType (Persian=Friday, Gregorian=Saturday+Sunday)
    }

    private void OnLanguageChanged(string? lang)
    {
        CalendarLanguage language = lang switch
        {
            "English" => CalendarLanguage.English,
            "Arabic" => CalendarLanguage.Arabic,
            "Kurdish" => CalendarLanguage.Kurdish,
            _ => CalendarLanguage.Farsi,
        };

        BasicDatePicker.Language = language;
        EnglishDatePicker.Language = language;
        CustomDatePicker.Language = language;
        GreenAccentDatePicker.Language = language;
        PurpleAccentDatePicker.Language = language;
        EditableDatePicker.Language = language;
        ThemedDatePicker.Language = language;
        WeeklyHolidayDatePicker.Language = language;
        SpecificDatePicker.Language = language;
        CustomHolidayBrushDatePicker.Language = language;
        TimePickerDatePicker.Language = language;
        RangeDatePicker.Language = language;
    }

    private void OnThemeChanged(string? theme)
    {
        if (Application.Current == null) return;

        Application.Current.RequestedThemeVariant = theme switch
        {
            "Light" => ThemeVariant.Light,
            "Dark" => ThemeVariant.Dark,
            _ => ThemeVariant.Default,
        };
    }

    private void SetToToday() => CustomDatePicker.SelectedDate = PersianCalendarHelper.TodayForCalendarType(CustomDatePicker.CalendarType);
    private void Clear() => CustomDatePicker.SelectedDate = null;

    private void ShowGregorian()
    {
        if (CustomDatePicker.SelectedDate.HasValue)
        {
            var pd = CustomDatePicker.SelectedDate.Value;
            var gregorianDate = CustomDatePicker.CalendarType == CalendarType.Gregorian
                ? pd.ToDateTimeAsGregorian()
                : pd.ToDateTime();
            CustomDateText = $"Gregorian: {gregorianDate:yyyy/MM/dd} ({gregorianDate:MMMM dd, yyyy})";
        }
        else
        {
            CustomDateText = "Gregorian: (no date selected)";
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class DatePickerConfig : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private PersianDate? _selectedDate;
    public PersianDate? SelectedDate
    {
        get => _selectedDate;
        set { _selectedDate = value; OnPropertyChanged(); SelectedDateChanged?.Invoke(this, value); }
    }

    public event Action<DatePickerConfig, PersianDate?>? SelectedDateChanged;

    private string _displayFormat = "long";
    public string DisplayFormat { get => _displayFormat; set { _displayFormat = value; OnPropertyChanged(); } }

    private CalendarLanguage _language = CalendarLanguage.Farsi;
    public CalendarLanguage Language { get => _language; set { _language = value; OnPropertyChanged(); } }

    private bool _isEditable;
    public bool IsEditable { get => _isEditable; set { _isEditable = value; OnPropertyChanged(); } }

    private InvalidValueAction _invalidValueAction = InvalidValueAction.SetToNull;
    public InvalidValueAction InvalidValueAction { get => _invalidValueAction; set { _invalidValueAction = value; OnPropertyChanged(); } }

    private string? _watermark;
    public string? Watermark { get => _watermark; set { _watermark = value; OnPropertyChanged(); } }

    private string? _accentBrush;
    public string? AccentBrush { get => _accentBrush; set { _accentBrush = value; OnPropertyChanged(); } }

    private string? _theme;
    public string? Theme { get => _theme; set { _theme = value; OnPropertyChanged(); } }

    private string? _holidayBrush;
    public string? HolidayBrush { get => _holidayBrush; set { _holidayBrush = value; OnPropertyChanged(); } }

    private IReadOnlyList<DayOfWeek>? _weeklyHolidays;
    public IReadOnlyList<DayOfWeek>? WeeklyHolidays { get => _weeklyHolidays; set { _weeklyHolidays = value; OnPropertyChanged(); } }

    private IReadOnlyList<PersianDate>? _holidays;
    public IReadOnlyList<PersianDate>? Holidays { get => _holidays; set { _holidays = value; OnPropertyChanged(); } }

    private bool _isRangeMode;
    public bool IsRangeMode { get => _isRangeMode; set { _isRangeMode = value; OnPropertyChanged(); } }

    private bool _isTimePickerEnabled;
    public bool IsTimePickerEnabled { get => _isTimePickerEnabled; set { _isTimePickerEnabled = value; OnPropertyChanged(); } }

    private PersianDate? _rangeStart;
    public PersianDate? RangeStart
    {
        get => _rangeStart;
        set { _rangeStart = value; OnPropertyChanged(); }
    }

    private PersianDate? _rangeEnd;
    public PersianDate? RangeEnd
    {
        get => _rangeEnd;
        set { _rangeEnd = value; OnPropertyChanged(); }
    }

    private TimeSpan _selectedTime = TimeSpan.Zero;
    public TimeSpan SelectedTime
    {
        get => _selectedTime;
        set { _selectedTime = value; OnPropertyChanged(); SelectedTimeChanged?.Invoke(this, value); }
    }

    public event Action<DatePickerConfig, TimeSpan>? SelectedTimeChanged;

    private TimeSpan _rangeStartTime = TimeSpan.Zero;
    public TimeSpan RangeStartTime
    {
        get => _rangeStartTime;
        set { _rangeStartTime = value; OnPropertyChanged(); RangeTimeChanged?.Invoke(this); }
    }

    private TimeSpan _rangeEndTime = new(23, 59, 59);
    public TimeSpan RangeEndTime
    {
        get => _rangeEndTime;
        set { _rangeEndTime = value; OnPropertyChanged(); RangeTimeChanged?.Invoke(this); }
    }

    public event Action<DatePickerConfig>? RangeTimeChanged;

    private CalendarType _calendarType = CalendarType.Auto;
    public CalendarType CalendarType
    {
        get => _calendarType;
        set { _calendarType = value; OnPropertyChanged(); }
    }

    public string? FlowDirection { get; set; }

    /// <summary>
    /// Formats a date using the active calendar's day/month names and the picker's
    /// resolved localization. Mirrors ModernPersianDatePicker.FormatSelectedDate for
    /// the manual MVVM code path where the view-model only has the config object.
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

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class RelayCommand : ICommand
{
    private readonly Action _execute;
    public RelayCommand(Action execute) => _execute = execute;
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => _execute();
}

public class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    public RelayCommand(Action<T?> execute) => _execute = execute;
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => _execute(parameter is T t ? t : default);
}
