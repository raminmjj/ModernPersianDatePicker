using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using ModernPersianDatePicker.Localizations;

namespace ModernPersianDatePicker;

/// <summary>
/// The Quick Range panel of a range picker: a list of preset ranges, a Month / Season /
/// Half-year grid for any year, editable From / To pickers, and a footer with a summary
/// and Clear / Cancel / Apply. Clicking a preset or a period fills From / To; only Apply
/// commits the range (<see cref="QuickRangeApplied"/>).
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

    /// <summary>
    /// When true, the embedded From/To date pickers will display their built-in
    /// Hour/Minute/Second spinners inside their popups, and the Apply button will
    /// forward the user-selected times through <see cref="QuickRangeSelectedEventArgs"/>.
    /// Defaults to false so QuickRangeView stays date-only unless the host explicitly
    /// opts in (typically by mirroring the parent picker's IsRangeMode &amp;&amp; IsTimePickerEnabled).
    /// </summary>
    public static readonly StyledProperty<bool> IsTimePickerEnabledProperty =
        AvaloniaProperty.Register<QuickRangeView, bool>(nameof(IsTimePickerEnabled));

    /// <summary>
    /// Time-of-day component exposed to the embedded From picker when
    /// <see cref="IsTimePickerEnabled"/> is true. Updated from outside (typically by
    /// the host) so the From picker re-opens with the previously selected start time.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> StartTimeProperty =
        AvaloniaProperty.Register<QuickRangeView, TimeSpan>(nameof(StartTime), TimeSpan.Zero);

    /// <summary>
    /// Time-of-day component exposed to the embedded To picker when
    /// <see cref="IsTimePickerEnabled"/> is true.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> EndTimeProperty =
        AvaloniaProperty.Register<QuickRangeView, TimeSpan>(nameof(EndTime),
            new TimeSpan(23, 59, 59));

    // Events

    /// <summary>Apply: From and To are both set; carries the range, its type and the times.</summary>
    public event EventHandler<QuickRangeSelectedEventArgs>? QuickRangeApplied;

    /// <summary>Clear: the user asked for no range.</summary>
    public event EventHandler? QuickRangeCleared;

    /// <summary>Cancel: back to the calendar, nothing changed.</summary>
    public event EventHandler? QuickRangeCancelled;

    /// <summary>The period grid shown on the right: months, seasons or half-years.</summary>
    internal enum PeriodKind { Month, Season, HalfYear }

    // One preset or period: a key (QuickRangeSelectedEventArgs.RangeType), its range, the
    // text the summary shows for it, and the button that shows it.
    private sealed class RangeItem
    {
        public RangeItem(string key, PersianDate start, PersianDate end, string summary)
        {
            Key = key;
            Start = start;
            End = end;
            Summary = summary;
        }

        public string Key { get; }
        public PersianDate Start { get; }
        public PersianDate End { get; }
        public string Summary { get; }
        public Button? Button { get; set; }
    }

    // Template parts
    private ModernPersianDatePicker? _fromPicker;
    private ModernPersianDatePicker? _toPicker;
    private TextBlock? _fromLabel;
    private TextBlock? _toLabel;
    private Panel? _presetPanel;
    private TextBlock? _presetsHeader;
    private Button? _groupMonthButton;
    private Button? _groupSeasonButton;
    private Button? _groupHalfYearButton;
    private Button? _yearPreviousButton;
    private Button? _yearNextButton;
    private TextBlock? _yearText;
    private Panel? _groupContentPanel;
    private TextBlock? _summaryText;
    private Button? _applyButton;
    private Button? _cancelButton;
    private Button? _clearButton;

    // State
    private readonly List<RangeItem> _presets = new();
    private readonly List<RangeItem> _periods = new();
    private PeriodKind _periodKind = PeriodKind.Month;
    private int _displayYear;
    private bool _syncingPickers;

    static QuickRangeView()
    {
        LocalizationProviderProperty.Changed.AddClassHandler<QuickRangeView>((x, _) => { x.SyncPickersSettings(); x.Rebuild(); });
        CalendarTypeProperty.Changed.AddClassHandler<QuickRangeView>((x, _) => { x.SyncPickersSettings(); x.Rebuild(); });
        WeeklyHolidaysProperty.Changed.AddClassHandler<QuickRangeView>((x, _) => x.SyncPickersSettings());
        RangeStartProperty.Changed.AddClassHandler<QuickRangeView>((x, _) => x.SyncPickersFromRange());
        RangeEndProperty.Changed.AddClassHandler<QuickRangeView>((x, _) => x.SyncPickersFromRange());
        IsTimePickerEnabledProperty.Changed.AddClassHandler<QuickRangeView>((x, _) => x.SyncPickersSettings());
        StartTimeProperty.Changed.AddClassHandler<QuickRangeView>((x, _) => x.SyncPickersFromRange());
        EndTimeProperty.Changed.AddClassHandler<QuickRangeView>((x, _) => x.SyncPickersFromRange());
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

    /// <inheritdoc cref="IsTimePickerEnabledProperty"/>
    public bool IsTimePickerEnabled
    {
        get => GetValue(IsTimePickerEnabledProperty);
        set => SetValue(IsTimePickerEnabledProperty, value);
    }

    /// <inheritdoc cref="StartTimeProperty"/>
    public TimeSpan StartTime
    {
        get => GetValue(StartTimeProperty);
        set => SetValue(StartTimeProperty, value);
    }

    /// <inheritdoc cref="EndTimeProperty"/>
    public TimeSpan EndTime
    {
        get => GetValue(EndTimeProperty);
        set => SetValue(EndTimeProperty, value);
    }

    public IReadOnlyList<DayOfWeek> WeeklyHolidays
    {
        get => GetValue(WeeklyHolidaysProperty);
        set => SetValue(WeeklyHolidaysProperty, value);
    }

    // ── Read-only state ──

    /// <summary>The date in the From box (the range start once applied).</summary>
    public PersianDate? DraftStart => _fromPicker?.SelectedDate;

    /// <summary>The date in the To box (the range end once applied).</summary>
    public PersianDate? DraftEnd => _toPicker?.SelectedDate;

    /// <summary>The year the Month / Season / Half-year grid shows.</summary>
    public int DisplayYear => _displayYear;

    /// <summary>The footer summary, e.g. "Azar 1405 · 30 days".</summary>
    public string SummaryText => _summaryText?.Text ?? string.Empty;

    /// <summary>The key of the preset or period that matches From / To, or "custom".</summary>
    public string CurrentRangeType => FindMatch()?.Key ?? "custom";

    internal PeriodKind CurrentPeriodKind => _periodKind;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // Detach previous handlers
        if (_applyButton != null) _applyButton.Click -= OnApply_Click;
        if (_cancelButton != null) _cancelButton.Click -= OnCancel_Click;
        if (_clearButton != null) _clearButton.Click -= OnClear_Click;
        if (_groupMonthButton != null) _groupMonthButton.Click -= OnGroupMonth_Click;
        if (_groupSeasonButton != null) _groupSeasonButton.Click -= OnGroupSeason_Click;
        if (_groupHalfYearButton != null) _groupHalfYearButton.Click -= OnGroupHalfYear_Click;
        if (_yearPreviousButton != null) _yearPreviousButton.Click -= OnYearPrevious_Click;
        if (_yearNextButton != null) _yearNextButton.Click -= OnYearNext_Click;
        if (_fromPicker != null) _fromPicker.PropertyChanged -= OnPickerPropertyChanged;
        if (_toPicker != null) _toPicker.PropertyChanged -= OnPickerPropertyChanged;

        // Find template parts
        _fromPicker = e.NameScope.Find<ModernPersianDatePicker>("PART_FromPicker");
        _toPicker = e.NameScope.Find<ModernPersianDatePicker>("PART_ToPicker");
        _fromLabel = e.NameScope.Find<TextBlock>("PART_FromLabel");
        _toLabel = e.NameScope.Find<TextBlock>("PART_ToLabel");
        _presetPanel = e.NameScope.Find<Panel>("PART_PresetPanel");
        _presetsHeader = e.NameScope.Find<TextBlock>("PART_PresetsHeader");
        _groupMonthButton = e.NameScope.Find<Button>("PART_GroupMonthButton");
        _groupSeasonButton = e.NameScope.Find<Button>("PART_GroupSeasonButton");
        _groupHalfYearButton = e.NameScope.Find<Button>("PART_GroupHalfYearButton");
        _yearPreviousButton = e.NameScope.Find<Button>("PART_YearPreviousButton");
        _yearNextButton = e.NameScope.Find<Button>("PART_YearNextButton");
        _yearText = e.NameScope.Find<TextBlock>("PART_YearText");
        _groupContentPanel = e.NameScope.Find<Panel>("PART_GroupContentPanel");
        _summaryText = e.NameScope.Find<TextBlock>("PART_SummaryText");
        _applyButton = e.NameScope.Find<Button>("PART_ApplyButton");
        _cancelButton = e.NameScope.Find<Button>("PART_CancelButton");
        _clearButton = e.NameScope.Find<Button>("PART_ClearRangeButton");

        // Attach handlers
        if (_applyButton != null) _applyButton.Click += OnApply_Click;
        if (_cancelButton != null) _cancelButton.Click += OnCancel_Click;
        if (_clearButton != null) _clearButton.Click += OnClear_Click;
        if (_groupMonthButton != null) _groupMonthButton.Click += OnGroupMonth_Click;
        if (_groupSeasonButton != null) _groupSeasonButton.Click += OnGroupSeason_Click;
        if (_groupHalfYearButton != null) _groupHalfYearButton.Click += OnGroupHalfYear_Click;
        if (_yearPreviousButton != null) _yearPreviousButton.Click += OnYearPrevious_Click;
        if (_yearNextButton != null) _yearNextButton.Click += OnYearNext_Click;

        // Configure pickers
        foreach (var picker in new[] { _fromPicker, _toPicker })
        {
            if (picker == null)
                continue;
            picker.IsRangeMode = false;
            picker.IsEditable = true;
            picker.PropertyChanged += OnPickerPropertyChanged;
        }
        SyncPickersSettings();
        SyncPickersFromRange();

        ResetRow3();
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

    private PersianDate GetToday() => PersianCalendarHelper.TodayForCalendarType(GetEffectiveCalendarType());

    private int GetDaysInMonth(int year, int month)
        => IsGregorian ? DateTime.DaysInMonth(year, month) : PersianCalendarHelper.GetDaysInMonth(year, month);

    private PersianDate MakeDate(int year, int month, int day)
    {
        int firstDayOfWeek = IsGregorian
            ? PersianCalendarHelper.GetGregorianFirstDayOfWeek(year, month)
            : PersianCalendarHelper.GetFirstDayOfWeek(year, month);
        return new PersianDate(year, month, day, (firstDayOfWeek + day - 1) % 7);
    }

    private DateTime ToDateTime(PersianDate date)
        => ModernPersianDatePicker.ToDateTime(date, GetEffectiveCalendarType()) ?? DateTime.Today;

    private PersianDate AddDays(PersianDate date, int days)
        => ModernPersianDatePicker.FromDateTime(ToDateTime(date).AddDays(days), GetEffectiveCalendarType());

    /// <summary>The number of days from <paramref name="start"/> to <paramref name="end"/>, both included.</summary>
    private int DayCount(PersianDate start, PersianDate end)
        => Math.Abs((ToDateTime(end) - ToDateTime(start)).Days) + 1;

    private string MonthName(int month, ICalendarLocalization loc)
        => IsGregorian ? PersianCalendarHelper.GetGregorianMonthName(month, loc) : loc.MonthNames[month];

    private (PersianDate Start, PersianDate End) MonthRange(int year, int month)
        => (MakeDate(year, month, 1), MakeDate(year, month, GetDaysInMonth(year, month)));

    private (PersianDate Start, PersianDate End) MonthsRange(int year, int firstMonth, int lastMonth)
        => (MakeDate(year, firstMonth, 1), MakeDate(year, lastMonth, GetDaysInMonth(year, lastMonth)));

    // ── Pickers ──

    private void SyncPickersFromRange()
    {
        _syncingPickers = true;
        try
        {
            if (_fromPicker != null)
            {
                _fromPicker.SelectedDate = RangeStart;
                if (IsTimePickerEnabled)
                    _fromPicker.SelectedTime = StartTime;
            }
            if (_toPicker != null)
            {
                _toPicker.SelectedDate = RangeEnd;
                if (IsTimePickerEnabled)
                    _toPicker.SelectedTime = EndTime;
            }
        }
        finally
        {
            _syncingPickers = false;
        }
        UpdateSelection();
    }

    private void SyncPickersSettings()
    {
        foreach (var picker in new[] { _fromPicker, _toPicker })
        {
            if (picker == null)
                continue;
            picker.CalendarType = CalendarType;
            picker.LocalizationProvider = LocalizationProvider;
            picker.WeeklyHolidays = WeeklyHolidays;
            picker.IsTimePickerEnabled = IsTimePickerEnabled;
        }
    }

    private void OnPickerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ModernPersianDatePicker.SelectedDateProperty && !_syncingPickers)
            UpdateSelection();
    }

    /// <summary>Puts a range into From / To (a preset or period click); Apply commits it.</summary>
    private void SetDraft(PersianDate start, PersianDate end)
    {
        _syncingPickers = true;
        try
        {
            if (_fromPicker != null)
            {
                _fromPicker.SelectedDate = start;
                if (IsTimePickerEnabled)
                    _fromPicker.SelectedTime = DateRangeSelectedEventArgs.DefaultStartTime;
            }
            if (_toPicker != null)
            {
                _toPicker.SelectedDate = end;
                if (IsTimePickerEnabled)
                    _toPicker.SelectedTime = DateRangeSelectedEventArgs.DefaultEndTime;
            }
        }
        finally
        {
            _syncingPickers = false;
        }
        UpdateSelection();
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

    /// <summary>
    /// Resets the panel for a new opening: the grid shows the year of the current range
    /// (or this year) and the tab of its kind of period (a season range opens the Season
    /// tab). Kept under its old name: hosts call it when the panel is shown.
    /// </summary>
    public void ResetRow3()
    {
        var anchor = RangeStart ?? GetToday();
        _displayYear = anchor.Year;
        _periodKind = MatchingPeriodKind() ?? PeriodKind.Month;
        Rebuild();
    }

    private PeriodKind? MatchingPeriodKind()
    {
        if (RangeStart is not PersianDate start || RangeEnd is not PersianDate end || start.Year != end.Year)
            return null;

        var loc = GetLocalization();
        foreach (var kind in new[] { PeriodKind.Month, PeriodKind.Season, PeriodKind.HalfYear })
            foreach (var period in PeriodsFor(kind, start.Year, loc))
                if (period.Start == start && period.End == end)
                    return kind;
        return null;
    }

    // ── Building the lists ──

    private void Rebuild()
    {
        if (_displayYear == 0)
            _displayYear = GetToday().Year;

        UpdateLabels();
        BuildPresets();
        BuildPeriods();
        UpdateSelection();
    }

    private void UpdateLabels()
    {
        var loc = GetLocalization();

        if (_fromPicker != null)
            _fromPicker.Watermark = loc.FromLabel;
        if (_toPicker != null)
            _toPicker.Watermark = loc.ToLabel;
        if (_fromLabel != null)
            _fromLabel.Text = loc.FromLabel.TrimEnd('.', '…');
        if (_toLabel != null)
            _toLabel.Text = loc.ToLabel.TrimEnd('.', '…');
        if (_presetsHeader != null)
            _presetsHeader.Text = loc.PresetsLabel;
        if (_applyButton != null)
            _applyButton.Content = loc.ApplyLabel;
        if (_cancelButton != null)
            _cancelButton.Content = loc.CancelLabel;
        if (_clearButton != null)
            _clearButton.Content = loc.ClearLabel;
        if (_groupMonthButton != null)
            _groupMonthButton.Content = loc.MonthLabel;
        if (_groupSeasonButton != null)
            _groupSeasonButton.Content = loc.SeasonLabel;
        if (_groupHalfYearButton != null)
            _groupHalfYearButton.Content = loc.HalfYearLabel;
    }

    private void BuildPresets()
    {
        var loc = GetLocalization();
        var today = GetToday();

        // Week: Saturday..Friday (Persian) / Sunday..Saturday (Gregorian); the struct's
        // DayOfWeek is the 0-based index in that week in both calendars.
        var weekStart = AddDays(today, -today.DayOfWeek);
        var month = MonthRange(today.Year, today.Month);
        int lastMonthYear = today.Month == 1 ? today.Year - 1 : today.Year;
        int lastMonthNumber = today.Month == 1 ? 12 : today.Month - 1;
        var lastMonth = MonthRange(lastMonthYear, lastMonthNumber);
        var year = MonthsRange(today.Year, 1, 12);
        var yesterday = AddDays(today, -1);

        _presets.Clear();
        _presets.Add(new RangeItem("today", today, today, loc.TodayLabel));
        _presets.Add(new RangeItem("yesterday", yesterday, yesterday, loc.YesterdayLabel));
        _presets.Add(new RangeItem("week", weekStart, AddDays(weekStart, 6), loc.CurrentWeekLabel));
        _presets.Add(new RangeItem("last7days", AddDays(today, -6), today, loc.Last7DaysLabel));
        _presets.Add(new RangeItem("month", month.Start, month.End, loc.CurrentMonthLabel));
        _presets.Add(new RangeItem("lastmonth", lastMonth.Start, lastMonth.End, loc.LastMonthLabel));
        _presets.Add(new RangeItem("last30days", AddDays(today, -29), today, loc.Last30DaysLabel));
        _presets.Add(new RangeItem("year", year.Start, year.End, loc.CurrentYearLabel));

        if (_presetPanel == null)
            return;

        _presetPanel.Children.Clear();
        foreach (var preset in _presets)
        {
            var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
            grid.Children.Add(new TextBlock { Text = preset.Summary, VerticalAlignment = VerticalAlignment.Center });
            var count = new TextBlock
            {
                Text = loc.FormatDayCount(DayCount(preset.Start, preset.End)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 0, 0, 0),
                [Grid.ColumnProperty] = 1,
            };
            count.Classes.Add("qr-count");
            grid.Children.Add(count);

            var button = new Button
            {
                Content = grid,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Tag = preset.Key,
            };
            button.Classes.Add("qr-preset");
            var item = preset;
            button.Click += (_, _) => SetDraft(item.Start, item.End);
            preset.Button = button;
            _presetPanel.Children.Add(button);
        }
    }

    private List<RangeItem> PeriodsFor(PeriodKind kind, int year, ICalendarLocalization loc)
    {
        var list = new List<RangeItem>();
        switch (kind)
        {
            case PeriodKind.Month:
                for (int m = 1; m <= 12; m++)
                {
                    var r = MonthRange(year, m);
                    list.Add(new RangeItem("month", r.Start, r.End, $"{MonthName(m, loc)} {year}"));
                }
                break;
            case PeriodKind.Season:
                for (int i = 0; i < 4; i++)
                {
                    var r = MonthsRange(year, i * 3 + 1, i * 3 + 3);
                    list.Add(new RangeItem("season", r.Start, r.End, $"{loc.SeasonNames[i]} {year}"));
                }
                break;
            case PeriodKind.HalfYear:
                for (int i = 0; i < 2; i++)
                {
                    var r = MonthsRange(year, i * 6 + 1, i * 6 + 6);
                    list.Add(new RangeItem("halfyear", r.Start, r.End, $"{loc.HalfYearNames[i]} {year}"));
                }
                break;
        }
        return list;
    }

    private void BuildPeriods()
    {
        var loc = GetLocalization();
        var today = GetToday();
        _periods.Clear();
        _periods.AddRange(PeriodsFor(_periodKind, _displayYear, loc));

        if (_yearText != null)
            _yearText.Text = _displayYear.ToString(System.Globalization.CultureInfo.InvariantCulture);
        SetClass(_groupMonthButton, "selected", _periodKind == PeriodKind.Month);
        SetClass(_groupSeasonButton, "selected", _periodKind == PeriodKind.Season);
        SetClass(_groupHalfYearButton, "selected", _periodKind == PeriodKind.HalfYear);

        if (_groupContentPanel == null)
            return;

        var grid = new UniformGrid { Columns = _periodKind == PeriodKind.HalfYear ? 2 : 4 };
        for (int i = 0; i < _periods.Count; i++)
        {
            var period = _periods[i];
            string text = _periodKind switch
            {
                PeriodKind.Month => MonthName(i + 1, loc),
                PeriodKind.Season => loc.SeasonNames[i],
                _ => loc.HalfYearNames[i],
            };
            var button = new Button
            {
                Content = text,
                Margin = new Thickness(3),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Tag = period.Key,
            };
            button.Classes.Add("qr-period");
            // A period that has not started yet: still clickable (planning), but muted.
            if (period.Start > today)
                button.Classes.Add("future");
            var item = period;
            button.Click += (_, _) => SetDraft(item.Start, item.End);
            period.Button = button;
            grid.Children.Add(button);
        }

        _groupContentPanel.Children.Clear();
        _groupContentPanel.Children.Add(grid);
    }

    private static void SetClass(Control? control, string name, bool on)
    {
        if (control == null)
            return;
        if (on)
        {
            if (!control.Classes.Contains(name))
                control.Classes.Add(name);
        }
        else
        {
            control.Classes.Remove(name);
        }
    }

    // ── Selection and summary ──

    /// <summary>The From / To dates in order (the boxes may hold them reversed), or null.</summary>
    private (PersianDate Start, PersianDate End)? OrderedDraft()
    {
        if (DraftStart is not PersianDate from || DraftEnd is not PersianDate to)
            return null;
        return to < from ? (to, from) : (from, to);
    }

    private RangeItem? FindMatch()
    {
        if (OrderedDraft() is not { } draft)
            return null;
        foreach (var item in _presets)
            if (item.Start == draft.Start && item.End == draft.End)
                return item;
        foreach (var item in _periods)
            if (item.Start == draft.Start && item.End == draft.End)
                return item;
        return null;
    }

    private void UpdateSelection()
    {
        var draft = OrderedDraft();
        var match = FindMatch();

        // Every item whose range is the draft is highlighted (e.g. "Current month" AND
        // that month in the grid).
        foreach (var item in _presets)
            SetClass(item.Button, "selected", draft is { } d && item.Start == d.Start && item.End == d.End);
        foreach (var item in _periods)
            SetClass(item.Button, "selected", draft is { } d && item.Start == d.Start && item.End == d.End);

        if (_summaryText != null)
        {
            var loc = GetLocalization();
            _summaryText.Text = draft is { } r
                ? (match != null ? match.Summary + " · " : string.Empty) + loc.FormatDayCount(DayCount(r.Start, r.End))
                : string.Empty;
        }
    }

    // ── Buttons ──

    private void OnGroupMonth_Click(object? sender, RoutedEventArgs e) => ShowPeriods(PeriodKind.Month);
    private void OnGroupSeason_Click(object? sender, RoutedEventArgs e) => ShowPeriods(PeriodKind.Season);
    private void OnGroupHalfYear_Click(object? sender, RoutedEventArgs e) => ShowPeriods(PeriodKind.HalfYear);
    private void OnYearPrevious_Click(object? sender, RoutedEventArgs e) => ShowYear(_displayYear - 1);
    private void OnYearNext_Click(object? sender, RoutedEventArgs e) => ShowYear(_displayYear + 1);

    internal void ShowPeriods(PeriodKind kind)
    {
        _periodKind = kind;
        BuildPeriods();
        UpdateSelection();
    }

    internal void ShowYear(int year)
    {
        if (year < 1 || year > 9998)
            return;
        _displayYear = year;
        BuildPeriods();
        UpdateSelection();
    }

    /// <summary>Clicks the preset with this key (see <see cref="QuickRangeSelectedEventArgs.RangeType"/>).</summary>
    internal bool SelectPreset(string key)
    {
        foreach (var preset in _presets)
        {
            if (preset.Key == key)
            {
                SetDraft(preset.Start, preset.End);
                return true;
            }
        }
        return false;
    }

    /// <summary>Clicks the period at <paramref name="index"/> in the grid (0-based).</summary>
    internal void SelectPeriod(int index) => SetDraft(_periods[index].Start, _periods[index].End);

    internal IReadOnlyList<Button> PresetButtons => ButtonsOf(_presets);

    internal IReadOnlyList<Button> PeriodButtons => ButtonsOf(_periods);

    private static List<Button> ButtonsOf(List<RangeItem> items)
    {
        var list = new List<Button>();
        foreach (var item in items)
            if (item.Button != null)
                list.Add(item.Button);
        return list;
    }

    internal void Apply() => OnApply_Click(this, new RoutedEventArgs());
    internal void Cancel() => OnCancel_Click(this, new RoutedEventArgs());
    internal void ClearDraft() => OnClear_Click(this, new RoutedEventArgs());

    private void OnApply_Click(object? sender, RoutedEventArgs e)
    {
        var from = _fromPicker?.SelectedDate;
        var to = _toPicker?.SelectedDate;

        if (!from.HasValue || !to.HasValue)
            return;

        bool swapped = to.Value < from.Value;
        var start = swapped ? to.Value : from.Value;
        var end = swapped ? from.Value : to.Value;

        // Read the time-of-day from the picker that contributed the start/end date.
        // When the user swapped the dates (typed the later date in From), keep the
        // time-of-day attached to its original picker so the user's intent is preserved.
        TimeSpan startTime;
        TimeSpan endTime;
        if (IsTimePickerEnabled)
        {
            var startPicker = swapped ? _toPicker : _fromPicker;
            var endPicker = swapped ? _fromPicker : _toPicker;
            startTime = startPicker?.SelectedTime ?? DateRangeSelectedEventArgs.DefaultStartTime;
            endTime = endPicker?.SelectedTime ?? DateRangeSelectedEventArgs.DefaultEndTime;
        }
        else
        {
            startTime = DateRangeSelectedEventArgs.DefaultStartTime;
            endTime = DateRangeSelectedEventArgs.DefaultEndTime;
        }

        QuickRangeApplied?.Invoke(this,
            new QuickRangeSelectedEventArgs(start, end, CurrentRangeType, startTime, endTime));
    }

    private void OnCancel_Click(object? sender, RoutedEventArgs e)
    {
        // Drop the draft: the next opening starts from the committed range.
        SyncPickersFromRange();
        QuickRangeCancelled?.Invoke(this, EventArgs.Empty);
    }

    private void OnClear_Click(object? sender, RoutedEventArgs e)
    {
        _syncingPickers = true;
        try
        {
            _fromPicker?.Clear();
            _toPicker?.Clear();
        }
        finally
        {
            _syncingPickers = false;
        }
        UpdateSelection();
        QuickRangeCleared?.Invoke(this, EventArgs.Empty);
    }
}
