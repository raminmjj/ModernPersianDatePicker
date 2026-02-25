using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
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

    static CalendarView()
    {
        DisplayYearProperty.Changed.AddClassHandler<CalendarView>((x, e) => x.OnDisplayYearMonthChanged(e));
        DisplayMonthProperty.Changed.AddClassHandler<CalendarView>((x, e) => x.OnDisplayYearMonthChanged(e));
        UseEnglishNamesProperty.Changed.AddClassHandler<CalendarView>((x, e) => x.OnDisplayYearMonthChanged(e));
    }

    public CalendarView()
    {
        var today = PersianCalendarHelper.Today();
        DisplayYear = today.Year;
        DisplayMonth = today.Month;
    }

    private void OnDisplayYearMonthChanged(AvaloniaPropertyChangedEventArgs e)
    {
        // Only update calendar if the property actually changed
        if (e.Property == DisplayYearProperty || e.Property == DisplayMonthProperty || e.Property == UseEnglishNamesProperty)
        {
            UpdateCalendar();
        }
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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // Detach previous event handlers
        if (_previousButton != null)
            _previousButton.Click -= OnPreviousButton_Click;
        if (_nextButton != null)
            _nextButton.Click -= OnNextButton_Click;

        // Get template parts
        _monthYearText = e.NameScope.Find<TextBlock>("PART_MonthYearText");
        _daysGrid = e.NameScope.Find<Grid>("PART_DaysGrid");
        _previousButton = e.NameScope.Find<Button>("PART_PreviousButton");
        _nextButton = e.NameScope.Find<Button>("PART_NextButton");

        // Attach new event handlers
        if (_previousButton != null)
            _previousButton.Click += OnPreviousButton_Click;
        if (_nextButton != null)
            _nextButton.Click += OnNextButton_Click;

        UpdateCalendar();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        // Clean up event handlers to prevent memory leaks
        if (_previousButton != null)
            _previousButton.Click -= OnPreviousButton_Click;
        if (_nextButton != null)
            _nextButton.Click -= OnNextButton_Click;
    }

    private void OnPreviousButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DisplayMonth == 1)
        {
            DisplayMonth = 12;
            DisplayYear--;
        }
        else
        {
            DisplayMonth--;
        }
    }

    private void OnNextButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DisplayMonth == 12)
        {
            DisplayMonth = 1;
            DisplayYear++;
        }
        else
        {
            DisplayMonth++;
        }
    }

    private void UpdateCalendar()
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
                var button = new Button
                {
                    Content = day.ToString(),
                    [Grid.RowProperty] = currentRow,
                    [Grid.ColumnProperty] = currentColumn
                };
                button.Classes.Add("day-button");

                // Check if this is the selected date
                if (SelectedDate.HasValue &&
                    SelectedDate.Value.Year == DisplayYear &&
                    SelectedDate.Value.Month == DisplayMonth &&
                    SelectedDate.Value.Day == day)
                {
                    button.Classes.Add("selected");
                }

                // Check if this is today
                var today = PersianCalendarHelper.Today();
                if (today.Year == DisplayYear && today.Month == DisplayMonth && today.Day == day)
                {
                    button.Classes.Add("today");
                }

                button.Click += (s, e) => OnDayClicked(day);
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

    private void OnDayClicked(int day)
    {
        var selectedDate = new PersianDate(DisplayYear, DisplayMonth, day, 0);
        SelectedDate = selectedDate;
        DateSelected?.Invoke(this, new DateSelectedEventArgs(selectedDate));
    }
}
