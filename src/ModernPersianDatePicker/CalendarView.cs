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

    // Private fields
    private TextBlock? _monthYearText;
    private Grid? _daysGrid;
    private Button? _previousButton;
    private Button? _nextButton;
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
        {
            System.Diagnostics.Debug.WriteLine("Detaching previous button handlers");
            _previousButton.Click -= OnPreviousButton_Click;
        }
        if (_nextButton != null)
            _nextButton.Click -= OnNextButton_Click;

        // Get template parts
        _monthYearText = e.NameScope.Find<TextBlock>("PART_MonthYearText");
        _daysGrid = e.NameScope.Find<Grid>("PART_DaysGrid");
        _previousButton = e.NameScope.Find<Button>("PART_PreviousButton");
        _nextButton = e.NameScope.Find<Button>("PART_NextButton");
        
        System.Diagnostics.Debug.WriteLine($"Template parts found: previous={_previousButton != null}, next={_nextButton != null}, daysGrid={_daysGrid != null}");

        // Attach new event handlers
        if (_previousButton != null)
        {
            _previousButton.Click += OnPreviousButton_Click;
            System.Diagnostics.Debug.WriteLine("Attached PreviousButton click handler");
        }
        if (_nextButton != null)
        {
            _nextButton.Click += OnNextButton_Click;
            System.Diagnostics.Debug.WriteLine("Attached NextButton click handler");
        }

        UpdateCalendar();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Handled)
            return;

        int daysInMonth = PersianCalendarHelper.GetDaysInMonth(DisplayYear, DisplayMonth);
        bool moved = false;

        switch (e.Key)
        {
            case Key.Left:
                // Move to previous day (RTL: left is next day)
                if (_focusedDay < daysInMonth)
                {
                    _focusedDay++;
                    moved = true;
                }
                break;

            case Key.Right:
                // Move to previous day (RTL: right is previous day)
                if (_focusedDay > 1)
                {
                    _focusedDay--;
                    moved = true;
                }
                break;

            case Key.Up:
                // Move to previous week
                if (_focusedDay > 7)
                {
                    _focusedDay -= 7;
                    moved = true;
                }
                break;

            case Key.Down:
                // Move to next week
                if (_focusedDay + 7 <= daysInMonth)
                {
                    _focusedDay += 7;
                    moved = true;
                }
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

        if (moved)
        {
            // Update visual focus
            UpdateFocusedDay();
            e.Handled = true;
        }
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
            if (_monthYearText != null)
            {
                _monthYearText.Text = $"{PersianCalendarHelper.GetMonthName(DisplayMonth, UseEnglishNames)} {DisplayYear}";
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

                // Add rows for weeks (max 6 weeks)
                for (int i = 0; i < 6; i++)
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
        // Create the selected date with correct day of week
        var selectedDate = new PersianDate(DisplayYear, DisplayMonth, day, 0);
        
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
