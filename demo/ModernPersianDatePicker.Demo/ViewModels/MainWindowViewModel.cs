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
        BasicDatePicker.SelectedDate = PersianCalendarHelper.Today();
        WeeklyHolidayDatePicker.WeeklyHolidays = new[] { DayOfWeek.Friday, DayOfWeek.Saturday };
        SpecificDatePicker.Holidays = new[]
        {
            new PersianDate(1405, 1, 1, 1), // 1 Farvardin (Nowruz)
            new PersianDate(1405, 1, 13, 6),
        };

        CalendarTypeCommand = new RelayCommand<string>(OnCalendarTypeChanged);
        LanguageCommand = new RelayCommand<string>(OnLanguageChanged);
        LocalizedLanguageCommand = new RelayCommand<string>(OnLocalizedLanguageChanged);
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
    public DatePickerConfig RangeDatePicker { get; } = new() { IsRangeMode = true };
    public DatePickerConfig LocalizedDatePicker { get; } = new();
    public DatePickerConfig GregorianDatePicker { get; } = new() { Language = CalendarLanguage.English };

    public ICommand CalendarTypeCommand { get; }
    public ICommand LanguageCommand { get; }
    public ICommand LocalizedLanguageCommand { get; }
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

    private string _localizedDateText = "Selected: ";
    public string LocalizedDateText { get => _localizedDateText; set { _localizedDateText = value; OnPropertyChanged(); } }

    private string _gregorianDateText = "Selected: ";
    public string GregorianDateText { get => _gregorianDateText; set { _gregorianDateText = value; OnPropertyChanged(); } }

    private string _gregorianCalendarText = "Selected: ";
    public string GregorianCalendarText { get => _gregorianCalendarText; set { _gregorianCalendarText = value; OnPropertyChanged(); } }

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
        LocalizedDatePicker.CalendarType = calType;
        GregorianDatePicker.CalendarType = calType;
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
        LocalizedDatePicker.Language = language;
        GregorianDatePicker.Language = language;
    }

    private void OnLocalizedLanguageChanged(string? lang)
    {
        LocalizedDatePicker.Language = lang switch
        {
            "English" => CalendarLanguage.English,
            "Arabic" => CalendarLanguage.Arabic,
            "Kurdish" => CalendarLanguage.Kurdish,
            _ => CalendarLanguage.Farsi,
        };
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

    private void SetToToday() => CustomDatePicker.SelectedDate = PersianCalendarHelper.Today();
    private void Clear() => CustomDatePicker.SelectedDate = null;

    private void ShowGregorian()
    {
        if (CustomDatePicker.SelectedDate.HasValue)
        {
            var gregorianDate = CustomDatePicker.SelectedDate.Value.ToDateTime();
            GregorianDateText = $"Gregorian: {gregorianDate:yyyy/MM/dd} ({gregorianDate:MMMM dd, yyyy})";
        }
        else
        {
            GregorianDateText = "Gregorian: (no date selected)";
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

    private int _hour;
    public int Hour
    {
        get => _hour;
        set { _hour = value; OnPropertyChanged(); }
    }

    private int _minute;
    public int Minute
    {
        get => _minute;
        set { _minute = value; OnPropertyChanged(); }
    }

    private int _second;
    public int Second
    {
        get => _second;
        set { _second = value; OnPropertyChanged(); }
    }

    private CalendarType _calendarType = CalendarType.Auto;
    public CalendarType CalendarType
    {
        get => _calendarType;
        set { _calendarType = value; OnPropertyChanged(); }
    }

    public string? FlowDirection { get; set; }

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
