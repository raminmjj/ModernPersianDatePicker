using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace ModernPersianDatePicker;

/// <summary>
/// Calendar view control for displaying Persian calendar months
/// </summary>
public class CalendarView : TemplatedControl
{
    // Styled Properties
    public static readonly StyledProperty<int> DisplayYearProperty =
        AvaloniaProperty.Register<CalendarView, int>(nameof(DisplayYear));

    public static readonly StyledProperty<int> DisplayMonthProperty =
        AvaloniaProperty.Register<CalendarView, int>(nameof(DisplayMonth));

    public static readonly StyledProperty<PersianDate?> SelectedDateProperty =
        AvaloniaProperty.Register<CalendarView, PersianDate?>(nameof(SelectedDate));

    public static readonly StyledProperty<bool> UseEnglishNamesProperty =
        AvaloniaProperty.Register<CalendarView, bool>(nameof(UseEnglishNames));

    // Events
    public event EventHandler<DateSelectedEventArgs>? DateSelected;
    public event EventHandler<TodayClickedEventArgs>? TodayClicked;

    // Private fields
    private TextBlock? _monthText;
    private TextBlock? _yearText;
    private TextBlock? _todayText;
    private Grid? _daysGrid;
    private Grid? _monthGrid;
    private Grid? _yearGrid;
    private Button? _previousButton;
    private Button? _nextButton;
    private Button? _monthButton;
    private Button? _yearButton;
    private Button? _todayButton;
    private Popup? _monthPopup;
    private Popup? _yearPopup;
    private bool _isUpdatingCalendar;

    static CalendarView()
    {
        DisplayYearProperty.Changed.AddClassHandler<CalendarView>((x, e) => x.OnDisplayYearMonthChanged(e));
        DisplayMonthProperty.Changed.AddClassHandler<CalendarView>((x, e) => x.OnDisplayYearMonthChanged(e));
        UseEnglishNamesProperty.Changed.AddClassHandler<CalendarView>((x, e) => x.OnDisplayYearMonthChanged(e));
        SelectedDateProperty.Changed.AddClassHandler<CalendarView>((x, e) => x.OnSelectedDateChanged(e));
    }

    public CalendarView()
    {
        var today = PersianCalendarHelper.Today();
        DisplayYear = today.Year;
        DisplayMonth = today.Month;
        _focusedDay = today.Day;
        
        // Make control focusable
        Focusable = true;
    }

    private void OnDisplayYearMonthChanged(AvaloniaPropertyChangedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"Property changed: {e.Property.Name}, isUpdating: {_isUpdatingCalendar}");
        
        // Only update calendar if the property actually changed
        if (e.Property == DisplayYearProperty || e.Property == DisplayMonthProperty || e.Property == UseEnglishNamesProperty)
        {
            UpdateCalendar();
            System.Diagnostics.Debug.WriteLine($"Calendar updated for {e.Property.Name}");
        }
    }

    private void OnSelectedDateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        // Update the visual selection without rebuilding the entire calendar
        UpdateSelectedVisual();
    }

    private void UpdateSelectedVisual()
    {
        if (_daysGrid == null) return;

        // Remove 'selected' class from all day buttons
        foreach (var child in _daysGrid.Children)
        {
            if (child is Button button)
            {
                button.Classes.Remove("selected");
            }
        }

        // Add 'selected' class to the button matching SelectedDate
        if (SelectedDate.HasValue && 
            SelectedDate.Value.Year == DisplayYear && 
            SelectedDate.Value.Month == DisplayMonth)
        {
            foreach (var child in _daysGrid.Children)
            {
                if (child is Button button && 
                    int.TryParse(button.Content?.ToString(), out int day) &&
                    day == SelectedDate.Value.Day)
                {
                    button.Classes.Add("selected");
                    break;
                }
            }
        }
        
        // Also update focused day visual
        UpdateFocusedDay();
    }

    // Public Properties
    public int DisplayYear
    {
        get => GetValue(DisplayYearProperty);
        set => SetValue(DisplayYearProperty, value);
    }

    public int DisplayMonth
    {
        get => GetValue(DisplayMonthProperty);
        set => SetValue(DisplayMonthProperty, value);
    }

    public PersianDate? SelectedDate
    {
        get => GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    public bool UseEnglishNames
    {
        get => GetValue(UseEnglishNamesProperty);
        set => SetValue(UseEnglishNamesProperty, value);
    }

    // Private field for keyboard navigation
    private int _focusedDay = 1;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        System.Diagnostics.Debug.WriteLine("OnApplyTemplate called");

        // Detach previous event handlers
        if (_previousButton != null)
            _previousButton.Click -= OnPreviousButton_Click;
        if (_nextButton != null)
            _nextButton.Click -= OnNextButton_Click;
        if (_monthButton != null)
            _monthButton.Click -= OnMonthButton_Click;
        if (_yearButton != null)
            _yearButton.Click -= OnYearButton_Click;
        if (_todayButton != null)
            _todayButton.Click -= OnTodayButton_Click;

        // Get template parts
        _monthText = e.NameScope.Find<TextBlock>("PART_MonthText");
        _yearText = e.NameScope.Find<TextBlock>("PART_YearText");
        _todayText = e.NameScope.Find<TextBlock>("PART_TodayText");
        _daysGrid = e.NameScope.Find<Grid>("PART_DaysGrid");
        _monthGrid = e.NameScope.Find<Grid>("PART_MonthGrid");
        _yearGrid = e.NameScope.Find<Grid>("PART_YearGrid");
        _previousButton = e.NameScope.Find<Button>("PART_PreviousButton");
        _nextButton = e.NameScope.Find<Button>("PART_NextButton");
        _monthButton = e.NameScope.Find<Button>("PART_MonthButton");
        _yearButton = e.NameScope.Find<Button>("PART_YearButton");
        _todayButton = e.NameScope.Find<Button>("PART_TodayButton");
        _monthPopup = e.NameScope.Find<Popup>("PART_MonthPopup");
        _yearPopup = e.NameScope.Find<Popup>("PART_YearPopup");

        System.Diagnostics.Debug.WriteLine($"Template parts found: month={_monthButton != null}, year={_yearButton != null}, today={_todayButton != null}");

        // Attach new event handlers
        if (_previousButton != null)
            _previousButton.Click += OnPreviousButton_Click;
        if (_nextButton != null)
            _nextButton.Click += OnNextButton_Click;
        if (_monthButton != null)
            _monthButton.Click += OnMonthButton_Click;
        if (_yearButton != null)
            _yearButton.Click += OnYearButton_Click;
        if (_todayButton != null)
            _todayButton.Click += OnTodayButton_Click;

        UpdateCalendar();
    }

    private void OnMonthButton_Click(object? sender, RoutedEventArgs e)
    {
        ShowMonthSelection();
    }

    private void OnYearButton_Click(object? sender, RoutedEventArgs e)
    {
        ShowYearSelection();
    }

    private void OnTodayButton_Click(object? sender, RoutedEventArgs e)
    {
        // Fire event to parent to select today and close popup
        var today = PersianCalendarHelper.Today();
        TodayClicked?.Invoke(this, new TodayClickedEventArgs(today));
    }

    private void ShowMonthSelection()
    {
        if (_monthGrid == null || _monthPopup == null || _monthButton == null) return;

        _monthGrid.Children.Clear();
        
        int currentMonth = DisplayMonth;
        int row = 0;
        int col = 0;

        for (int month = 1; month <= 12; month++)
        {
            var button = new Button
            {
                Content = PersianCalendarHelper.GetMonthName(month, UseEnglishNames),
                [Grid.RowProperty] = row,
                [Grid.ColumnProperty] = col,
                Margin = new Thickness(2),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };

            if (month == currentMonth)
            {
                button.Classes.Add("selected");
            }

            int selectedMonth = month;
            button.Click += (s, e) =>
            {
                DisplayMonth = selectedMonth;
                _monthPopup.IsOpen = false;
            };

            _monthGrid.Children.Add(button);

            col++;
            if (col >= 3)
            {
                col = 0;
                row++;
            }
        }

        // Add row definitions
        _monthGrid.RowDefinitions.Clear();
        for (int i = 0; i < 4; i++)
        {
            _monthGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        }

        // Set placement target to the month button
        _monthPopup.PlacementTarget = _monthButton;
        _monthPopup.IsOpen = true;
    }

    private void ShowYearSelection()
    {
        if (_yearGrid == null || _yearPopup == null || _yearButton == null) return;

        _yearGrid.Children.Clear();
        
        int currentYear = DisplayYear;
        int startYear = currentYear - 10;
        int row = 0;
        int col = 0;

        for (int year = startYear; year < startYear + 30; year++)
        {
            if (year < 1) continue;

            var button = new Button
            {
                Content = year.ToString(),
                [Grid.RowProperty] = row,
                [Grid.ColumnProperty] = col,
                Margin = new Thickness(2),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };

            if (year == currentYear)
            {
                button.Classes.Add("selected");
            }

            int selectedYear = year;
            button.Click += (s, e) =>
            {
                DisplayYear = selectedYear;
                _yearPopup.IsOpen = false;
            };

            _yearGrid.Children.Add(button);

            col++;
            if (col >= 3)
            {
                col = 0;
                row++;
            }
        }

        // Add row definitions
        _yearGrid.RowDefinitions.Clear();
        for (int i = 0; i < 10; i++)
        {
            _yearGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        }

        // Set placement target to the year button
        _yearPopup.PlacementTarget = _yearButton;
        _yearPopup.IsOpen = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Handled)
            return;

        int daysInMonth = PersianCalendarHelper.GetDaysInMonth(DisplayYear, DisplayMonth);
        bool moved = false;
        bool monthChanged = false;

        switch (e.Key)
        {
            case Key.Left:
                // Move to next day (RTL: left is next day)
                if (_focusedDay < daysInMonth)
                {
                    _focusedDay++;
                    moved = true;
                }
                else
                {
                    // Move to next month
                    NavigateToMonth(1);
                    _focusedDay = 1;
                    monthChanged = true;
                }
                break;

            case Key.Right:
                // Move to previous day (RTL: right is previous day)
                if (_focusedDay > 1)
                {
                    _focusedDay--;
                    moved = true;
                }
                else
                {
                    // Move to previous month
                    NavigateToMonth(-1);
                    // Set to last day of previous month
                    daysInMonth = PersianCalendarHelper.GetDaysInMonth(DisplayYear, DisplayMonth);
                    _focusedDay = daysInMonth;
                    monthChanged = true;
                }
                break;

            case Key.Up:
                // Move to previous week
                if (_focusedDay > 7)
                {
                    _focusedDay -= 7;
                    moved = true;
                }
                else
                {
                    // Move to previous month, same weekday
                    NavigateToMonth(-1);
                    daysInMonth = PersianCalendarHelper.GetDaysInMonth(DisplayYear, DisplayMonth);
                    _focusedDay = Math.Min(_focusedDay, daysInMonth);
                    monthChanged = true;
                }
                break;

            case Key.Down:
                // Move to next week
                if (_focusedDay + 7 <= daysInMonth)
                {
                    _focusedDay += 7;
                    moved = true;
                }
                else
                {
                    // Move to next month, same weekday
                    NavigateToMonth(1);
                    daysInMonth = PersianCalendarHelper.GetDaysInMonth(DisplayYear, DisplayMonth);
                    _focusedDay = Math.Min(_focusedDay, daysInMonth);
                    monthChanged = true;
                }
                break;

            case Key.PageUp:
                // Move to previous month (faster navigation)
                NavigateToMonth(-1);
                monthChanged = true;
                break;

            case Key.PageDown:
                // Move to next month (faster navigation)
                NavigateToMonth(1);
                monthChanged = true;
                break;

            case Key.Home:
                // Move to first day of month
                _focusedDay = 1;
                moved = true;
                break;

            case Key.End:
                // Move to last day of month
                _focusedDay = daysInMonth;
                moved = true;
                break;

            case Key.Space:
            case Key.Enter:
                // Select the focused day
                OnDayClicked(_focusedDay);
                e.Handled = true;
                return;

            case Key.Escape:
                // Close popup (handled by parent)
                return;
        }

        if (moved || monthChanged)
        {
            // Update visual focus
            UpdateFocusedDay();
            e.Handled = true;
        }
    }

    private void NavigateToMonth(int deltaMonths)
    {
        int newMonth = DisplayMonth + deltaMonths;
        int newYear = DisplayYear;

        if (newMonth < 1)
        {
            newMonth = 12;
            newYear--;
        }
        else if (newMonth > 12)
        {
            newMonth = 1;
            newYear++;
        }

        // Validate year range (optional, prevent negative years)
        if (newYear < 1)
        {
            newYear = 1;
            newMonth = 1;
        }

        // Use SetCurrentValue to trigger property changed handler
        SetCurrentValue(DisplayMonthProperty, newMonth);
        SetCurrentValue(DisplayYearProperty, newYear);
    }

    private void UpdateFocusedDay()
    {
        if (_daysGrid == null) return;

        // Remove focus from all day buttons
        foreach (var child in _daysGrid.Children)
        {
            if (child is Button button)
            {
                button.Focusable = false;
            }
        }

        // Add focus to the focused day button
        foreach (var child in _daysGrid.Children)
        {
            if (child is Button button && 
                int.TryParse(button.Content?.ToString(), out int day) &&
                day == _focusedDay)
            {
                button.Focusable = true;
                button.Focus();
                break;
            }
        }
    }

    private void OnPreviousButton_Click(object? sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"Previous button clicked. Current: {DisplayYear}/{DisplayMonth}");
        
        if (DisplayMonth == 1)
        {
            SetCurrentValue(DisplayMonthProperty, 12);
            SetCurrentValue(DisplayYearProperty, DisplayYear - 1);
        }
        else
        {
            SetCurrentValue(DisplayMonthProperty, DisplayMonth - 1);
        }
        
        System.Diagnostics.Debug.WriteLine($"After SetCurrentValue: {DisplayYear}/{DisplayMonth}");
    }

    private void OnNextButton_Click(object? sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"Next button clicked. Current: {DisplayYear}/{DisplayMonth}");
        
        if (DisplayMonth == 12)
        {
            SetCurrentValue(DisplayMonthProperty, 1);
            SetCurrentValue(DisplayYearProperty, DisplayYear + 1);
        }
        else
        {
            SetCurrentValue(DisplayMonthProperty, DisplayMonth + 1);
        }
        
        System.Diagnostics.Debug.WriteLine($"After SetCurrentValue: {DisplayYear}/{DisplayMonth}");
    }

    private void UpdateCalendar()
    {
        System.Diagnostics.Debug.WriteLine($"UpdateCalendar called, isUpdating: {_isUpdatingCalendar}");
        
        // Prevent re-entrant calls
        if (_isUpdatingCalendar)
        {
            System.Diagnostics.Debug.WriteLine("UpdateCalendar blocked - already updating");
            return;
        }
        
        _isUpdatingCalendar = true;
        System.Diagnostics.Debug.WriteLine($"UpdateCalendar starting - DisplayYear: {DisplayYear}, DisplayMonth: {DisplayMonth}");

        // Reset focused day to selected date or today
        if (SelectedDate.HasValue &&
            SelectedDate.Value.Year == DisplayYear &&
            SelectedDate.Value.Month == DisplayMonth)
        {
            _focusedDay = SelectedDate.Value.Day;
        }
        else
        {
            var today = PersianCalendarHelper.Today();
            if (today.Year == DisplayYear && today.Month == DisplayMonth)
            {
                _focusedDay = today.Day;
            }
            else
            {
                _focusedDay = 1;
            }
        }
        
        try
        {
            if (_monthText != null)
            {
                _monthText.Text = PersianCalendarHelper.GetMonthName(DisplayMonth, UseEnglishNames);
            }
            
            if (_yearText != null)
            {
                _yearText.Text = DisplayYear.ToString();
            }
            
            if (_todayText != null)
            {
                var today = PersianCalendarHelper.Today();
                _todayText.Text = UseEnglishNames ? "Today" : "امروز";
            }

            if (_daysGrid != null)
            {
                _daysGrid.Children.Clear();
                _daysGrid.ColumnDefinitions.Clear();
                _daysGrid.RowDefinitions.Clear();

                // Create 7 columns for days of week
                for (int i = 0; i < 7; i++)
                {
                    _daysGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                }

                // Add day names header
                for (int i = 0; i < 7; i++)
                {
                    var dayName = PersianCalendarHelper.GetDayName(i, UseEnglishNames, shortFormat: true);
                    var textBlock = new TextBlock
                    {
                        Text = dayName,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        FontWeight = Avalonia.Media.FontWeight.Bold,
                        [Grid.RowProperty] = 0,
                        [Grid.ColumnProperty] = i
                    };
                    textBlock.Classes.Add("day-header");
                    _daysGrid.Children.Add(textBlock);
                }

                // Add rows for weeks (max 6 weeks + 1 for header = 7 total rows)
                for (int i = 0; i < 7; i++)
                {
                    _daysGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                }

                // Get first day of month and days in month
                int firstDayOfWeek = PersianCalendarHelper.GetFirstDayOfWeek(DisplayYear, DisplayMonth);
                int daysInMonth = PersianCalendarHelper.GetDaysInMonth(DisplayYear, DisplayMonth);

                // Add day buttons
                int currentRow = 1;
                int currentColumn = firstDayOfWeek;

                for (int day = 1; day <= daysInMonth; day++)
                {
                    int currentDay = day; // Capture the current day value
                    var button = new Button
                    {
                        Content = currentDay.ToString(),
                        [Grid.RowProperty] = currentRow,
                        [Grid.ColumnProperty] = currentColumn
                    };
                    button.Classes.Add("day-button");

                    // Check if this is the selected date
                    if (SelectedDate.HasValue &&
                        SelectedDate.Value.Year == DisplayYear &&
                        SelectedDate.Value.Month == DisplayMonth &&
                        SelectedDate.Value.Day == currentDay)
                    {
                        button.Classes.Add("selected");
                    }

                    // Check if this is today
                    var today = PersianCalendarHelper.Today();
                    if (today.Year == DisplayYear && today.Month == DisplayMonth && today.Day == currentDay)
                    {
                        button.Classes.Add("today");
                    }

                    button.Click += OnDayClicked;
                    _daysGrid.Children.Add(button);

                    currentColumn++;
                    if (currentColumn > 6)
                    {
                        currentColumn = 0;
                        currentRow++;
                    }
                }
            }
        }
        finally
        {
            _isUpdatingCalendar = false;
            System.Diagnostics.Debug.WriteLine("UpdateCalendar finished, isUpdating: false");
        }
    }

    private void OnDayClicked(int day)
    {
        // Calculate the day of week for the selected date
        var firstDayOfMonth = new PersianDate(DisplayYear, DisplayMonth, 1, 0);
        var firstDayOfWeek = PersianCalendarHelper.GetFirstDayOfWeek(DisplayYear, DisplayMonth);
        var dayOfWeek = (firstDayOfWeek + (day - 1)) % 7;
        
        // Create the selected date with correct day of week
        var selectedDate = new PersianDate(DisplayYear, DisplayMonth, day, dayOfWeek);

        System.Diagnostics.Debug.WriteLine($"Day clicked: {day}, DisplayYear: {DisplayYear}, DisplayMonth: {DisplayMonth}");
        System.Diagnostics.Debug.WriteLine($"Selected date: {selectedDate.ToString("long")}");

        // Fire the event to parent control
        DateSelected?.Invoke(this, new DateSelectedEventArgs(selectedDate));
    }

    private void OnDayClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Content is not string dayStr)
            return;
            
        if (!int.TryParse(dayStr, out int day))
            return;
        
        OnDayClicked(day);
    }
}
