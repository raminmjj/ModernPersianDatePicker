using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using ModernPersianDatePicker.Localizations;

namespace ModernPersianDatePicker;

/// <summary>
/// Quick range selection view for choosing preset date ranges
/// </summary>
public class QuickRangeView : TemplatedControl
{
    // Styled Properties
    public static readonly StyledProperty<ICalendarLocalization?> LocalizationProviderProperty =
        AvaloniaProperty.Register<QuickRangeView, ICalendarLocalization?>(nameof(LocalizationProvider));

    public static readonly StyledProperty<CalendarType> CalendarTypeProperty =
        AvaloniaProperty.Register<QuickRangeView, CalendarType>(nameof(CalendarType));

    public static readonly StyledProperty<PersianDate?> RangeStartProperty =
        AvaloniaProperty.Register<QuickRangeView, PersianDate?>(nameof(RangeStart));

    public static readonly StyledProperty<PersianDate?> RangeEndProperty =
        AvaloniaProperty.Register<QuickRangeView, PersianDate?>(nameof(RangeEnd));

    // Events
    public event EventHandler<QuickRangeSelectedEventArgs>? QuickRangeApplied;

    // Private fields
    private ModernPersianDatePicker? _fromPicker;
    private ModernPersianDatePicker? _toPicker;
    private Button? _applyButton;
    private Button? _todayButton;
    private Button? _weekButton;
    private Button? _monthButton;
    private Button? _yearButton;
    private Button? _groupMonthButton;
    private Button? _groupSeasonButton;
    private Button? _groupHalfYearButton;
    private Panel? _groupSelectorPanel;
    private Panel? _groupContentPanel;
    private Border? _groupContentBorder;
    private Button? _backButton;
    private StackPanel? _row1Panel;
    private StackPanel? _row2Panel;

    static QuickRangeView()
    {
        LocalizationProviderProperty.Changed.AddClassHandler<QuickRangeView>((x, _) => x.UpdateLabels());
    }

    public QuickRangeView()
    {
        Focusable = false;
    }

    // Public Properties
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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // Detach previous handlers
        if (_applyButton != null) _applyButton.Click -= OnApply_Click;
        if (_todayButton != null) _todayButton.Click -= OnToday_Click;
        if (_weekButton != null) _weekButton.Click -= OnWeek_Click;
        if (_monthButton != null) _monthButton.Click -= OnPresetMonth_Click;
        if (_yearButton != null) _yearButton.Click -= OnYear_Click;
        if (_groupMonthButton != null) _groupMonthButton.Click -= OnGroupMonth_Click;
        if (_groupSeasonButton != null) _groupSeasonButton.Click -= OnGroupSeason_Click;
        if (_groupHalfYearButton != null) _groupHalfYearButton.Click -= OnGroupHalfYear_Click;
        if (_backButton != null) _backButton.Click -= OnBack_Click;

        // Find template parts
        _fromPicker = e.NameScope.Find<ModernPersianDatePicker>("PART_FromPicker");
        _toPicker = e.NameScope.Find<ModernPersianDatePicker>("PART_ToPicker");
        _applyButton = e.NameScope.Find<Button>("PART_ApplyButton");
        _todayButton = e.NameScope.Find<Button>("PART_TodayButton");
        _weekButton = e.NameScope.Find<Button>("PART_WeekButton");
        _monthButton = e.NameScope.Find<Button>("PART_MonthButton");
        _yearButton = e.NameScope.Find<Button>("PART_YearButton");
        _groupMonthButton = e.NameScope.Find<Button>("PART_GroupMonthButton");
        _groupSeasonButton = e.NameScope.Find<Button>("PART_GroupSeasonButton");
        _groupHalfYearButton = e.NameScope.Find<Button>("PART_GroupHalfYearButton");
        _groupSelectorPanel = e.NameScope.Find<Panel>("PART_GroupSelectorPanel");
        _groupContentPanel = e.NameScope.Find<Panel>("PART_GroupContentPanel");
        _groupContentBorder = e.NameScope.Find<Border>("PART_GroupContentBorder");
        _backButton = e.NameScope.Find<Button>("PART_BackButton");
        _row1Panel = e.NameScope.Find<StackPanel>("PART_Row1Panel");
        _row2Panel = e.NameScope.Find<StackPanel>("PART_Row2Panel");

        // Attach handlers
        if (_applyButton != null) _applyButton.Click += OnApply_Click;
        if (_todayButton != null) _todayButton.Click += OnToday_Click;
        if (_weekButton != null) _weekButton.Click += OnWeek_Click;
        if (_monthButton != null) _monthButton.Click += OnPresetMonth_Click;
        if (_yearButton != null) _yearButton.Click += OnYear_Click;
        if (_groupMonthButton != null) _groupMonthButton.Click += OnGroupMonth_Click;
        if (_groupSeasonButton != null) _groupSeasonButton.Click += OnGroupSeason_Click;
        if (_groupHalfYearButton != null) _groupHalfYearButton.Click += OnGroupHalfYear_Click;
        if (_backButton != null) _backButton.Click += OnBack_Click;

        // Configure pickers
        if (_fromPicker != null)
        {
            _fromPicker.IsRangeMode = false;
            _fromPicker.CalendarType = CalendarType;
            _fromPicker.LocalizationProvider = LocalizationProvider;
            if (RangeStart.HasValue)
                _fromPicker.SelectedDate = RangeStart;
        }

        if (_toPicker != null)
        {
            _toPicker.IsRangeMode = false;
            _toPicker.CalendarType = CalendarType;
            _toPicker.LocalizationProvider = LocalizationProvider;
            if (RangeEnd.HasValue)
                _toPicker.SelectedDate = RangeEnd;
        }

        UpdateLabels();
        ShowGroupSelector();
    }

    private ICalendarLocalization GetLocalization()
    {
        return LocalizationProvider ?? new FarsiLocalization();
    }

    private void UpdateLabels()
    {
        var loc = GetLocalization();

        if (_applyButton != null)
            _applyButton.Content = loc.ApplyLabel;
        if (_todayButton != null)
            SetPresetButtonContent(_todayButton, loc.TodayLabel);
        if (_weekButton != null)
            SetPresetButtonContent(_weekButton, loc.CurrentWeekLabel);
        if (_monthButton != null)
            SetPresetButtonContent(_monthButton, loc.CurrentMonthLabel);
        if (_yearButton != null)
            SetPresetButtonContent(_yearButton, loc.CurrentYearLabel);
        if (_groupMonthButton != null)
            _groupMonthButton.Content = loc.MonthLabel;
        if (_groupSeasonButton != null)
            _groupSeasonButton.Content = loc.SeasonLabel;
        if (_groupHalfYearButton != null)
            _groupHalfYearButton.Content = loc.HalfYearLabel;
        if (_backButton != null)
            _backButton.Content = loc.BackLabel;
    }

    private void SetPresetButtonContent(Button button, string label)
    {
        if (button.Content is StackPanel sp)
        {
            foreach (var child in sp.Children)
            {
                if (child is TextBlock tb)
                {
                    tb.Text = label;
                    break;
                }
            }
        }
        else
        {
            button.Content = label;
        }
    }

    private void OnApply_Click(object? sender, RoutedEventArgs e)
    {
        var from = _fromPicker?.SelectedDate;
        var to = _toPicker?.SelectedDate;

        if (from.HasValue && to.HasValue)
        {
            var start = from.Value <= to.Value ? from.Value : to.Value;
            var end = from.Value <= to.Value ? to.Value : from.Value;
            QuickRangeApplied?.Invoke(this, new QuickRangeSelectedEventArgs(start, end, "custom"));
        }
    }

    private void OnToday_Click(object? sender, RoutedEventArgs e)
    {
        var today = PersianCalendarHelper.Today();
        QuickRangeApplied?.Invoke(this, new QuickRangeSelectedEventArgs(today, today, "today"));
    }

    private void OnWeek_Click(object? sender, RoutedEventArgs e)
    {
        var today = PersianCalendarHelper.Today();
        int dayOfWeek = today.DayOfWeek; // 0=Saturday, 6=Friday

        // Calculate week start (Saturday)
        int startDay = today.Day - dayOfWeek;
        int startMonth = today.Month;
        int startYear = today.Year;

        if (startDay < 1)
        {
            startMonth--;
            if (startMonth < 1)
            {
                startMonth = 12;
                startYear--;
            }
            startDay += PersianCalendarHelper.GetDaysInMonth(startYear, startMonth);
        }

        // Calculate week end (Friday)
        int endDay = today.Day + (6 - dayOfWeek);
        int endMonth = today.Month;
        int endYear = today.Year;

        int daysInMonth = PersianCalendarHelper.GetDaysInMonth(endYear, endMonth);
        if (endDay > daysInMonth)
        {
            endDay -= daysInMonth;
            endMonth++;
            if (endMonth > 12)
            {
                endMonth = 1;
                endYear++;
            }
        }

        var startDow1 = PersianCalendarHelper.GetFirstDayOfWeek(startYear, startMonth);
        var weekStart = new PersianDate(startYear, startMonth, startDay, (startDow1 + startDay - 1) % 7);

        var startDow2 = PersianCalendarHelper.GetFirstDayOfWeek(endYear, endMonth);
        var weekEnd = new PersianDate(endYear, endMonth, endDay, (startDow2 + endDay - 1) % 7);

        QuickRangeApplied?.Invoke(this, new QuickRangeSelectedEventArgs(weekStart, weekEnd, "week"));
    }

    private void OnPresetMonth_Click(object? sender, RoutedEventArgs e)
    {
        var today = PersianCalendarHelper.Today();
        int daysInMonth = PersianCalendarHelper.GetDaysInMonth(today.Year, today.Month);

        var startDow = PersianCalendarHelper.GetFirstDayOfWeek(today.Year, today.Month);
        var monthStart = new PersianDate(today.Year, today.Month, 1, startDow);
        var monthEnd = new PersianDate(today.Year, today.Month, daysInMonth, (startDow + daysInMonth - 1) % 7);

        QuickRangeApplied?.Invoke(this, new QuickRangeSelectedEventArgs(monthStart, monthEnd, "month"));
    }

    private void OnYear_Click(object? sender, RoutedEventArgs e)
    {
        var today = PersianCalendarHelper.Today();
        int daysInLastMonth = PersianCalendarHelper.GetDaysInMonth(today.Year, 12);

        var startDow1 = PersianCalendarHelper.GetFirstDayOfWeek(today.Year, 1);
        var yearStart = new PersianDate(today.Year, 1, 1, startDow1);

        var startDow2 = PersianCalendarHelper.GetFirstDayOfWeek(today.Year, 12);
        var yearEnd = new PersianDate(today.Year, 12, daysInLastMonth, (startDow2 + daysInLastMonth - 1) % 7);

        QuickRangeApplied?.Invoke(this, new QuickRangeSelectedEventArgs(yearStart, yearEnd, "year"));
    }

    private void OnGroupMonth_Click(object? sender, RoutedEventArgs e)
    {
        ShowMonthGrid();
    }

    private void OnGroupSeason_Click(object? sender, RoutedEventArgs e)
    {
        ShowSeasonGrid();
    }

    private void OnGroupHalfYear_Click(object? sender, RoutedEventArgs e)
    {
        ShowHalfYearGrid();
    }

    private void OnBack_Click(object? sender, RoutedEventArgs e)
    {
        ShowGroupSelector();
    }

    private void ShowGroupSelector()
    {
        if (_groupSelectorPanel != null)
        {
            _groupSelectorPanel.IsVisible = true;
            _groupSelectorPanel.IsHitTestVisible = true;
        }
        if (_groupContentBorder != null)
        {
            _groupContentBorder.IsVisible = false;
            _groupContentBorder.IsHitTestVisible = false;
        }
        if (_backButton != null)
        {
            _backButton.IsVisible = false;
            _backButton.IsHitTestVisible = false;
        }
    }

    private void ShowGroupContent()
    {
        if (_groupSelectorPanel != null)
        {
            _groupSelectorPanel.IsVisible = false;
            _groupSelectorPanel.IsHitTestVisible = false;
        }
        if (_groupContentBorder != null)
        {
            _groupContentBorder.IsVisible = true;
            _groupContentBorder.IsHitTestVisible = true;
        }
        if (_backButton != null)
        {
            _backButton.IsVisible = true;
            _backButton.IsHitTestVisible = true;
        }
    }

    private void ShowMonthGrid()
    {
        if (_groupContentPanel == null) return;

        ShowGroupContent();
        _groupContentPanel.Children.Clear();

        var loc = GetLocalization();
        var today = PersianCalendarHelper.Today();
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*,*") };

        for (int i = 0; i < 4; i++)
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        int row = 0, col = 0;
        for (int month = 1; month <= 12; month++)
        {
            var monthName = loc.MonthNames[month];
            var button = new Button
            {
                Content = monthName,
                [Grid.RowProperty] = row,
                [Grid.ColumnProperty] = col,
                Margin = new Thickness(2),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            button.Classes.Add("month-year-button");

            if (month == today.Month)
                button.Classes.Add("selected");

            int selectedMonth = month;
            button.Click += (s, e) => OnMonthSelected(selectedMonth);

            grid.Children.Add(button);
            col++;
            if (col >= 3) { col = 0; row++; }
        }

        _groupContentPanel.Children.Add(grid);
    }

    private void OnMonthSelected(int month)
    {
        var today = PersianCalendarHelper.Today();
        int daysInMonth = PersianCalendarHelper.GetDaysInMonth(today.Year, month);
        int startDow = PersianCalendarHelper.GetFirstDayOfWeek(today.Year, month);

        var monthStart = new PersianDate(today.Year, month, 1, startDow);
        var monthEnd = new PersianDate(today.Year, month, daysInMonth, (startDow + daysInMonth - 1) % 7);

        QuickRangeApplied?.Invoke(this, new QuickRangeSelectedEventArgs(monthStart, monthEnd, "month"));
    }

    private void ShowSeasonGrid()
    {
        if (_groupContentPanel == null) return;

        ShowGroupContent();
        _groupContentPanel.Children.Clear();

        var loc = GetLocalization();
        var today = PersianCalendarHelper.Today();
        int currentSeason = (today.Month - 1) / 3; // 0-3

        string[] seasonNames;
        if (loc is FarsiLocalization)
            seasonNames = new[] { "بهار", "تابستان", "پاییز", "زمستان" };
        else if (loc is KurdishLocalization)
            seasonNames = new[] { "بەهار", "تابستان", "پاییز", "زستان" };
        else if (loc is ArabicLocalization)
            seasonNames = new[] { "ربيع", "صيف", "خريف", "شتاء" };
        else
            seasonNames = new[] { "Spring", "Summer", "Autumn", "Winter" };

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*,*,*") };
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        for (int i = 0; i < 4; i++)
        {
            var button = new Button
            {
                Content = seasonNames[i],
                [Grid.RowProperty] = 0,
                [Grid.ColumnProperty] = i,
                Margin = new Thickness(2),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            button.Classes.Add("month-year-button");

            if (i == currentSeason)
                button.Classes.Add("selected");

            int seasonIndex = i;
            button.Click += (s, e) => OnSeasonSelected(seasonIndex);

            grid.Children.Add(button);
        }

        _groupContentPanel.Children.Add(grid);
    }

    private void OnSeasonSelected(int seasonIndex)
    {
        var today = PersianCalendarHelper.Today();
        int startMonth = seasonIndex * 3 + 1;
        int endMonth = startMonth + 2;
        int daysInEndMonth = PersianCalendarHelper.GetDaysInMonth(today.Year, endMonth);

        var startDow1 = PersianCalendarHelper.GetFirstDayOfWeek(today.Year, startMonth);
        var rangeStart = new PersianDate(today.Year, startMonth, 1, startDow1);

        var startDow2 = PersianCalendarHelper.GetFirstDayOfWeek(today.Year, endMonth);
        var rangeEnd = new PersianDate(today.Year, endMonth, daysInEndMonth, (startDow2 + daysInEndMonth - 1) % 7);

        QuickRangeApplied?.Invoke(this, new QuickRangeSelectedEventArgs(rangeStart, rangeEnd, "season"));
    }

    private void ShowHalfYearGrid()
    {
        if (_groupContentPanel == null) return;

        ShowGroupContent();
        _groupContentPanel.Children.Clear();

        var loc = GetLocalization();
        var today = PersianCalendarHelper.Today();
        int currentHalf = today.Month <= 6 ? 0 : 1;

        string h1Label, h2Label;
        if (loc is FarsiLocalization)
        { h1Label = "نیم‌سال اول"; h2Label = "نیم‌سال دوم"; }
        else if (loc is KurdishLocalization)
        { h1Label = "نیوەساڵی یەکەم"; h2Label = "نیوەساڵی دووەم"; }
        else if (loc is ArabicLocalization)
        { h1Label = "النصف الأول"; h2Label = "النصف الثاني"; }
        else
        { h1Label = "H1 (Jan-Jun)"; h2Label = "H2 (Jul-Dec)"; }

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*") };
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        for (int i = 0; i < 2; i++)
        {
            var button = new Button
            {
                Content = i == 0 ? h1Label : h2Label,
                [Grid.RowProperty] = 0,
                [Grid.ColumnProperty] = i,
                Margin = new Thickness(2),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            button.Classes.Add("month-year-button");

            if (i == currentHalf)
                button.Classes.Add("selected");

            int halfIndex = i;
            button.Click += (s, e) => OnHalfYearSelected(halfIndex);

            grid.Children.Add(button);
        }

        _groupContentPanel.Children.Add(grid);
    }

    private void OnHalfYearSelected(int halfIndex)
    {
        var today = PersianCalendarHelper.Today();
        int startMonth = halfIndex == 0 ? 1 : 7;
        int endMonth = halfIndex == 0 ? 6 : 12;
        int daysInEndMonth = PersianCalendarHelper.GetDaysInMonth(today.Year, endMonth);

        var startDow1 = PersianCalendarHelper.GetFirstDayOfWeek(today.Year, startMonth);
        var rangeStart = new PersianDate(today.Year, startMonth, 1, startDow1);

        var startDow2 = PersianCalendarHelper.GetFirstDayOfWeek(today.Year, endMonth);
        var rangeEnd = new PersianDate(today.Year, endMonth, daysInEndMonth, (startDow2 + daysInEndMonth - 1) % 7);

        QuickRangeApplied?.Invoke(this, new QuickRangeSelectedEventArgs(rangeStart, rangeEnd, "halfyear"));
    }
}
