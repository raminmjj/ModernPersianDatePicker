using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;

namespace ModernPersianDatePicker.Demo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Subscribe to date changed events
        BasicDatePicker.SelectedDateChanged += OnBasicDateChanged;
        EnglishDatePicker.SelectedDateChanged += OnEnglishDateChanged;
        CustomDatePicker.SelectedDateChanged += OnCustomDateChanged;
        GreenAccentDatePicker.SelectedDateChanged += OnAccentDateChanged;
        PurpleAccentDatePicker.SelectedDateChanged += OnAccentDateChanged;
        EditableDatePicker.SelectedDateChanged += OnEditableDateChanged;
        ThemedDatePicker.SelectedDateChanged += OnThemedDateChanged;
        WeeklyHolidayDatePicker.SelectedDateChanged += OnWeeklyHolidayDateChanged;
        SpecificHolidayDatePicker.SelectedDateChanged += OnSpecificHolidayDateChanged;
        CustomHolidayBrushDatePicker.SelectedDateChanged += OnCustomHolidayBrushDateChanged;
        TimePickerDatePicker.SelectedDateChanged += OnTimePickerDateChanged;
        RangeDatePicker.DateRangeSelected += OnRangeDateSelected;

        // Set default date for basic picker
        BasicDatePicker.SelectedDate = PersianCalendarHelper.Today();

        // Configure specific date holidays
        SpecificHolidayDatePicker.Holidays = new[]
        {
            new PersianDate(1405, 1, 1, 1),  // 1 Farvardin (Nowruz)
            new PersianDate(1405, 1, 13, 6), // 13 Farvardin
        };
    }

    private void OnBasicDateChanged(object? sender, SelectedDateChangedEventArgs e)
    {
        if (e.NewDate.HasValue)
        {
            BasicDateText.Text = $"Selected: {e.NewDate.Value.ToString("long")}";
        }
        else
        {
            BasicDateText.Text = "Selected: (none)";
        }
    }

    private void OnEnglishDateChanged(object? sender, SelectedDateChangedEventArgs e)
    {
        if (e.NewDate.HasValue)
        {
            EnglishDateText.Text = $"Selected: {e.NewDate.Value.ToString("short")}";
        }
        else
        {
            EnglishDateText.Text = "Selected: (none)";
        }
    }

    private void OnCustomDateChanged(object? sender, SelectedDateChangedEventArgs e)
    {
        if (e.NewDate.HasValue)
        {
            CustomDateText.Text = $"Selected: {e.NewDate.Value.ToString("long")}";
        }
        else
        {
            CustomDateText.Text = "Selected: (none)";
        }
    }

    private void OnAccentDateChanged(object? sender, SelectedDateChangedEventArgs e)
    {
        if (e.NewDate.HasValue)
        {
            AccentDateText.Text = $"Selected: {e.NewDate.Value.ToString("long")}";
        }
        else
        {
            AccentDateText.Text = "Selected: (none)";
        }
    }

    private void OnSetToTodayClick(object? sender, RoutedEventArgs e)
    {
        CustomDatePicker.SetToToday();
    }

    private void OnClearClick(object? sender, RoutedEventArgs e)
    {
        CustomDatePicker.Clear();
    }

    private void OnShowGregorianClick(object? sender, RoutedEventArgs e)
    {
        if (CustomDatePicker.SelectedDate.HasValue)
        {
            var gregorianDate = CustomDatePicker.SelectedDate.Value.ToDateTime();
            GregorianDateText.Text = $"Gregorian: {gregorianDate:yyyy/MM/dd} ({gregorianDate:MMMM dd, yyyy})";
        }
        else
        {
            GregorianDateText.Text = "Gregorian: (no date selected)";
        }
    }

    private void OnEditableDateChanged(object? sender, SelectedDateChangedEventArgs e)
    {
        EditableDateText.Text = FormatDateForDisplay(e.NewDate, EditableDatePicker.CalendarType);
    }

    private string FormatDateForDisplay(PersianDate? date, CalendarType calendarType)
    {
        if (!date.HasValue) return "Selected: (none)";

        var effectiveType = calendarType;
        if (effectiveType == CalendarType.Auto)
        {
            var culture = System.Globalization.CultureInfo.CurrentUICulture;
            effectiveType = culture.TwoLetterISOLanguageName is "fa" or "ps"
                ? CalendarType.Persian : CalendarType.Gregorian;
        }

        if (effectiveType == CalendarType.Gregorian)
        {
            var d = date.Value.ToDateTime();
            return $"Selected: {d:yyyy/MM/dd} ({d:dddd, MMMM dd, yyyy})";
        }
        return $"Selected: {date.Value.ToString("long")}";
    }

    private void OnInvalidActionChanged(object? sender, RoutedEventArgs e)
    {
        if (RbSetToNull.IsChecked == true)
            EditableDatePicker.InvalidValueAction = InvalidValueAction.SetToNull;
        else if (RbSetToToday.IsChecked == true)
            EditableDatePicker.InvalidValueAction = InvalidValueAction.SetToToday;
        else if (RbKeep.IsChecked == true)
            EditableDatePicker.InvalidValueAction = InvalidValueAction.Keep;
    }

    private void OnThemeModeChanged(object? sender, RoutedEventArgs e)
    {
        if (Application.Current == null) return;

        if (RbThemeSystem.IsChecked == true)
            Application.Current.RequestedThemeVariant = ThemeVariant.Default;
        else if (RbThemeLight.IsChecked == true)
            Application.Current.RequestedThemeVariant = ThemeVariant.Light;
        else if (RbThemeDark.IsChecked == true)
            Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
    }

    private void OnThemedDateChanged(object? sender, SelectedDateChangedEventArgs e)
    {
        ThemedDateText.Text = e.NewDate.HasValue
            ? $"Selected: {e.NewDate.Value.ToString("long")}"
            : "Selected: (none)";
    }

    private void OnWeeklyHolidayDateChanged(object? sender, SelectedDateChangedEventArgs e)
    {
        WeeklyHolidayText.Text = e.NewDate.HasValue
            ? $"Selected: {e.NewDate.Value.ToString("long")}"
            : "Selected: (none)";
    }

    private void OnSpecificHolidayDateChanged(object? sender, SelectedDateChangedEventArgs e)
    {
        SpecificHolidayText.Text = e.NewDate.HasValue
            ? $"Selected: {e.NewDate.Value.ToString("long")}"
            : "Selected: (none)";
    }

    private void OnCustomHolidayBrushDateChanged(object? sender, SelectedDateChangedEventArgs e)
    {
        CustomHolidayBrushText.Text = e.NewDate.HasValue
            ? $"Selected: {e.NewDate.Value.ToString("long")}"
            : "Selected: (none)";
    }

    private void OnTimePickerDateChanged(object? sender, SelectedDateChangedEventArgs e)
    {
        if (e.NewDate.HasValue)
        {
            var dp = TimePickerDatePicker;
            TimePickerDateText.Text = $"Selected: {e.NewDate.Value.ToString("long")} {dp.Hour:D2}:{dp.Minute:D2}:{dp.Second:D2}";
        }
        else
        {
            TimePickerDateText.Text = "Selected: (none)";
        }
    }

    private void OnRangeDateSelected(object? sender, DateRangeSelectedEventArgs e)
    {
        if (e.RangeStart.HasValue && e.RangeEnd.HasValue)
        {
            RangeDateText.Text = $"Range: {e.RangeStart.Value.ToString("short")} ~ {e.RangeEnd.Value.ToString("short")}";
        }
        else if (e.RangeStart.HasValue)
        {
            RangeDateText.Text = $"Start: {e.RangeStart.Value.ToString("short")} (click end date)";
        }
        else
        {
            RangeDateText.Text = "Selected: (none)";
        }
    }

    private void OnLanguageChanged(object? sender, RoutedEventArgs e)
    {
        CalendarLanguage lang;
        if (RbLangFarsi.IsChecked == true)
            lang = CalendarLanguage.Farsi;
        else if (RbLangEnglish.IsChecked == true)
            lang = CalendarLanguage.English;
        else if (RbLangArabic.IsChecked == true)
            lang = CalendarLanguage.Arabic;
        else if (RbLangKurdish.IsChecked == true)
            lang = CalendarLanguage.Kurdish;
        else
            lang = CalendarLanguage.Farsi;

        BasicDatePicker.Language = lang;
        EnglishDatePicker.Language = lang;
        CustomDatePicker.Language = lang;
        GreenAccentDatePicker.Language = lang;
        PurpleAccentDatePicker.Language = lang;
        EditableDatePicker.Language = lang;
        ThemedDatePicker.Language = lang;
        WeeklyHolidayDatePicker.Language = lang;
        SpecificHolidayDatePicker.Language = lang;
        CustomHolidayBrushDatePicker.Language = lang;
        TimePickerDatePicker.Language = lang;
        RangeDatePicker.Language = lang;
    }

    private void OnCalendarTypeDemoChanged(object? sender, RoutedEventArgs e)
    {
        CalendarType type;
        if (RbCalPersian.IsChecked == true)
            type = CalendarType.Persian;
        else if (RbCalGregorian.IsChecked == true)
            type = CalendarType.Gregorian;
        else
            type = CalendarType.Auto;

        BasicDatePicker.CalendarType = type;
        EnglishDatePicker.CalendarType = type;
        CustomDatePicker.CalendarType = type;
        GreenAccentDatePicker.CalendarType = type;
        PurpleAccentDatePicker.CalendarType = type;
        EditableDatePicker.CalendarType = type;
        ThemedDatePicker.CalendarType = type;
        WeeklyHolidayDatePicker.CalendarType = type;
        SpecificHolidayDatePicker.CalendarType = type;
        CustomHolidayBrushDatePicker.CalendarType = type;
        TimePickerDatePicker.CalendarType = type;
        RangeDatePicker.CalendarType = type;

        // Holiday settings are now automatically managed by the control
        // based on CalendarType (Persian=Friday, Gregorian=Saturday+Sunday)
    }
}
