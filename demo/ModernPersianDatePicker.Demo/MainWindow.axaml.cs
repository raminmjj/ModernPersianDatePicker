using System;
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
        TimePickerDatePicker.SelectedTimeChanged += OnTimePickerTimeChanged;
        RangeDatePicker.DateRangeSelected += OnRangeDateSelected;
        RangeDatePicker.RangeTimeChanged += OnRangeTimeChanged;

        // Set default date for basic picker — respect the active calendar type
        // (Auto resolves to Gregorian on non-fa/ps system cultures).
        BasicDatePicker.SelectedDate = PersianCalendarHelper.TodayForCalendarType(BasicDatePicker.CalendarType);

        // Configure specific date holidays — two mid-week days (Tue & Wed) of the
        // current month in the active calendar, so the example always highlights
        // something visible regardless of the system culture.
        SpecificHolidayDatePicker.Holidays = PersianCalendarHelper.GetMidWeekDaysOfCurrentMonth(
            SpecificHolidayDatePicker.CalendarType);
    }

    private void OnBasicDateChanged(object? sender, SelectedDateChangedEventArgs e)
    {
        BasicDateText.Text = e.NewDate.HasValue
            ? $"Selected: {BasicDatePicker.FormatSelectedDate("long")}"
            : "Selected: (none)";
    }

    private void OnEnglishDateChanged(object? sender, SelectedDateChangedEventArgs e)
    {
        EnglishDateText.Text = e.NewDate.HasValue
            ? $"Selected: {EnglishDatePicker.FormatSelectedDate("short")}"
            : "Selected: (none)";
    }

    private void OnCustomDateChanged(object? sender, SelectedDateChangedEventArgs e)
    {
        CustomDateText.Text = e.NewDate.HasValue
            ? $"Selected: {CustomDatePicker.FormatSelectedDate("long")}"
            : "Selected: (none)";
    }

    private void OnAccentDateChanged(object? sender, SelectedDateChangedEventArgs e)
    {
        var dp = (ModernPersianDatePicker)sender!;
        AccentDateText.Text = e.NewDate.HasValue
            ? $"Selected: {dp.FormatSelectedDate("long")}"
            : "Selected: (none)";
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
            // Use ToDateTimeAsGregorian when the picker is in Gregorian mode
            // (avoids PersianCalendar rejecting e.g. July 31st).
            var d = CustomDatePicker.SelectedDate.Value;
            var gregorianDate = CustomDatePicker.CalendarType == CalendarType.Gregorian
                ? d.ToDateTimeAsGregorian()
                : d.ToDateTime();
            GregorianDateText.Text = $"Gregorian: {gregorianDate:yyyy/MM/dd} ({gregorianDate:MMMM dd, yyyy})";
        }
        else
        {
            GregorianDateText.Text = "Gregorian: (no date selected)";
        }
    }

    private void OnEditableDateChanged(object? sender, SelectedDateChangedEventArgs e)
    {
        EditableDateText.Text = e.NewDate.HasValue
            ? $"Selected: {EditableDatePicker.FormatSelectedDate("long")}"
            : "Selected: (none)";
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
            ? $"Selected: {ThemedDatePicker.FormatSelectedDate("long")}"
            : "Selected: (none)";
    }

    private void OnWeeklyHolidayDateChanged(object? sender, SelectedDateChangedEventArgs e)
    {
        WeeklyHolidayText.Text = e.NewDate.HasValue
            ? $"Selected: {WeeklyHolidayDatePicker.FormatSelectedDate("long")}"
            : "Selected: (none)";
    }

    private void OnSpecificHolidayDateChanged(object? sender, SelectedDateChangedEventArgs e)
    {
        SpecificHolidayText.Text = e.NewDate.HasValue
            ? $"Selected: {SpecificHolidayDatePicker.FormatSelectedDate("long")}"
            : "Selected: (none)";
    }

    private void OnCustomHolidayBrushDateChanged(object? sender, SelectedDateChangedEventArgs e)
    {
        CustomHolidayBrushText.Text = e.NewDate.HasValue
            ? $"Selected: {CustomHolidayBrushDatePicker.FormatSelectedDate("long")}"
            : "Selected: (none)";
    }

    private void OnTimePickerDateChanged(object? sender, SelectedDateChangedEventArgs e)
        => RefreshTimePickerText();

    private void OnTimePickerTimeChanged(object? sender, SelectedTimeChangedEventArgs e)
        => RefreshTimePickerText();

    private void RefreshTimePickerText()
    {
        var dp = TimePickerDatePicker;
        if (dp.SelectedDate.HasValue)
            TimePickerDateText.Text = $"Selected: {dp.FormatSelectedDate("long")} {dp.SelectedTime:hh\\:mm\\:ss}";
        else
            TimePickerDateText.Text = "Selected: (none)";
    }

    private void OnRangeDateSelected(object? sender, DateRangeSelectedEventArgs e)
        => RefreshRangeText();

    private void OnRangeTimeChanged(object? sender, EventArgs e)
        => RefreshRangeText();

    private void RefreshRangeText()
    {
        var dp = RangeDatePicker;
        if (dp.RangeStart.HasValue && dp.RangeEnd.HasValue)
        {
            var start = dp.FormatRangeStart("short");
            var end = dp.FormatRangeEnd("short");
            if (dp.IsTimePickerEnabled)
            {
                var st = dp.RangeStartTime;
                var et = dp.RangeEndTime;
                RangeDateText.Text = $"Range: {start} {st.Hours:D2}:{st.Minutes:D2}:{st.Seconds:D2}" +
                    $" ~ {end} {et.Hours:D2}:{et.Minutes:D2}:{et.Seconds:D2}";
            }
            else
            {
                RangeDateText.Text = $"Range: {start} ~ {end}";
            }
        }
        else if (dp.RangeStart.HasValue)
        {
            RangeDateText.Text = $"Start: {dp.FormatRangeStart("short")} (click end date)";
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
