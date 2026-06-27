using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using ModernPersianDatePicker.Localizations;

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

    public static readonly StyledProperty<ICalendarLocalization?> LocalizationProviderProperty =
        AvaloniaProperty.Register<CalendarView, ICalendarLocalization?>(nameof(LocalizationProvider));

    public static readonly StyledProperty<CalendarType> CalendarTypeProperty =
        AvaloniaProperty.Register<CalendarView, CalendarType>(nameof(CalendarType));

    public static readonly StyledProperty<int> FirstDayOfWeekProperty =
        AvaloniaProperty.Register<CalendarView, int>(nameof(FirstDayOfWeek), 0);

    /// <summary>
    /// Override brush for holiday day numbers. When null, the theme's built-in holiday color is used.
    /// </summary>
    public static readonly StyledProperty<IBrush?> HolidayBrushProperty =
        AvaloniaProperty.Register<CalendarView, IBrush?>(nameof(HolidayBrush));

    /// <summary>
    /// Days of the week (.NET <see cref="DayOfWeek"/>) treated as recurring weekly holidays,
    /// e.g. <see cref="DayOfWeek.Friday"/> for the Persian weekend. Defaults to Friday.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<DayOfWeek>> WeeklyHolidaysProperty =
        AvaloniaProperty.Register<CalendarView, IReadOnlyList<DayOfWeek>>(nameof(WeeklyHolidays),
            new[] { DayOfWeek.Friday });

    /// <summary>
    /// Specific calendar dates to mark as holidays, in addition to <see cref="WeeklyHolidays"/>.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<PersianDate>> HolidaysProperty =
        AvaloniaProperty.Register<CalendarView, IReadOnlyList<PersianDate>>(nameof(Holidays),
            Array.Empty<PersianDate>());

    public static readonly StyledProperty<bool> IsRangeModeProperty =
        AvaloniaProperty.Register<CalendarView, bool>(nameof(IsRangeMode));

    public static readonly StyledProperty<PersianDate?> RangeStartProperty =
        AvaloniaProperty.Register<CalendarView, PersianDate?>(nameof(RangeStart));

    public static readonly StyledProperty<PersianDate?> RangeEndProperty =
        AvaloniaProperty.Register<CalendarView, PersianDate?>(nameof(RangeEnd));

    // Events
    public event EventHandler<DateSelectedEventArgs>? DateSelected;
    public event EventHandler<DateRangeSelectedEventArgs>? DateRangeSelected;
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
    private bool _isSelectingEnd;

    // Derived caches for holiday checks (rebuilt when the source properties change)
    private HashSet<int> _weeklyHolidayIndices = new() { ToPersianWeekdayIndex(DayOfWeek.Friday) };
    private HashSet<PersianDate> _holidays = new();

    static CalendarView()
    {
        DisplayYearProperty.Changed.AddClassHandler<CalendarView>((x, e) => x.OnDisplayYearMonthChanged(e));
        DisplayMonthProperty.Changed.AddClassHandler<CalendarView>((x, e) => x.OnDisplayYearMonthChanged(e));
        LocalizationProviderProperty.Changed.AddClassHandler<CalendarView>((x, e) => x.OnDisplayYearMonthChanged(e));
        CalendarTypeProperty.Changed.AddClassHandler<CalendarView>((x, e) => x.OnDisplayYearMonthChanged(e));
        FirstDayOfWeekProperty.Changed.AddClassHandler<CalendarView>((x, e) => x.OnDisplayYearMonthChanged(e));
        SelectedDateProperty.Changed.AddClassHandler<CalendarView>((x, e) => x.OnSelectedDateChanged(e));
        WeeklyHolidaysProperty.Changed.AddClassHandler<CalendarView>((x, e) => x.OnHolidaysChanged(e));
        HolidaysProperty.Changed.AddClassHandler<CalendarView>((x, e) => x.OnHolidaysChanged(e));
        HolidayBrushProperty.Changed.AddClassHandler<CalendarView>((x, e) => x.OnHolidayBrushChanged(e));
        RangeStartProperty.Changed.AddClassHandler<CalendarView>((x, _) => x.UpdateCalendar());
        RangeEndProperty.Changed.AddClassHandler<CalendarView>((x, _) => x.UpdateCalendar());
    }

    public CalendarView()
    {
        // Set initial values based on CalendarType (default is Persian)
        if (CalendarType == CalendarType.Gregorian)
        {
            var today = DateTime.Today;
            DisplayYear = today.Year;
            DisplayMonth = today.Month;
            _focusedDay = today.Day;
        }
        else
        {
            var today = PersianCalendarHelper.Today();
            DisplayYear = today.Year;
            DisplayMonth = today.Month;
            _focusedDay = today.Day;
        }

        Focusable = true;

        Visual.SetFlowDirection(this, CalendarType == CalendarType.Gregorian
            ? Avalonia.Media.FlowDirection.LeftToRight
            : Avalonia.Media.FlowDirection.RightToLeft);
    }

    private void OnDisplayYearMonthChanged(AvaloniaPropertyChangedEventArgs e)
    {
        // Only update calendar if the property actually changed
        if (e.Property == DisplayYearProperty || e.Property == DisplayMonthProperty || e.Property == LocalizationProviderProperty
            || e.Property == CalendarTypeProperty || e.Property == FirstDayOfWeekProperty)
        {
            if (e.Property == CalendarTypeProperty)
            {
                // Set FlowDirection: LTR for Gregorian, RTL for Persian
                Visual.SetFlowDirection(this, CalendarType == CalendarType.Gregorian
                    ? Avalonia.Media.FlowDirection.LeftToRight
                    : Avalonia.Media.FlowDirection.RightToLeft);
            }
            UpdateCalendar();
        }
    }

    private void OnSelectedDateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        // Update the visual selection without rebuilding the entire calendar
        UpdateSelectedVisual();
    }

    private void OnHolidaysChanged(AvaloniaPropertyChangedEventArgs e)
    {
        // Rebuild the derived lookup caches from the source lists, then refresh the grid.
        _weeklyHolidayIndices = WeeklyHolidays?
            .Select(ToPersianWeekdayIndex)
            .ToHashSet() ?? new HashSet<int>();

        _holidays = Holidays != null
            ? new HashSet<PersianDate>(Holidays)
            : new HashSet<PersianDate>();

        UpdateCalendar();
    }

    private void OnHolidayBrushChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var brush = e.NewValue as IBrush;
        if (brush == null)
            Resources.Remove("PersianDatePickerHolidayForegroundBrush");
        else
            Resources["PersianDatePickerHolidayForegroundBrush"] = brush;
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

    public ICalendarLocalization? LocalizationProvider
    {
        get => GetValue(LocalizationProviderProperty);
        set => SetValue(LocalizationProviderProperty, value);
    }

    public CalendarType CalendarType
    {
        get => GetValue(CalendarTypeProperty);
        set => SetValue(CalendarTypeProperty, value);
    }

    public int FirstDayOfWeek
    {
        get => GetValue(FirstDayOfWeekProperty);
        set => SetValue(FirstDayOfWeekProperty, value);
    }

    /// <summary>
    /// Override brush for holiday day numbers. When null, the theme's built-in holiday color is used.
    /// </summary>
    public IBrush? HolidayBrush
    {
        get => GetValue(HolidayBrushProperty);
        set => SetValue(HolidayBrushProperty, value);
    }

    /// <summary>
    /// Days of the week treated as recurring weekly holidays. Defaults to <see cref="DayOfWeek.Friday"/>.
    /// </summary>
    public IReadOnlyList<DayOfWeek> WeeklyHolidays
    {
        get => GetValue(WeeklyHolidaysProperty);
        set => SetValue(WeeklyHolidaysProperty, value);
    }

    /// <summary>
    /// Specific calendar dates to mark as holidays.
    /// </summary>
    public IReadOnlyList<PersianDate> Holidays
    {
        get => GetValue(HolidaysProperty);
        set => SetValue(HolidaysProperty, value);
    }

    public bool IsRangeMode
    {
        get => GetValue(IsRangeModeProperty);
        set => SetValue(IsRangeModeProperty, value);
    }

    public PersianDate? RangeStart
    {
        get => GetValue(RangeStartProperty);
        set => SetValue(RangeStartProperty, value);
    }

    public PersianDate? RangeEnd
    {
        get => GetValue(RangeEndProperty);
        set => SetValue(RangeEndProperty, value);
    }

    private ICalendarLocalization GetLocalization()
    {
        return LocalizationProvider ?? new Localizations.FarsiLocalization();
    }

    /// <summary>
    /// Maps a .NET <see cref="DayOfWeek"/> (Sunday=0…Saturday=6) to the Persian weekday
    /// index used by this calendar (Saturday=0 … Friday=6). Same formula used by the
    /// calendar's own day-of-week computation: <c>((int)dotNetDayOfWeek + 1) % 7</c>.
    /// </summary>
    public static int ToPersianWeekdayIndex(DayOfWeek dotNetDayOfWeek)
        => ((int)dotNetDayOfWeek + 1) % 7;

    // Private field for keyboard navigation
    private int _focusedDay = 1;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

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
            var monthName = CalendarType == CalendarType.Gregorian
                ? PersianCalendarHelper.GetGregorianMonthName(month, GetLocalization())
                : GetLocalization().MonthNames[month];
            var button = new Button
            {
                Content = monthName,
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

        bool isLtr = Visual.GetFlowDirection(this) == Avalonia.Media.FlowDirection.LeftToRight;
        int daysInMonth = CalendarType == CalendarType.Gregorian
            ? PersianCalendarHelper.GetGregorianDaysInMonth(DisplayYear, DisplayMonth)
            : PersianCalendarHelper.GetDaysInMonth(DisplayYear, DisplayMonth);
        bool moved = false;
        bool monthChanged = false;

        switch (e.Key)
        {
            case Key.Left:
                if (isLtr)
                {
                    if (_focusedDay > 1) { _focusedDay--; moved = true; }
                    else { NavigateToMonth(-1); daysInMonth = CalendarType == CalendarType.Gregorian ? PersianCalendarHelper.GetGregorianDaysInMonth(DisplayYear, DisplayMonth) : PersianCalendarHelper.GetDaysInMonth(DisplayYear, DisplayMonth); _focusedDay = daysInMonth; monthChanged = true; }
                }
                else
                {
                    if (_focusedDay < daysInMonth) { _focusedDay++; moved = true; }
                    else { NavigateToMonth(1); _focusedDay = 1; monthChanged = true; }
                }
                break;

            case Key.Right:
                if (isLtr)
                {
                    if (_focusedDay < daysInMonth) { _focusedDay++; moved = true; }
                    else { NavigateToMonth(1); _focusedDay = 1; monthChanged = true; }
                }
                else
                {
                    if (_focusedDay > 1) { _focusedDay--; moved = true; }
                    else { NavigateToMonth(-1); daysInMonth = CalendarType == CalendarType.Gregorian ? PersianCalendarHelper.GetGregorianDaysInMonth(DisplayYear, DisplayMonth) : PersianCalendarHelper.GetDaysInMonth(DisplayYear, DisplayMonth); _focusedDay = daysInMonth; monthChanged = true; }
                }
                break;

            case Key.Up:
                if (_focusedDay > 7) { _focusedDay -= 7; moved = true; }
                else { NavigateToMonth(-1); daysInMonth = CalendarType == CalendarType.Gregorian ? PersianCalendarHelper.GetGregorianDaysInMonth(DisplayYear, DisplayMonth) : PersianCalendarHelper.GetDaysInMonth(DisplayYear, DisplayMonth); _focusedDay = Math.Min(_focusedDay, daysInMonth); monthChanged = true; }
                break;

            case Key.Down:
                if (_focusedDay + 7 <= daysInMonth) { _focusedDay += 7; moved = true; }
                else { NavigateToMonth(1); daysInMonth = CalendarType == CalendarType.Gregorian ? PersianCalendarHelper.GetGregorianDaysInMonth(DisplayYear, DisplayMonth) : PersianCalendarHelper.GetDaysInMonth(DisplayYear, DisplayMonth); _focusedDay = Math.Min(_focusedDay, daysInMonth); monthChanged = true; }
                break;

            case Key.PageUp:
                NavigateToMonth(-1);
                monthChanged = true;
                break;

            case Key.PageDown:
                NavigateToMonth(1);
                monthChanged = true;
                break;

            case Key.Home:
                _focusedDay = 1;
                moved = true;
                break;

            case Key.End:
                _focusedDay = daysInMonth;
                moved = true;
                break;

            case Key.Space:
            case Key.Enter:
                OnDayClicked(_focusedDay);
                e.Handled = true;
                return;

            case Key.Escape:
                return;
        }

        if (moved || monthChanged)
        {
            UpdateFocusedDay();
            e.Handled = true;
        }
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        if (e.Handled) return;

        bool isLtr = Visual.GetFlowDirection(this) == Avalonia.Media.FlowDirection.LeftToRight;
        int delta = isLtr
            ? (e.Delta.Y > 0 ? 1 : -1)
            : (e.Delta.Y > 0 ? -1 : 1);

        int daysInMonth = CalendarType == CalendarType.Gregorian
            ? PersianCalendarHelper.GetGregorianDaysInMonth(DisplayYear, DisplayMonth)
            : PersianCalendarHelper.GetDaysInMonth(DisplayYear, DisplayMonth);
        int newDay = _focusedDay + delta;

        if (newDay >= 1 && newDay <= daysInMonth)
        {
            _focusedDay = newDay;
        }
        else if (newDay > daysInMonth)
        {
            NavigateToMonth(1);
            _focusedDay = 1;
        }
        else
        {
            NavigateToMonth(-1);
            daysInMonth = CalendarType == CalendarType.Gregorian
                ? PersianCalendarHelper.GetGregorianDaysInMonth(DisplayYear, DisplayMonth)
                : PersianCalendarHelper.GetDaysInMonth(DisplayYear, DisplayMonth);
            _focusedDay = daysInMonth;
        }

        UpdateFocusedDay();
        e.Handled = true;
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
        if (DisplayMonth == 1)
        {
            SetCurrentValue(DisplayMonthProperty, 12);
            SetCurrentValue(DisplayYearProperty, DisplayYear - 1);
        }
        else
        {
            SetCurrentValue(DisplayMonthProperty, DisplayMonth - 1);
        }
    }

    private void OnNextButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DisplayMonth == 12)
        {
            SetCurrentValue(DisplayMonthProperty, 1);
            SetCurrentValue(DisplayYearProperty, DisplayYear + 1);
        }
        else
        {
            SetCurrentValue(DisplayMonthProperty, DisplayMonth + 1);
        }
    }

    private void UpdateCalendar()
    {
        // Prevent re-entrant calls
        if (_isUpdatingCalendar)
        {
            return;
        }
        
        _isUpdatingCalendar = true;

        // Reset focused day to selected date or today
        if (SelectedDate.HasValue &&
            SelectedDate.Value.Year == DisplayYear &&
            SelectedDate.Value.Month == DisplayMonth)
        {
            _focusedDay = SelectedDate.Value.Day;
        }
        else
        {
            if (CalendarType == CalendarType.Gregorian)
            {
                var today = DateTime.Today;
                _focusedDay = (today.Year == DisplayYear && today.Month == DisplayMonth) ? today.Day : 1;
            }
            else
            {
                var today = PersianCalendarHelper.Today();
                _focusedDay = (today.Year == DisplayYear && today.Month == DisplayMonth) ? today.Day : 1;
            }
        }
        
        try
        {
            var loc = GetLocalization();

            if (_monthText != null)
            {
                _monthText.Text = CalendarType == CalendarType.Gregorian
                    ? PersianCalendarHelper.GetGregorianMonthName(DisplayMonth, loc)
                    : loc.MonthNames[DisplayMonth];
            }
            
            if (_yearText != null)
            {
                _yearText.Text = DisplayYear.ToString();
            }
            
            if (_todayText != null)
            {
                _todayText.Text = loc.TodayLabel;
            }

            if (_daysGrid != null)
            {
                _daysGrid.Children.Clear();
                _daysGrid.ColumnDefinitions.Clear();
                _daysGrid.RowDefinitions.Clear();

                for (int i = 0; i < 7; i++)
                {
                    _daysGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                }

                // Add day names header
                for (int i = 0; i < 7; i++)
                {
                    int dayIndex = (FirstDayOfWeek + i) % 7;
                    var dayName = CalendarType == CalendarType.Gregorian
                        ? PersianCalendarHelper.GetGregorianShortDayName(dayIndex, loc)
                        : loc.ShortDayNames[dayIndex];
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

                for (int i = 0; i < 7; i++)
                {
                    _daysGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                }

                int firstDayOfWeek = CalendarType == CalendarType.Gregorian
                    ? PersianCalendarHelper.GetGregorianFirstDayOfWeek(DisplayYear, DisplayMonth)
                    : PersianCalendarHelper.GetFirstDayOfWeek(DisplayYear, DisplayMonth);
                int daysInMonth = CalendarType == CalendarType.Gregorian
                    ? PersianCalendarHelper.GetGregorianDaysInMonth(DisplayYear, DisplayMonth)
                    : PersianCalendarHelper.GetDaysInMonth(DisplayYear, DisplayMonth);

                // Normalize firstDayOfWeek relative to FirstDayOfWeek setting
                int startColumn = (firstDayOfWeek - FirstDayOfWeek + 7) % 7;

                int currentRow = 1;
                int currentColumn = startColumn;

                for (int day = 1; day <= daysInMonth; day++)
                {
                    int currentDay = day;
                    int dayOfWeekIndex = (startColumn + day - 1) % 7;
                    var button = new Button
                    {
                        Content = currentDay.ToString(),
                        [Grid.RowProperty] = currentRow,
                        [Grid.ColumnProperty] = currentColumn
                    };
                    button.Classes.Add("day-button");

                    if (SelectedDate.HasValue &&
                        SelectedDate.Value.Year == DisplayYear &&
                        SelectedDate.Value.Month == DisplayMonth &&
                        SelectedDate.Value.Day == currentDay)
                    {
                        button.Classes.Add("selected");
                    }

                    var todayPersian = PersianCalendarHelper.Today();
                    var todayGregorian = DateTime.Today;
                    bool isToday = CalendarType == CalendarType.Gregorian
                        ? todayGregorian.Year == DisplayYear && todayGregorian.Month == DisplayMonth && todayGregorian.Day == currentDay
                        : todayPersian.Year == DisplayYear && todayPersian.Month == DisplayMonth && todayPersian.Day == currentDay;
                    if (isToday)
                    {
                        button.Classes.Add("today");
                    }

                    if (CalendarType == CalendarType.Persian)
                    {
                        bool isHoliday = _weeklyHolidayIndices.Contains(currentColumn)
                            || _holidays.Contains(new PersianDate(DisplayYear, DisplayMonth, currentDay, currentColumn));
                        if (isHoliday)
                        {
                            button.Classes.Add("holiday");
                        }
                    }

                    if (IsRangeMode)
                    {
                        var buttonDate = new PersianDate(DisplayYear, DisplayMonth, currentDay, dayOfWeekIndex);
                        bool isStart = RangeStart.HasValue && buttonDate == RangeStart.Value;
                        bool isEnd = RangeEnd.HasValue && buttonDate == RangeEnd.Value;
                        bool isIn = RangeStart.HasValue && RangeEnd.HasValue
                            && buttonDate > RangeStart.Value && buttonDate < RangeEnd.Value;

                        if (isStart) button.Classes.Add("range-start");
                        if (isEnd) button.Classes.Add("range-end");
                        if (isIn) button.Classes.Add("in-range");
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
        }
    }

    private void OnDayClicked(int day)
    {
        var firstDayOfWeek = PersianCalendarHelper.GetFirstDayOfWeek(DisplayYear, DisplayMonth);
        var dayOfWeek = (firstDayOfWeek + (day - 1)) % 7;
        var clickedDate = new PersianDate(DisplayYear, DisplayMonth, day, dayOfWeek);

        if (IsRangeMode)
        {
            if (!_isSelectingEnd)
            {
                RangeStart = clickedDate;
                RangeEnd = null;
                _isSelectingEnd = true;
            }
            else
            {
                if (clickedDate < RangeStart)
                {
                    RangeEnd = RangeStart;
                    RangeStart = clickedDate;
                }
                else
                {
                    RangeEnd = clickedDate;
                }
                _isSelectingEnd = false;
                DateRangeSelected?.Invoke(this, new DateRangeSelectedEventArgs(RangeStart, RangeEnd));
            }
            UpdateCalendar();
        }
        else
        {
            DateSelected?.Invoke(this, new DateSelectedEventArgs(clickedDate));
        }
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
