using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using ModernPersianDatePicker;

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

        // Set default date for basic picker
        BasicDatePicker.SelectedDate = PersianCalendarHelper.Today();

        // Configure weekly holidays: Friday + Saturday
        WeeklyHolidayDatePicker.WeeklyHolidays = new[] { System.DayOfWeek.Friday, System.DayOfWeek.Saturday };

        // Configure specific date holidays
        SpecificHolidayDatePicker.Holidays = new[]
        {
            new PersianDate(1405, 1, 1, 1),  // 1 Farvardin (Nowruz)
            new PersianDate(1404, 10, 18, 6), // 18 Dey
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
        if (e.NewDate.HasValue)
        {
            EditableDateText.Text = $"Selected: {e.NewDate.Value.ToString("long")}";
        }
        else
        {
            EditableDateText.Text = "Selected: (none)";
        }
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
}
