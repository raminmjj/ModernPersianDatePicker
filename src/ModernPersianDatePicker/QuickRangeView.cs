using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
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

    public static readonly StyledProperty<IReadOnlyList<DayOfWeek>> WeeklyHolidaysProperty =
        AvaloniaProperty.Register<QuickRangeView, IReadOnlyList<DayOfWeek>>(nameof(WeeklyHolidays),
            new[] { DayOfWeek.Friday });

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
        LocalizationProviderProperty.Changed.AddClassHandler<QuickRangeView>((x, _) => { x.UpdateLabels(); x.SyncPickersSettings(); });
        CalendarTypeProperty.Changed.AddClassHandler<QuickRangeView>((x, _) => x.SyncPickersSettings());
        WeeklyHolidaysProperty.Changed.AddClassHandler<QuickRangeView>((x, _) => x.SyncPickersSettings());
        RangeStartProperty.Changed.AddClassHandler<QuickRangeView>((x, _) => x.SyncPickersFromRange());
        RangeEndProperty.Changed.AddClassHandler<QuickRangeView>((x, _) => x.SyncPickersFromRange());
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

    public IReadOnlyList<DayOfWeek> WeeklyHolidays
    {
        get => GetValue(WeeklyHolidaysProperty);
        set => SetValue(WeeklyHolidaysProperty, value);
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
            _fromPicker.IsEditable = true;
            _fromPicker.CalendarType = CalendarType;
            _fromPicker.LocalizationProvider = LocalizationProvider;
            if (RangeStart.HasValue)
                _fromPicker.SelectedDate = RangeStart;
        }

        if (_toPicker != null)
        {
            _toPicker.IsRangeMode = false;
            _toPicker.IsEditable = true;
            _toPicker.CalendarType = CalendarType;
            _toPicker.LocalizationProvider = LocalizationProvider;
            if (RangeEnd.HasValue)
                _toPicker.SelectedDate = RangeEnd;
        }

        UpdateLabels();
        ShowGroupSelector();
    }

    // ── Calendar-aware helpers ──

    private bool IsGregorian => GetEffectiveCalendarType() == CalendarType.Gregorian;

    private CalendarType GetEffectiveCalendarType()
    {
        if (CalendarType != CalendarType.Auto)
            return CalendarType;

        var culture = System.Globalization.CultureInfo.CurrentUICulture;
        return culture.TwoLetterISOLanguageName is "fa" or "ps"
            ? CalendarType.Persian
            : CalendarType.Gregorian;
    }

    private ICalendarLocalization GetLocalization()
    {
        if (LocalizationProvider != null)
            return LocalizationProvider;

        return IsGregorian
            ? new EnglishLocalization()
            : new FarsiLocalization();
    }

    private PersianDate GetToday()
    {
        if (IsGregorian)
        {
            var today = DateTime.Today;
            int dow = PersianCalendarHelper.GetGregorianFirstDayOfWeek(today.Year, today.Month);
            int dayOfWeek = (dow + today.Day - 1) % 7;
            return new PersianDate(today.Year, today.Month, today.Day, dayOfWeek);
        }
        return PersianCalendarHelper.Today();
    }

    private int GetDaysInMonth(int year, int month)
    {
        if (IsGregorian)
            return DateTime.DaysInMonth(year, month);
        return PersianCalendarHelper.GetDaysInMonth(year, month);
    }

    private int GetFirstDayOfWeek(int year, int month)
    {
        if (IsGregorian)
            return PersianCalendarHelper.GetGregorianFirstDayOfWeek(year, month);
        return PersianCalendarHelper.GetFirstDayOfWeek(year, month);
    }

    private PersianDate MakeDate(int year, int month, int day)
    {
        int dow = GetFirstDayOfWeek(year, month);
        int dayOfWeek = (dow + day - 1) % 7;
        return new PersianDate(year, month, day, dayOfWeek);
    }

    // ── Label sync ──

    private void SyncPickersFromRange()
    {
        if (_fromPicker != null)
            _fromPicker.SelectedDate = RangeStart;
        if (_toPicker != null)
            _toPicker.SelectedDate = RangeEnd;
    }

    private void SyncPickersSettings()
    {
        if (_fromPicker != null)
        {
            _fromPicker.CalendarType = CalendarType;
            _fromPicker.LocalizationProvider = LocalizationProvider;
            _fromPicker.WeeklyHolidays = WeeklyHolidays;
        }
        if (_toPicker != null)
        {
            _toPicker.CalendarType = CalendarType;
            _toPicker.LocalizationProvider = LocalizationProvider;
            _toPicker.WeeklyHolidays = WeeklyHolidays;
        }
    }

    /// <summary>
    /// Called when the Quick Range view becomes visible to force pickers to refresh.
    /// Handles the case where pickers were not yet templated when properties changed.
    /// </summary>
    public void RefreshPickers()
    {
        SyncPickersSettings();
        SyncPickersFromRange();
        // Force embedded pickers to re-apply all settings to their internal calendar views
        _fromPicker?.ForceRefreshSettings();
        _toPicker?.ForceRefreshSettings();
    }

    private void UpdateLabels()
    {
        var loc = GetLocalization();

        if (_fromPicker != null)
            _fromPicker.Watermark = loc.FromLabel;
        if (_toPicker != null)
            _toPicker.Watermark = loc.ToLabel;
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

    // ── Preset handlers ──

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
        var today = GetToday();
        QuickRangeApplied?.Invoke(this, new QuickRangeSelectedEventArgs(today, today, "today"));
    }

    private void OnWeek_Click(object? sender, RoutedEventArgs e)
    {
        var today = GetToday();
        int dayOfWeek = today.DayOfWeek;

        // Calculate week start (Saturday for Persian, Sunday for Gregorian)
        int weekStartDow = IsGregorian ? 0 : 0; // Both use 0-based for the start calculation
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
            startDay += GetDaysInMonth(startYear, startMonth);
        }

        // Calculate week end (Friday for Persian, Saturday for Gregorian)
        int endDay = today.Day + (6 - dayOfWeek);
        int endMonth = today.Month;
        int endYear = today.Year;

        int daysInMonth = GetDaysInMonth(endYear, endMonth);
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

        var weekStart = MakeDate(startYear, startMonth, startDay);
        var weekEnd = MakeDate(endYear, endMonth, endDay);

        QuickRangeApplied?.Invoke(this, new QuickRangeSelectedEventArgs(weekStart, weekEnd, "week"));
    }

    private void OnPresetMonth_Click(object? sender, RoutedEventArgs e)
    {
        var today = GetToday();
        int daysInMonth = GetDaysInMonth(today.Year, today.Month);

        var monthStart = MakeDate(today.Year, today.Month, 1);
        var monthEnd = MakeDate(today.Year, today.Month, daysInMonth);

        QuickRangeApplied?.Invoke(this, new QuickRangeSelectedEventArgs(monthStart, monthEnd, "month"));
    }

    private void OnYear_Click(object? sender, RoutedEventArgs e)
    {
        var today = GetToday();
        int daysInLastMonth = GetDaysInMonth(today.Year, 12);

        var yearStart = MakeDate(today.Year, 1, 1);
        var yearEnd = MakeDate(today.Year, 12, daysInLastMonth);

        QuickRangeApplied?.Invoke(this, new QuickRangeSelectedEventArgs(yearStart, yearEnd, "year"));
    }

    // ── Group selectors ──

    private void OnGroupMonth_Click(object? sender, RoutedEventArgs e) => ShowMonthGrid();
    private void OnGroupSeason_Click(object? sender, RoutedEventArgs e) => ShowSeasonGrid();
    private void OnGroupHalfYear_Click(object? sender, RoutedEventArgs e) => ShowHalfYearGrid();
    private void OnBack_Click(object? sender, RoutedEventArgs e) => ShowGroupSelector();

    private void ShowGroupSelector() => Animate(showContent: false);
    private void ShowGroupContent() => Animate(showContent: true);

    public void ResetRow3()
    {
        if (_groupSelectorPanel != null)
        {
            _groupSelectorPanel.IsVisible = true;
            _groupSelectorPanel.IsHitTestVisible = true;
            _groupSelectorPanel.Opacity = 1;
        }
        if (_groupContentBorder != null)
        {
            _groupContentBorder.IsVisible = false;
            _groupContentBorder.IsHitTestVisible = false;
            _groupContentBorder.Opacity = 1;
        }
        if (_backButton != null)
        {
            _backButton.IsVisible = false;
            _backButton.IsHitTestVisible = false;
            _backButton.Opacity = 1;
        }
    }

    // ── Animations ──

    private Animation CreateFadeIn() => new()
    {
        Duration = TimeSpan.FromMilliseconds(200),
        FillMode = FillMode.Forward,
        Children =
        {
            new KeyFrame { Cue = new Cue(0), Setters = { new Setter(Visual.OpacityProperty, 0.0) } },
            new KeyFrame { Cue = new Cue(1), Setters = { new Setter(Visual.OpacityProperty, 1.0) } }
        }
    };

    private Animation CreateFadeOut() => new()
    {
        Duration = TimeSpan.FromMilliseconds(200),
        FillMode = FillMode.Forward,
        Children =
        {
            new KeyFrame { Cue = new Cue(0), Setters = { new Setter(Visual.OpacityProperty, 1.0) } },
            new KeyFrame { Cue = new Cue(1), Setters = { new Setter(Visual.OpacityProperty, 0.0) } }
        }
    };

    private async void Animate(bool showContent)
    {
        if (showContent)
        {
            var tasks = new List<System.Threading.Tasks.Task>();

            if (_groupSelectorPanel != null && _groupSelectorPanel.IsVisible)
            {
                _groupSelectorPanel.IsHitTestVisible = false;
                tasks.Add(CreateFadeOut().RunAsync(_groupSelectorPanel));
            }

            if (_groupContentBorder != null)
            {
                _groupContentBorder.IsVisible = true;
                _groupContentBorder.IsHitTestVisible = true;
                _groupContentBorder.Opacity = 0;
                tasks.Add(CreateFadeIn().RunAsync(_groupContentBorder));
            }

            if (_backButton != null)
            {
                _backButton.IsVisible = true;
                _backButton.IsHitTestVisible = true;
                _backButton.Opacity = 0;
                tasks.Add(CreateFadeIn().RunAsync(_backButton));
            }

            await System.Threading.Tasks.Task.WhenAll(tasks);

            if (_groupSelectorPanel != null)
            {
                _groupSelectorPanel.IsVisible = false;
                _groupSelectorPanel.Opacity = 1;
            }
        }
        else
        {
            var tasks = new List<System.Threading.Tasks.Task>();

            if (_groupContentBorder != null && _groupContentBorder.IsVisible)
            {
                _groupContentBorder.IsHitTestVisible = false;
                tasks.Add(CreateFadeOut().RunAsync(_groupContentBorder));
            }

            if (_backButton != null && _backButton.IsVisible)
            {
                _backButton.IsHitTestVisible = false;
                tasks.Add(CreateFadeOut().RunAsync(_backButton));
            }

            if (_groupSelectorPanel != null)
            {
                _groupSelectorPanel.IsVisible = true;
                _groupSelectorPanel.IsHitTestVisible = true;
                _groupSelectorPanel.Opacity = 0;
                tasks.Add(CreateFadeIn().RunAsync(_groupSelectorPanel));
            }

            await System.Threading.Tasks.Task.WhenAll(tasks);

            if (_groupContentBorder != null)
            {
                _groupContentBorder.IsVisible = false;
                _groupContentBorder.Opacity = 1;
            }
            if (_backButton != null)
            {
                _backButton.IsVisible = false;
                _backButton.Opacity = 1;
            }
        }
    }

    // ── Month grid ──

    private void ShowMonthGrid()
    {
        if (_groupContentPanel == null) return;

        ShowGroupContent();
        _groupContentPanel.Children.Clear();

        var loc = GetLocalization();
        var today = GetToday();
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*,*") };

        for (int i = 0; i < 4; i++)
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        int row = 0, col = 0;
        for (int month = 1; month <= 12; month++)
        {
            var monthName = IsGregorian
                ? PersianCalendarHelper.GetGregorianMonthName(month, loc)
                : loc.MonthNames[month];
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
        var today = GetToday();
        int daysInMonth = GetDaysInMonth(today.Year, month);

        var monthStart = MakeDate(today.Year, month, 1);
        var monthEnd = MakeDate(today.Year, month, daysInMonth);

        QuickRangeApplied?.Invoke(this, new QuickRangeSelectedEventArgs(monthStart, monthEnd, "month"));
    }

    // ── Season grid ──

    private void ShowSeasonGrid()
    {
        if (_groupContentPanel == null) return;

        ShowGroupContent();
        _groupContentPanel.Children.Clear();

        var loc = GetLocalization();
        var today = GetToday();
        int currentSeason = (today.Month - 1) / 3; // 0-3

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*,*,*") };
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        for (int i = 0; i < 4; i++)
        {
            var button = new Button
            {
                Content = loc.SeasonNames[i],
                [Grid.RowProperty] = 0,
                [Grid.ColumnProperty] = i,
                Margin = new Thickness(2),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            button.Classes.Add("month-year-button");

            int seasonIndex = i;
            button.Click += (s, e) => OnSeasonSelected(seasonIndex);

            grid.Children.Add(button);
        }

        _groupContentPanel.Children.Add(grid);
    }

    private void OnSeasonSelected(int seasonIndex)
    {
        var today = GetToday();
        int startMonth = seasonIndex * 3 + 1;
        int endMonth = startMonth + 2;
        int daysInEndMonth = GetDaysInMonth(today.Year, endMonth);

        var rangeStart = MakeDate(today.Year, startMonth, 1);
        var rangeEnd = MakeDate(today.Year, endMonth, daysInEndMonth);

        QuickRangeApplied?.Invoke(this, new QuickRangeSelectedEventArgs(rangeStart, rangeEnd, "season"));
    }

    // ── Half-year grid ──

    private void ShowHalfYearGrid()
    {
        if (_groupContentPanel == null) return;

        ShowGroupContent();
        _groupContentPanel.Children.Clear();

        var loc = GetLocalization();
        var today = GetToday();
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

            int halfIndex = i;
            button.Click += (s, e) => OnHalfYearSelected(halfIndex);

            grid.Children.Add(button);
        }

        _groupContentPanel.Children.Add(grid);
    }

    private void OnHalfYearSelected(int halfIndex)
    {
        var today = GetToday();
        int startMonth = halfIndex == 0 ? 1 : 7;
        int endMonth = halfIndex == 0 ? 6 : 12;
        int daysInEndMonth = GetDaysInMonth(today.Year, endMonth);

        var rangeStart = MakeDate(today.Year, startMonth, 1);
        var rangeEnd = MakeDate(today.Year, endMonth, daysInEndMonth);

        QuickRangeApplied?.Invoke(this, new QuickRangeSelectedEventArgs(rangeStart, rangeEnd, "halfyear"));
    }
}
