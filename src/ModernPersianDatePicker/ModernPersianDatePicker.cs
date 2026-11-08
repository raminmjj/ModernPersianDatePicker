using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using ModernPersianDatePicker.Localizations;

namespace ModernPersianDatePicker;

/// <summary>
/// Modern Persian Date Picker control for Avalonia UI
/// Provides a calendar dropdown for selecting Persian (Jalali/Shamsi) dates
/// </summary>
public class ModernPersianDatePicker : TemplatedControl
{
    /// <summary>
    /// The picker's look is found by THIS type, so a subclass (an application's own
    /// date field) gets the picker's ControlTheme - and the picker's type selectors -
    /// without overriding anything itself.
    /// </summary>
    protected override Type StyleKeyOverride => typeof(ModernPersianDatePicker);

    // Styled Properties
    public static readonly StyledProperty<PersianDate?> SelectedDateProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, PersianDate?>(nameof(SelectedDate));

    public static readonly StyledProperty<string> DisplayFormatProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, string>(nameof(DisplayFormat), "long");

    public static readonly StyledProperty<CalendarLanguage> LanguageProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, CalendarLanguage>(nameof(Language));

    public static readonly StyledProperty<ICalendarLocalization?> LocalizationProviderProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, ICalendarLocalization?>(nameof(LocalizationProvider));

    public static readonly StyledProperty<bool> IsEditableProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, bool>(nameof(IsEditable));

    public static readonly StyledProperty<InvalidValueAction> InvalidValueActionProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, InvalidValueAction>(nameof(InvalidValueAction), InvalidValueAction.SetToNull);

    public static readonly StyledProperty<PersianDate?> MinDateProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, PersianDate?>(nameof(MinDate));

    public static readonly StyledProperty<PersianDate?> MaxDateProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, PersianDate?>(nameof(MaxDate));

    public static readonly StyledProperty<string> WatermarkProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, string>(nameof(Watermark), DefaultWatermark);

    /// <summary>
    /// <see cref="Watermark"/>'s default: "no watermark of the application's own" -
    /// the field then shows the localized "select a date" label of its language.
    /// </summary>
    private const string DefaultWatermark = "Select date...";

    /// <summary>
    /// Accent brush used for the selected day background, today border and focus border.
    /// When null (default), the theme's built-in accent color is used.
    /// </summary>
    public static readonly StyledProperty<IBrush?> AccentBrushProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, IBrush?>(nameof(AccentBrush));

    /// <summary>
    /// How this picker resolves its Light/Dark theme. Defaults to <see cref="ThemeMode.System"/>,
    /// which tracks the operating system setting and updates live.
    /// </summary>
    public static readonly new StyledProperty<ThemeMode> ThemeProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, ThemeMode>(nameof(Theme), ThemeMode.System);

    /// <summary>
    /// Override brush for holiday day numbers. When null, the theme's built-in holiday color is used.
    /// </summary>
    public static readonly StyledProperty<IBrush?> HolidayBrushProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, IBrush?>(nameof(HolidayBrush));

    /// <summary>
    /// Days of the week (.NET <see cref="DayOfWeek"/>) treated as recurring weekly holidays
    /// (e.g. <see cref="DayOfWeek.Friday"/> for the Persian weekend). Defaults to Friday.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<DayOfWeek>> WeeklyHolidaysProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, IReadOnlyList<DayOfWeek>>(nameof(WeeklyHolidays),
            new[] { DayOfWeek.Friday });

    /// <summary>
    /// Specific calendar dates to mark as holidays, in addition to <see cref="WeeklyHolidays"/>.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<PersianDate>> HolidaysProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, IReadOnlyList<PersianDate>>(nameof(Holidays),
            Array.Empty<PersianDate>());

    /// <summary>
    /// When true, enables date range selection (click start, click end). Defaults to false.
    /// </summary>
    public static readonly StyledProperty<bool> IsRangeModeProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, bool>(nameof(IsRangeMode));

    /// <summary>
    /// Start of the selected date range. Only meaningful when <see cref="IsRangeMode"/> is true.
    /// </summary>
    public static readonly StyledProperty<PersianDate?> RangeStartProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, PersianDate?>(nameof(RangeStart));

    /// <summary>
    /// End of the selected date range. Only meaningful when <see cref="IsRangeMode"/> is true.
    /// </summary>
    public static readonly StyledProperty<PersianDate?> RangeEndProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, PersianDate?>(nameof(RangeEnd));

    /// <summary>
    /// When true, displays Hour/Minute/Second spinners below the calendar in the popup.
    /// Defaults to false.
    /// </summary>
    public static readonly StyledProperty<bool> IsTimePickerEnabledProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, bool>(nameof(IsTimePickerEnabled));

    /// <summary>
    /// Selected time-of-day. Only meaningful when <see cref="IsTimePickerEnabled"/> is true
    /// (single-date mode) or when <see cref="IsRangeMode"/> &amp;&amp; <see cref="IsTimePickerEnabled"/>
    /// are both true (range mode captures time inside the From/To pickers of the Quick Range
    /// panel, surfaced back as <see cref="RangeStartTime"/>/<see cref="RangeEndTime"/>).
    /// Defaults to 00:00:00.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> SelectedTimeProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, TimeSpan>(nameof(SelectedTime),
            TimeSpan.Zero,
            validate: t => t >= TimeSpan.Zero && t < TimeSpan.FromDays(1));

    /// <summary>
    /// Time-of-day component for the range start. Only meaningful when
    /// <see cref="IsRangeMode"/> &amp;&amp; <see cref="IsTimePickerEnabled"/> are both true.
    /// Defaults to 00:00:00. Updated automatically when a quick range with explicit
    /// start time is applied from the Quick Range panel.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> RangeStartTimeProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, TimeSpan>(nameof(RangeStartTime),
            DateRangeSelectedEventArgs.DefaultStartTime);

    /// <summary>
    /// Time-of-day component for the range end. Only meaningful when
    /// <see cref="IsRangeMode"/> &amp;&amp; <see cref="IsTimePickerEnabled"/> are both true.
    /// Defaults to 23:59:59. Updated automatically when a quick range with explicit
    /// end time is applied from the Quick Range panel.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> RangeEndTimeProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, TimeSpan>(nameof(RangeEndTime),
            DateRangeSelectedEventArgs.DefaultEndTime);

    /// <summary>
    /// Calendar system to display. Defaults to <see cref="CalendarType.Persian"/>.
    /// </summary>
    public static readonly StyledProperty<CalendarType> CalendarTypeProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, CalendarType>(nameof(CalendarType));

    /// <summary>
    /// The selected date as a calendar-neutral <see cref="DateTime"/> (the date part;
    /// <see cref="DateTimeKind.Unspecified"/>), kept in sync with <see cref="SelectedDate"/>.
    /// Bind this instead of <see cref="SelectedDate"/> when the application stores real
    /// dates: <see cref="SelectedDate"/> holds Year/Month/Day in whichever calendar is
    /// active, so its numbers change meaning when the calendar (or, in
    /// <see cref="CalendarType.Auto"/> mode, the culture) changes. The time of day of a
    /// value set from outside is not interpreted; use <see cref="SelectedTime"/> for it.
    /// </summary>
    public static readonly StyledProperty<DateTime?> SelectedDateTimeProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, DateTime?>(
            nameof(SelectedDateTime), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// True while the editable text box holds text that is not a valid date (or is
    /// outside <see cref="MinDate"/>/<see cref="MaxDate"/>) and
    /// <see cref="InvalidValueAction"/> is <see cref="InvalidValueAction.Keep"/>:
    /// the text stays as typed and <see cref="SelectedDate"/> is NOT changed, so the
    /// application must not treat the old date as what the user entered. Reset by the
    /// next valid or empty entry, or when <see cref="SelectedDate"/> changes.
    /// </summary>
    /// <summary>
    /// The calendar <see cref="SelectedDate"/>, <see cref="RangeStart"/> and
    /// <see cref="RangeEnd"/> are expressed in right now (Persian or Gregorian -
    /// never <see cref="CalendarType.Auto"/>): the same struct means a different day in
    /// each. Changes when <see cref="CalendarType"/> changes, or - in Auto mode - with
    /// the culture (<see cref="RefreshCalendar"/>). To store a date, prefer
    /// <see cref="SelectedDateTime"/>.
    /// </summary>
    /// <summary>
    /// Vertical alignment of the text in the box (the template binds it). Defaults to
    /// <see cref="Avalonia.Layout.VerticalAlignment.Center"/> in the theme.
    /// </summary>
    public static readonly StyledProperty<Avalonia.Layout.VerticalAlignment> VerticalContentAlignmentProperty =
        ContentControl.VerticalContentAlignmentProperty.AddOwner<ModernPersianDatePicker>();

    public static readonly DirectProperty<ModernPersianDatePicker, CalendarType> SelectedDateCalendarProperty =
        AvaloniaProperty.RegisterDirect<ModernPersianDatePicker, CalendarType>(
            nameof(SelectedDateCalendar), o => o.SelectedDateCalendar);

    public static readonly DirectProperty<ModernPersianDatePicker, bool> HasInvalidInputProperty =
        AvaloniaProperty.RegisterDirect<ModernPersianDatePicker, bool>(
            nameof(HasInvalidInput), o => o.HasInvalidInput);

    // Events
    public event EventHandler<SelectedDateChangedEventArgs>? SelectedDateChanged;
    public event EventHandler<DateRangeSelectedEventArgs>? DateRangeSelected;
    /// <summary>Raised when <see cref="SelectedTime"/> changes (single-date time-of-day).</summary>
    public event EventHandler<SelectedTimeChangedEventArgs>? SelectedTimeChanged;
    /// <summary>Raised when <see cref="RangeStartTime"/> or <see cref="RangeEndTime"/> change.</summary>
    public event EventHandler? RangeTimeChanged;

    // Private fields
    private Popup? _popup;
    private Control? _popupContent;
    private ToggleButton? _toggleButton;
    private TextBlock? _displayTextBlock;
    private TextBox? _editableTextBox;
    private CalendarView? _calendarView;
    private TimePickerView? _timePickerView;
    private bool _isPopupOpen;
    private bool _isDisposed;
    private bool _isUpdatingText;
    private const double MinimumPopupWidth = 280;
    private bool _hasInvalidInput;
    private bool _syncingDateTime;

    // The concrete calendar (Persian or Gregorian) that SelectedDate / RangeStart /
    // RangeEnd are currently expressed in. In Auto mode it is the culture's calendar at
    // the moment the values were last converted, so a later culture switch can convert
    // them (see RefreshCalendar).
    private CalendarType _valuesCalendar;

    /// <inheritdoc cref="VerticalContentAlignmentProperty"/>
    public Avalonia.Layout.VerticalAlignment VerticalContentAlignment
    {
        get => GetValue(VerticalContentAlignmentProperty);
        set => SetValue(VerticalContentAlignmentProperty, value);
    }

    /// <inheritdoc cref="SelectedDateCalendarProperty"/>
    public CalendarType SelectedDateCalendar
    {
        get => _valuesCalendar;
        private set => SetAndRaise(SelectedDateCalendarProperty, ref _valuesCalendar, value);
    }

    private static event EventHandler? CultureChangedRequested;

    // Constructors
    static ModernPersianDatePicker()
    {
        SelectedDateProperty.Changed.AddClassHandler<ModernPersianDatePicker>((x, e) => x.OnSelectedDateChanged(e));
        LanguageProperty.Changed.AddClassHandler<ModernPersianDatePicker>((x, e) => x.OnLocalizationChanged(e));
        LocalizationProviderProperty.Changed.AddClassHandler<ModernPersianDatePicker>((x, e) => x.OnLocalizationChanged(e));
        IsEditableProperty.Changed.AddClassHandler<ModernPersianDatePicker>((x, e) => x.OnIsEditableChanged(e));
        AccentBrushProperty.Changed.AddClassHandler<ModernPersianDatePicker>((x, e) => x.OnAccentBrushChanged(e));
        ThemeProperty.Changed.AddClassHandler<ModernPersianDatePicker>((x, e) => x.OnThemeChanged(e));
        HolidayBrushProperty.Changed.AddClassHandler<ModernPersianDatePicker>((x, e) => x.OnHolidayBrushChanged(e));
        WeeklyHolidaysProperty.Changed.AddClassHandler<ModernPersianDatePicker>((x, e) => x.OnHolidaysChanged(e));
        HolidaysProperty.Changed.AddClassHandler<ModernPersianDatePicker>((x, e) => x.OnHolidaysChanged(e));
        IsRangeModeProperty.Changed.AddClassHandler<ModernPersianDatePicker>((x, e) => x.OnRangeModeChanged(e));
        RangeStartProperty.Changed.AddClassHandler<ModernPersianDatePicker>((x, e) => x.OnRangeChanged(e));
        RangeEndProperty.Changed.AddClassHandler<ModernPersianDatePicker>((x, e) => x.OnRangeChanged(e));
        IsTimePickerEnabledProperty.Changed.AddClassHandler<ModernPersianDatePicker>((x, e) => x.OnTimePickerEnabledChanged(e));
        SelectedTimeProperty.Changed.AddClassHandler<ModernPersianDatePicker>((x, e) => x.OnSelectedTimeChanged(e));
        CalendarTypeProperty.Changed.AddClassHandler<ModernPersianDatePicker>((x, e) => x.OnCalendarTypeChanged(e));
        SelectedDateTimeProperty.Changed.AddClassHandler<ModernPersianDatePicker>((x, e) => x.OnSelectedDateTimeChanged(e));
        RangeStartTimeProperty.Changed.AddClassHandler<ModernPersianDatePicker>((x, e) => x.OnRangeTimeChanged(e));
        RangeEndTimeProperty.Changed.AddClassHandler<ModernPersianDatePicker>((x, e) => x.OnRangeTimeChanged(e));
        WatermarkProperty.Changed.AddClassHandler<ModernPersianDatePicker>((x, _) => x.ApplyWatermark());

        // ONE focus stop: an editable picker's text box takes the focus, so the control
        // itself is not focusable then - it used to be, NEXT to its text box (Tab and
        // Enter stopped twice per field, and Focus() landed where typing went nowhere).
        // Use FocusEditableText() to put the caret in the box.
        FocusableProperty.OverrideMetadata<ModernPersianDatePicker>(
            new StyledPropertyMetadata<bool>(
                coerce: (o, value) => value && !((ModernPersianDatePicker)o).IsEditable));
        IsEditableProperty.Changed.AddClassHandler<ModernPersianDatePicker>((x, _) => x.CoerceValue(FocusableProperty));
    }

    public ModernPersianDatePicker()
    {
        SelectedDateCalendar = GetEffectiveCalendarType();

        // Resolve the initial theme (system or explicit) for this instance.
        ApplyTheme();
        UpdateDisplayText();
    }

    // Public Properties
    public PersianDate? SelectedDate
    {
        get => GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    /// <inheritdoc cref="SelectedDateTimeProperty"/>
    public DateTime? SelectedDateTime
    {
        get => GetValue(SelectedDateTimeProperty);
        set => SetValue(SelectedDateTimeProperty, value);
    }

    /// <inheritdoc cref="HasInvalidInputProperty"/>
    public bool HasInvalidInput
    {
        get => _hasInvalidInput;
        private set => SetAndRaise(HasInvalidInputProperty, ref _hasInvalidInput, value);
    }

    /// <summary>
    /// Puts the caret in the editable text box (an editable picker's only focus stop);
    /// for a picker that is not editable, focuses the picker itself.
    /// </summary>
    public bool FocusEditableText()
        => IsEditable && _editableTextBox is { IsVisible: true } box ? box.Focus() : Focus();

    /// <summary>
    /// Parses the editable text box's current text now, exactly as Enter or losing focus
    /// would. Call it before saving when the save can be triggered without moving focus
    /// (e.g. a keyboard shortcut), so a just-typed date is not missed.
    /// </summary>
    public void CommitText()
    {
        if (_editableTextBox != null)
            ValidateAndParseInput(_editableTextBox.Text);
    }

    /// <summary>Parses <paramref name="text"/> as if it had been typed and committed.</summary>
    internal void CommitText(string? text) => ValidateAndParseInput(text);

    /// <summary>
    /// Re-resolves the effective calendar (relevant in <see cref="CalendarType.Auto"/>
    /// mode, which follows <see cref="System.Globalization.CultureInfo.CurrentUICulture"/>)
    /// and, when it changed, converts <see cref="SelectedDate"/>, <see cref="RangeStart"/>
    /// and <see cref="RangeEnd"/> into it - the same day, new numbers - and re-renders.
    /// <see cref="SelectedDateTime"/> does not change. Every loaded picker does this when
    /// <see cref="NotifyCultureChanged"/> is called, and a picker does it when it loads.
    /// </summary>
    public void RefreshCalendar()
    {
        if (GetEffectiveCalendarType() != _valuesCalendar)
            ApplyEffectiveCalendar();
    }

    /// <summary>
    /// Tells every loaded picker that the application changed
    /// <see cref="System.Globalization.CultureInfo.CurrentUICulture"/>. .NET raises no event
    /// for that, so an application that switches language at runtime calls this right after
    /// setting the culture; pickers in <see cref="CalendarType.Auto"/> mode then switch
    /// calendar without changing the dates they hold. Call it on the UI thread.
    /// </summary>
    public static void NotifyCultureChanged() => CultureChangedRequested?.Invoke(null, EventArgs.Empty);

    private void OnCultureChangedRequested(object? sender, EventArgs e) => RefreshCalendar();

    public string DisplayFormat
    {
        get => GetValue(DisplayFormatProperty);
        set => SetValue(DisplayFormatProperty, value);
    }

    public CalendarLanguage Language
    {
        get => GetValue(LanguageProperty);
        set => SetValue(LanguageProperty, value);
    }

    public ICalendarLocalization? LocalizationProvider
    {
        get => GetValue(LocalizationProviderProperty);
        set => SetValue(LocalizationProviderProperty, value);
    }

    public bool IsEditable
    {
        get => GetValue(IsEditableProperty);
        set => SetValue(IsEditableProperty, value);
    }

    public InvalidValueAction InvalidValueAction
    {
        get => GetValue(InvalidValueActionProperty);
        set => SetValue(InvalidValueActionProperty, value);
    }

    public PersianDate? MinDate
    {
        get => GetValue(MinDateProperty);
        set => SetValue(MinDateProperty, value);
    }

    public PersianDate? MaxDate
    {
        get => GetValue(MaxDateProperty);
        set => SetValue(MaxDateProperty, value);
    }

    public string Watermark
    {
        get => GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }

    /// <summary>
    /// Accent brush used for the selected day background, today border and focus border.
    /// When null (default), the theme's built-in accent color is used.
    /// </summary>
    public IBrush? AccentBrush
    {
        get => GetValue(AccentBrushProperty);
        set => SetValue(AccentBrushProperty, value);
    }

    /// <summary>
    /// How this picker resolves its Light/Dark theme. Defaults to <see cref="ThemeMode.System"/>.
    /// Applied per-instance, independent of the application-wide theme.
    /// </summary>
    public new ThemeMode Theme
    {
        get => GetValue(ThemeProperty);
        set => SetValue(ThemeProperty, value);
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

    /// <summary>
    /// When true, enables date range selection (click start, click end). Defaults to false.
    /// </summary>
    public bool IsRangeMode
    {
        get => GetValue(IsRangeModeProperty);
        set => SetValue(IsRangeModeProperty, value);
    }

    /// <summary>
    /// Start of the selected date range. Only meaningful when <see cref="IsRangeMode"/> is true.
    /// </summary>
    public PersianDate? RangeStart
    {
        get => GetValue(RangeStartProperty);
        set => SetValue(RangeStartProperty, value);
    }

    /// <summary>
    /// End of the selected date range. Only meaningful when <see cref="IsRangeMode"/> is true.
    /// </summary>
    public PersianDate? RangeEnd
    {
        get => GetValue(RangeEndProperty);
        set => SetValue(RangeEndProperty, value);
    }

    /// <summary>
    /// When true, displays Hour/Minute/Second spinners below the calendar. Defaults to false.
    /// </summary>
    public bool IsTimePickerEnabled
    {
        get => GetValue(IsTimePickerEnabledProperty);
        set => SetValue(IsTimePickerEnabledProperty, value);
    }

    /// <summary>
    /// Selected time-of-day in the range [00:00:00, 23:59:59]. Only meaningful when
    /// <see cref="IsTimePickerEnabled"/> is true.
    /// </summary>
    public TimeSpan SelectedTime
    {
        get => GetValue(SelectedTimeProperty);
        set => SetValue(SelectedTimeProperty, value);
    }

    /// <summary>
    /// Time-of-day for the range start (00:00:00 by default). Only meaningful when
    /// <see cref="IsRangeMode"/> &amp;&amp; <see cref="IsTimePickerEnabled"/> are both true.
    /// </summary>
    public TimeSpan RangeStartTime
    {
        get => GetValue(RangeStartTimeProperty);
        set => SetValue(RangeStartTimeProperty, value);
    }

    /// <summary>
    /// Time-of-day for the range end (23:59:59 by default). Only meaningful when
    /// <see cref="IsRangeMode"/> &amp;&amp; <see cref="IsTimePickerEnabled"/> are both true.
    /// </summary>
    public TimeSpan RangeEndTime
    {
        get => GetValue(RangeEndTimeProperty);
        set => SetValue(RangeEndTimeProperty, value);
    }

    /// <summary>
    /// Calendar system to display. Defaults to <see cref="CalendarType.Persian"/>.
    /// </summary>
    public CalendarType CalendarType
    {
        get => GetValue(CalendarTypeProperty);
        set => SetValue(CalendarTypeProperty, value);
    }

    // Protected Methods
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // Detach previous event handlers safely
        if (_toggleButton != null)
            _toggleButton.Click -= OnToggleButton_Click;
        if (_popup != null)
            _popup.Closed -= OnPopup_Closed;
        if (_popupContent != null)
            _popupContent.PointerWheelChanged -= OnPopupContentPointerWheelChanged;

        if (_calendarView != null)
        {
            _calendarView.DateSelected -= OnCalendarView_DateSelected;
            _calendarView.DateRangeSelected -= OnCalendarView_DateRangeSelected;
            _calendarView.TodayClicked -= OnCalendarView_TodayClicked;
            _calendarView.QuickRangeApplied -= OnCalendarView_QuickRangeApplied;
            _calendarView.QuickRangeViewVisibilityChanged -= OnCalendarView_QuickRangeViewVisibilityChanged;
        }
        if (_timePickerView != null)
            _timePickerView.TimeChanged -= OnTimePickerView_TimeChanged;

        // Get template parts
        _popup = e.NameScope.Find<Popup>("PART_Popup");
        _toggleButton = e.NameScope.Find<ToggleButton>("PART_ToggleButton");
        _displayTextBlock = e.NameScope.Find<TextBlock>("PART_DisplayText");
        _editableTextBox = e.NameScope.Find<TextBox>("PART_EditableText");
        _calendarView = e.NameScope.Find<CalendarView>("PART_CalendarView");
        _timePickerView = e.NameScope.Find<TimePickerView>("PART_TimePickerView");

        // Attach event handlers
        if (_toggleButton != null)
        {
            _toggleButton.Click += OnToggleButton_Click;
        }

        if (_popup != null)
        {
            _popup.Closed += OnPopup_Closed;

            // Attach a wheel-event handler to the popup's content root (the Border).
            // This catches ALL wheel events within the popup — from CalendarView (month
            // navigation), from TimePickerView TextBoxes (time adjustment), and from
            // empty space. CalendarView and TimePickerView handlers run FIRST (as
            // children, during bubbling), so this only prevents unhandled events from
            // reaching the main window's ScrollViewer.
            _popupContent = _popup.Child;
            if (_popupContent != null)
                _popupContent.PointerWheelChanged += OnPopupContentPointerWheelChanged;
        }

        // Add preview click handler to display text
        if (_displayTextBlock != null)
        {
            _displayTextBlock.PointerPressed += OnDisplayText_PointerPressed;
        }

        // Handle editable text box
        if (_editableTextBox != null)
        {
            _editableTextBox.KeyDown += OnEditableText_KeyDown;
            _editableTextBox.LostFocus += OnEditableText_LostFocus;
        }

        if (_calendarView != null)
        {
            _calendarView.LocalizationProvider = GetLocalization();
            _calendarView.CalendarType = CalendarType;
            _calendarView.HolidayBrush = HolidayBrush;

            // Set holidays based on effective calendar type
            if (GetEffectiveCalendarType() == CalendarType.Gregorian)
                _calendarView.WeeklyHolidays = new[] { DayOfWeek.Saturday, DayOfWeek.Sunday };
            else
                _calendarView.WeeklyHolidays = WeeklyHolidays;

            _calendarView.Holidays = Holidays;
            _calendarView.IsRangeMode = IsRangeMode;
            _calendarView.RangeStart = RangeStart;
            _calendarView.RangeEnd = RangeEnd;
            // Mirror the time-picker-enabled flag and current range times into the
            // calendar view so the embedded Quick Range panel can show time spinners
            // on its From/To pickers and pre-populate them with the current values.
            _calendarView.IsTimePickerEnabled = IsRangeMode && IsTimePickerEnabled;
            _calendarView.RangeStartTime = RangeStartTime;
            _calendarView.RangeEndTime = RangeEndTime;
            _calendarView.DateSelected += OnCalendarView_DateSelected;
            _calendarView.DateRangeSelected += OnCalendarView_DateRangeSelected;
            _calendarView.TodayClicked += OnCalendarView_TodayClicked;
            _calendarView.QuickRangeApplied += OnCalendarView_QuickRangeApplied;
            _calendarView.QuickRangeViewVisibilityChanged += OnCalendarView_QuickRangeViewVisibilityChanged;
        }

        if (_timePickerView != null)
        {
            _timePickerView.Time = SelectedTime;
            _timePickerView.TimeChanged += OnTimePickerView_TimeChanged;
            // In range mode the parent picker's own TimePicker is intentionally
            // hidden — the time-of-day for the range is instead captured inside
            // the From/To pickers shown in the Quick Range panel.
            _timePickerView.IsVisible = !IsRangeMode && IsTimePickerEnabled;
        }

        UpdateEditMode();

        // Set initial watermark on editable TextBox
        if (_editableTextBox != null)
        {
            _editableTextBox.PlaceholderText = EffectiveWatermark();
        }

        UpdateDisplayText();
    }

    private void OnDisplayText_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (!_isPopupOpen)
        {
            OpenPopup();
            e.Handled = true;
        }
    }

    private void OnEditableText_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Enter && _editableTextBox != null)
        {
            ValidateAndParseInput(_editableTextBox.Text);
            e.Handled = true;
        }
    }

    private void OnEditableText_LostFocus(object? sender, FocusChangedEventArgs e)
    {
        if (_editableTextBox != null)
        {
            ValidateAndParseInput(_editableTextBox.Text);
        }
    }

    private void ValidateAndParseInput(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            // An empty box is not an invalid value: it means "no date" - except under
            // SetToToday, whose contract is "never empty". (Keep used to keep the old
            // date here, so a cleared box could not clear the value.)
            SelectedDate = InvalidValueAction == InvalidValueAction.SetToToday
                ? PersianCalendarHelper.TodayForCalendarType(CalendarType)
                : null;
            HasInvalidInput = false;
            return;
        }

        var parsedDate = ParsePersianDate(input);

        if (parsedDate.HasValue)
        {
            // Validate min/max date
            if (MinDate.HasValue && parsedDate.Value < MinDate.Value)
            {
                HandleInvalidInput();
                return;
            }
            if (MaxDate.HasValue && parsedDate.Value > MaxDate.Value)
            {
                HandleInvalidInput();
                return;
            }

            SelectedDate = parsedDate;
            // Also when the date did not change (re-typing the same date after an
            // invalid entry raises no SelectedDate change).
            HasInvalidInput = false;
        }
        else
        {
            HandleInvalidInput();
        }
    }

    private void HandleInvalidInput()
    {
        switch (InvalidValueAction)
        {
            case InvalidValueAction.SetToToday:
                SelectedDate = PersianCalendarHelper.TodayForCalendarType(CalendarType);
                break;
            case InvalidValueAction.SetToNull:
                SelectedDate = null;
                break;
            case InvalidValueAction.Keep:
                // Keep the typed text and the current selected date - and say so: the
                // application must not take the old date as what the user entered.
                HasInvalidInput = true;
                break;
        }
    }

    private PersianDate? ParsePersianDate(string input)
    {
        try
        {
            // Split date and time parts (time is optional)
            var dateTimeParts = NormalizeDigits(input).Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var datePart = dateTimeParts[0];

            // Normalize separators
            var normalized = datePart.Replace('-', '/').Replace('_', '/').Trim();
            var parts = normalized.Split('/');

            if (parts.Length != 3)
                return null;

            // Parse year
            if (!int.TryParse(parts[0].Trim(), out int year))
                return null;

            // Handle 2-digit years
            if (year >= 0 && year < 100)
            {
                year = GetEffectiveCalendarType() == CalendarType.Gregorian ? 2000 + year : 1400 + year;
            }

            // Parse month
            if (!int.TryParse(parts[1].Trim(), out int month) || month < 1 || month > 12)
                return null;

            // Parse day
            if (!int.TryParse(parts[2].Trim(), out int day) || day < 1 || day > 31)
                return null;

            // Validate day for the specific month
            int daysInMonth = GetEffectiveCalendarType() == CalendarType.Gregorian
                ? DateTime.DaysInMonth(year, month)
                : PersianCalendarHelper.GetDaysInMonth(year, month);
            if (day > daysInMonth)
                return null;

            // Parse time component if present
            if (IsTimePickerEnabled && dateTimeParts.Length > 1)
            {
                var timeParts = dateTimeParts[1].Split(':');
                int h = 0, m = 0, s = 0;
                if (timeParts.Length >= 2)
                {
                    if (int.TryParse(timeParts[0], out int parsedH) && parsedH is >= 0 and <= 23)
                        h = parsedH;
                    if (int.TryParse(timeParts[1], out int parsedM) && parsedM is >= 0 and <= 59)
                        m = parsedM;
                }
                if (timeParts.Length >= 3)
                {
                    if (int.TryParse(timeParts[2], out int parsedS) && parsedS is >= 0 and <= 59)
                        s = parsedS;
                }
                SelectedTime = new TimeSpan(h, m, s);
            }

            // Calculate day of week
            var firstDayOfWeek = GetEffectiveCalendarType() == CalendarType.Gregorian
                ? PersianCalendarHelper.GetGregorianFirstDayOfWeek(year, month)
                : PersianCalendarHelper.GetFirstDayOfWeek(year, month);
            var dayOfWeek = (firstDayOfWeek + (day - 1)) % 7;

            return new PersianDate(year, month, day, dayOfWeek);
        }
        catch
        {
            return null;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Handled)
            return;

        switch (e.Key)
        {
            case Key.Down:
            case Key.F4:
                if (!_isPopupOpen)
                {
                    OpenPopup();
                    e.Handled = true;
                }
                break;

            case Key.Escape:
                if (_isPopupOpen)
                {
                    ClosePopup();
                    e.Handled = true;
                }
                break;

            case Key.Enter:
                if (_isPopupOpen)
                {
                    ClosePopup();
                    e.Handled = true;
                }
                else if (IsEditable && _editableTextBox != null)
                {
                    // Try to parse manual input
                    ValidateAndParseInput(_editableTextBox.Text);
                    e.Handled = true;
                }
                break;

            case Key.Space:
                if (!_isPopupOpen)
                {
                    OpenPopup();
                    e.Handled = true;
                }
                break;
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _isDisposed = true;
        CultureChangedRequested -= OnCultureChangedRequested;

        // Clean up event handlers
        if (_toggleButton != null)
        {
            _toggleButton.Click -= OnToggleButton_Click;
        }

        if (_displayTextBlock != null)
        {
            _displayTextBlock.PointerPressed -= OnDisplayText_PointerPressed;
        }

        if (_editableTextBox != null)
        {
            _editableTextBox.KeyDown -= OnEditableText_KeyDown;
            _editableTextBox.LostFocus -= OnEditableText_LostFocus;
        }

        if (_popup != null)
        {
            _popup.IsOpen = false;
        }
    }

    protected override void OnAttachedToLogicalTree(Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        // A forced Light / Dark resolves its colours from the tree it joined (it may
        // re-define a key); System has nothing to resolve.
        if (Theme != ThemeMode.System)
            ApplyTheme();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _isDisposed = false;

        CultureChangedRequested -= OnCultureChangedRequested;
        CultureChangedRequested += OnCultureChangedRequested;
        // The culture may have changed while this picker was not loaded.
        RefreshCalendar();

        // Re-attach event handlers that were detached in OnUnloaded.
        // Detach first to avoid double-attachment if OnApplyTemplate also runs.
        if (_toggleButton != null)
        {
            _toggleButton.Click -= OnToggleButton_Click;
            _toggleButton.Click += OnToggleButton_Click;
        }
        if (_displayTextBlock != null)
        {
            _displayTextBlock.PointerPressed -= OnDisplayText_PointerPressed;
            _displayTextBlock.PointerPressed += OnDisplayText_PointerPressed;
        }
        if (_editableTextBox != null)
        {
            _editableTextBox.KeyDown -= OnEditableText_KeyDown;
            _editableTextBox.KeyDown += OnEditableText_KeyDown;
            _editableTextBox.LostFocus -= OnEditableText_LostFocus;
            _editableTextBox.LostFocus += OnEditableText_LostFocus;
        }
        if (_popup != null)
        {
            _popup.Closed -= OnPopup_Closed;
            _popup.Closed += OnPopup_Closed;
        }

        UpdateEditMode();
        UpdateDisplayText();
    }

    // Private Methods
    private void OnSelectedDateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        // Property state, not visuals: kept in step even while unloaded (a view model
        // may set the date while its page is hidden).
        HasInvalidInput = false;
        SyncDateTimeFromSelectedDate();

        if (_isDisposed)
            return;

        UpdateDisplayText();

        var oldValue = e.OldValue as PersianDate?;
        var newValue = e.NewValue as PersianDate?;

        SelectedDateChanged?.Invoke(this, new SelectedDateChangedEventArgs(oldValue, newValue));
    }

    private void OnLocalizationChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_isDisposed)
            return;

        if (_calendarView != null)
            _calendarView.LocalizationProvider = GetLocalization();

        // Update editable TextBox watermark to reflect current language
        if (_editableTextBox != null)
        {
            _editableTextBox.PlaceholderText = EffectiveWatermark();
        }

        UpdateDisplayText();
    }

    private ICalendarLocalization GetLocalization()
    {
        return LocalizationProvider ?? Language switch
        {
            CalendarLanguage.English => new Localizations.EnglishLocalization(),
            CalendarLanguage.Arabic => new Localizations.ArabicLocalization(),
            CalendarLanguage.Kurdish => new Localizations.KurdishLocalization(),
            _ => new Localizations.FarsiLocalization()
        };
    }

    private CalendarType GetEffectiveCalendarType()
    {
        if (CalendarType != CalendarType.Auto)
            return CalendarType;

        var culture = System.Globalization.CultureInfo.CurrentUICulture;
        return culture.TwoLetterISOLanguageName is "fa" or "ps"
            ? CalendarType.Persian
            : CalendarType.Gregorian;
    }

    private void OnIsEditableChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_isDisposed)
            return;

        UpdateEditMode();
    }

    /// <summary>
    /// Toggles visibility of the display TextBlock and editable TextBox based on IsEditable.
    /// </summary>
    private void UpdateEditMode()
    {
        if (_isDisposed)
            return;

        if (_displayTextBlock != null)
            _displayTextBlock.IsVisible = !IsEditable;

        if (_editableTextBox != null)
            _editableTextBox.IsVisible = IsEditable;
    }

    /// <summary>
    /// When AccentBrush is set, override the theme accent resources locally so a single
    /// property can recolor the selected day, today border, focus border, etc.
    /// When cleared (null), remove the overrides to fall back to the theme.
    /// </summary>
    private void OnAccentBrushChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_isDisposed)
            return;

        var brush = e.NewValue as IBrush;
        UpdateAccentResources(brush);
    }

    /// <summary>
    /// Applies or clears local accent resource overrides.
    /// </summary>
    private void UpdateAccentResources(IBrush? accentBrush)
    {
        if (accentBrush == null)
        {
            // Fall back to theme defaults
            Resources.Remove("PersianDatePickerAccentBrush");
            Resources.Remove("PersianDatePickerAccentHoverBrush");
            Resources.Remove("PersianDatePickerFocusBorderBrush");
            Resources.Remove("PersianDatePickerSelectedFocusBackgroundBrush");
            return;
        }

        // Derive a darker hover shade when possible (SolidColorBrush only); otherwise reuse the same brush.
        IBrush hoverBrush = accentBrush;
        IBrush focusBrush = accentBrush;
        if (accentBrush is ISolidColorBrush solid)
        {
            hoverBrush = DarkenBrush(solid, 0.12);
        }

        Resources["PersianDatePickerAccentBrush"] = accentBrush;
        Resources["PersianDatePickerAccentHoverBrush"] = hoverBrush;
        Resources["PersianDatePickerFocusBorderBrush"] = focusBrush;
        Resources["PersianDatePickerSelectedFocusBackgroundBrush"] = hoverBrush;
    }

    /// <summary>
    /// Returns a new SolidColorBrush that is <paramref name="factor"/> darker than the source.
    /// </summary>
    private static IBrush DarkenBrush(ISolidColorBrush source, double factor)
    {
        byte r = (byte)Math.Round(source.Color.R * (1 - factor));
        byte g = (byte)Math.Round(source.Color.G * (1 - factor));
        byte b = (byte)Math.Round(source.Color.B * (1 - factor));
        return new SolidColorBrush(Color.FromArgb(source.Color.A, r, g, b), source.Opacity);
    }

    // ---------- Theme ----------

    private void OnThemeChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_isDisposed)
            return;

        ApplyTheme();
    }

    /// <summary>The colour keys of PersianDatePickerColors.xaml (every one the theme uses).</summary>
    internal static readonly string[] ColorKeys =
    {
        "PersianDatePickerBackgroundBrush",
        "PersianDatePickerBorderBrush",
        "PersianDatePickerTextForegroundBrush",
        "PersianDatePickerWatermarkForegroundBrush",
        "PersianDatePickerIconForegroundBrush",
        "PersianDatePickerAccentBrush",
        "PersianDatePickerAccentHoverBrush",
        "PersianDatePickerFocusBorderBrush",
        "PersianDatePickerPopupBackgroundBrush",
        "PersianDatePickerPopupBorderBrush",
        "PersianDatePickerHeaderForegroundBrush",
        "PersianDatePickerDayForegroundBrush",
        "PersianDatePickerHolidayForegroundBrush",
        "PersianDatePickerDayHoverBackgroundBrush",
        "PersianDatePickerSelectedForegroundBrush",
        "PersianDatePickerMonthYearHoverBackgroundBrush",
        "PersianDatePickerDayFocusBackgroundBrush",
        "PersianDatePickerSelectedFocusBorderBrush",
        "PersianDatePickerSelectedFocusBackgroundBrush",
    };

    /// <summary>
    /// Resolves <see cref="Theme"/>. System: no local colours - the picker follows the
    /// application's theme variant (and its popup with it). Light / Dark: each colour key
    /// is resolved for THAT variant - from PersianDatePickerColors.xaml, or from an
    /// application that re-defines the key - and pinned on this picker, so its template
    /// and its popup use it whatever the application's variant is.
    /// </summary>
    /// <remarks>
    /// It used to copy the Dark palette into C# hex values (a second palette to keep in
    /// step with the XAML), and "Light" only removed them - so a Light picker in a Dark
    /// application stayed dark. (A plain control cannot request a theme variant itself -
    /// only Application, TopLevel and ThemeVariantScope can.)
    /// </remarks>
    private void ApplyTheme()
    {
        foreach (var key in ColorKeys)
            Resources.Remove(key);

        if (Theme != ThemeMode.System)
        {
            var variant = Theme == ThemeMode.Dark ? ThemeVariant.Dark : ThemeVariant.Light;
            foreach (var key in ColorKeys)
            {
                // From the tree (a window or an application may re-define a key); a
                // picker not yet in a tree reaches the application directly.
                if (this.TryFindResource(key, variant, out var value)
                    || (Application.Current?.TryGetResource(key, variant, out value) ?? false))
                {
                    Resources[key] = value;
                }
            }
        }

        // The accent overrides the theme's accent keys (removed above).
        UpdateAccentResources(AccentBrush);
    }

    // ---------- Holidays ----------

    private void OnHolidayBrushChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_isDisposed)
            return;

        var brush = e.NewValue as IBrush;
        if (_calendarView != null)
            _calendarView.HolidayBrush = brush;
    }

    private void OnHolidaysChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_isDisposed)
            return;

        if (_calendarView != null)
        {
            if (e.Property == WeeklyHolidaysProperty)
                _calendarView.WeeklyHolidays = WeeklyHolidays;
            else
                _calendarView.Holidays = Holidays;
        }
    }

    private void OnRangeModeChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_isDisposed)
            return;

        if (_calendarView != null)
        {
            _calendarView.IsRangeMode = IsRangeMode;
            // Re-evaluate whether the embedded Quick Range pickers should expose time spinners
            _calendarView.IsTimePickerEnabled = IsRangeMode && IsTimePickerEnabled;
        }

        // In range mode the parent picker's own TimePicker is hidden; the time-of-day
        // for the range is captured by the From/To pickers inside the Quick Range panel.
        // In single-date mode the parent's TimePicker is shown when IsTimePickerEnabled.
        if (_timePickerView != null)
            _timePickerView.IsVisible = !IsRangeMode && IsTimePickerEnabled;

        UpdateDisplayText();
    }

    private void OnRangeChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_isDisposed)
            return;

        if (_calendarView != null)
        {
            if (e.Property == RangeStartProperty)
                _calendarView.RangeStart = RangeStart;
            else
                _calendarView.RangeEnd = RangeEnd;
        }

        UpdateDisplayText();
    }

    private void OnTimePickerEnabledChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_isDisposed)
            return;

        // In range mode the parent picker's own TimePicker is intentionally hidden;
        // time-of-day is captured by the From/To pickers inside the Quick Range panel.
        if (_timePickerView != null)
            _timePickerView.IsVisible = !IsRangeMode && IsTimePickerEnabled;

        // In range mode, IsTimePickerEnabled also toggles time spinners on the embedded
        // From/To pickers inside the Quick Range panel.
        if (_calendarView != null)
            _calendarView.IsTimePickerEnabled = IsRangeMode && IsTimePickerEnabled;

        UpdateDisplayText();
    }

    /// <summary>
    /// Propagates RangeStartTime / RangeEndTime changes to the embedded CalendarView
    /// so the Quick Range panel's From/To pickers stay in sync when reopened.
    /// </summary>
    private void OnRangeTimeChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_isDisposed)
            return;

        if (_calendarView != null)
        {
            if (e.Property == RangeStartTimeProperty)
                _calendarView.RangeStartTime = RangeStartTime;
            else if (e.Property == RangeEndTimeProperty)
                _calendarView.RangeEndTime = RangeEndTime;
        }

        UpdateDisplayText();

        RangeTimeChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnCalendarTypeChanged(AvaloniaPropertyChangedEventArgs e) => ApplyEffectiveCalendar();

    /// <summary>
    /// Converts the held values from the calendar they are expressed in
    /// (<see cref="_valuesCalendar"/>) to the current effective calendar and re-lays out.
    /// Used by a <see cref="CalendarType"/> change and by <see cref="RefreshCalendar"/>.
    /// Runs while unloaded too: the values must stay true even when nothing is shown.
    /// </summary>
    private void ApplyEffectiveCalendar()
    {
        // Resolve old and new calendar types to concrete Persian/Gregorian. The old one
        // is the calendar the values were last expressed in (tracked per instance), so an
        // Auto picker whose culture changed converts correctly.
        CalendarType newType = GetEffectiveCalendarType();
        CalendarType oldType = _valuesCalendar;
        // Set first: the conversions below re-sync SelectedDateTime in the NEW calendar.
        SelectedDateCalendar = newType;

        // Convert existing selected date(s) so the values stay semantically correct
        // after the calendar swap. Without this, picking 2026/07/31 Gregorian and
        // then switching to Persian would leave SelectedDate=(2026,7,31) — which is
        // then misinterpreted as Persian year 2026 (≈ 2646 Gregorian).
        if (oldType != newType)
        {
            if (SelectedDate.HasValue)
                SelectedDate = ConvertDate(SelectedDate.Value, oldType, newType);

            if (RangeStart.HasValue)
                RangeStart = ConvertDate(RangeStart.Value, oldType, newType);
            if (RangeEnd.HasValue)
                RangeEnd = ConvertDate(RangeEnd.Value, oldType, newType);

            // DisplayYear/DisplayMonth also need to be in the new calendar so the grid
            // opens on a sensible month rather than reusing the old calendar's numbers.
            // NOTE: SelectedDate and RangeStart have already been converted above, so use
            // them directly — do NOT call ConvertDate again (that would treat the new
            // values as if they were still in the old calendar and produce wrong results).
            var pivotDate = SelectedDate ?? RangeStart;
            if (pivotDate.HasValue && _calendarView != null)
            {
                _calendarView.DisplayYear = pivotDate.Value.Year;
                _calendarView.DisplayMonth = pivotDate.Value.Month;
            }
        }

        if (_calendarView != null)
        {
            _calendarView.CalendarType = CalendarType;

            // If we did NOT have a pivot date (no selection yet), fall back to today
            // in the new calendar so the grid opens on the current month.
            if (oldType == newType || !(SelectedDate.HasValue || RangeStart.HasValue))
            {
                if (newType == CalendarType.Gregorian)
                {
                    var today = DateTime.Today;
                    _calendarView.DisplayYear = today.Year;
                    _calendarView.DisplayMonth = today.Month;
                }
                else
                {
                    var today = PersianCalendarHelper.Today();
                    _calendarView.DisplayYear = today.Year;
                    _calendarView.DisplayMonth = today.Month;
                }
            }

            // Auto-set weekend holidays based on the new calendar.
            _calendarView.WeeklyHolidays = newType == CalendarType.Gregorian
                ? new[] { DayOfWeek.Saturday, DayOfWeek.Sunday }
                : new[] { DayOfWeek.Friday };
        }

        UpdateDisplayText();
    }


    private void OnSelectedDateTimeChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_syncingDateTime)
            return;

        _syncingDateTime = true;
        try
        {
            SelectedDate = SelectedDateTime is DateTime value ? FromDateTime(value, _valuesCalendar) : null;
        }
        finally
        {
            _syncingDateTime = false;
        }
    }

    private void SyncDateTimeFromSelectedDate()
    {
        if (_syncingDateTime)
            return;

        _syncingDateTime = true;
        try
        {
            SelectedDateTime = SelectedDate is PersianDate date ? ToDateTime(date, _valuesCalendar) : null;
        }
        finally
        {
            _syncingDateTime = false;
        }
    }

    /// <summary>The date a struct expressed in <paramref name="calendar"/> stands for; null when its values are not a valid date.</summary>
    internal static DateTime? ToDateTime(PersianDate date, CalendarType calendar)
    {
        try
        {
            return calendar == CalendarType.Gregorian
                ? PersianCalendarHelper.ToGregorianDateTime(date)
                : PersianCalendarHelper.ToGregorianDate(date);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    /// <summary>The struct for <paramref name="value"/>'s date in <paramref name="calendar"/> (Persian: Sat=0 week index; Gregorian: .NET DayOfWeek).</summary>
    internal static PersianDate FromDateTime(DateTime value, CalendarType calendar)
    {
        var day = value.Date;
        return calendar == CalendarType.Gregorian
            ? new PersianDate(day.Year, day.Month, day.Day, (int)day.DayOfWeek)
            : PersianCalendarHelper.ToPersianDate(day);
    }

    /// <summary>
    /// Replaces Persian (U+06F0..U+06F9) and Arabic-Indic (U+0660..U+0669) digits with
    /// ASCII digits: a Persian keyboard types them, and <see cref="int.TryParse(string?, out int)"/>
    /// does not accept them.
    /// </summary>
    internal static string NormalizeDigits(string input)
    {
        var chars = input.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            var c = chars[i];
            if (c is >= '۰' and <= '۹')
                chars[i] = (char)('0' + (c - '۰'));
            else if (c is >= '٠' and <= '٩')
                chars[i] = (char)('0' + (c - '٠'));
        }
        return new string(chars);
    }

    /// <summary>
    /// Converts a PersianDate struct from one calendar system to the other.
    /// The struct's DayOfWeek field is recomputed in the target calendar's convention
    /// (Persian week index Sat=0..Fri=6 for Persian; .NET DayOfWeek Sun=0..Sat=6 for Gregorian).
    /// </summary>
    /// <remarks>
    /// This method is defensive: if the source values are not actually valid in the
    /// <paramref name="from"/> calendar (which can happen when 'Auto' calendar type
    /// resolves incorrectly, or when prior state was loaded from a different calendar),
    /// it falls back to treating them as already being in the target calendar and
    /// merely recomputes the DayOfWeek. This prevents ArgumentOutOfRangeException
    /// crashes when e.g. switching Persian→Gregorian with a date like (2026, 7, 31)
    /// (Persian month 7 / Mehr only has 30 days, so PersianCalendar.ToDateTime would
    /// reject day=31).
    /// </remarks>
    private static PersianDate ConvertDate(PersianDate date, CalendarType from, CalendarType to)
    {
        if (from == to)
            return date;

        if (from == CalendarType.Persian && to == CalendarType.Gregorian)
        {
            // Validate the source date is actually a valid Persian date before
            // asking PersianCalendar to convert it. If not (e.g. day=31 in Mehr,
            // or month outside [1,12]), the values are likely already Gregorian —
            // fall back to identity (just recompute the DayOfWeek in .NET convention).
            bool invalidPersian = date.Month < 1 || date.Month > 12;
            if (!invalidPersian)
            {
                int persianDaysInMonth = PersianCalendarHelper.GetDaysInMonth(date.Year, date.Month);
                invalidPersian = date.Day < 1 || date.Day > persianDaysInMonth;
            }
            if (invalidPersian)
            {
                var fallback = PersianCalendarHelper.ToGregorianDateTime(date);
                return new PersianDate(date.Year, date.Month, date.Day, (int)fallback.DayOfWeek);
            }

            var dt = PersianCalendarHelper.ToGregorianDate(date);
            int dow = (int)dt.DayOfWeek; // .NET: Sun=0..Sat=6
            return new PersianDate(dt.Year, dt.Month, dt.Day, dow);
        }

        // from == Gregorian, to == Persian
        // The struct currently holds Gregorian values; build a DateTime and convert.
        // Validate the source date is a valid Gregorian date first.
        try
        {
            var gdt = PersianCalendarHelper.ToGregorianDateTime(date);
            return PersianCalendarHelper.ToPersianDate(gdt);
        }
        catch
        {
            // Values aren't valid Gregorian — return as-is.
            return date;
        }
    }

    private void OnSelectedTimeChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_isDisposed)
            return;

        if (_timePickerView != null)
            _timePickerView.Time = SelectedTime;

        UpdateDisplayText();

        var oldTime = (e.OldValue is TimeSpan ot) ? ot : TimeSpan.Zero;
        var newTime = (e.NewValue is TimeSpan nt) ? nt : TimeSpan.Zero;
        SelectedTimeChanged?.Invoke(this, new SelectedTimeChangedEventArgs(oldTime, newTime));
    }

    private void OnTimePickerView_TimeChanged(object? sender, EventArgs e)
    {
        if (_isDisposed || _timePickerView == null)
            return;

        _isUpdatingText = true;
        SelectedTime = _timePickerView.Time;
        _isUpdatingText = false;

        UpdateDisplayText();
    }

    private void OnToggleButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_isDisposed)
            return;

        if (_isPopupOpen)
            ClosePopup();
        else
            OpenPopup();

        e.Handled = true;
    }

    /// <summary>
    /// Handles wheel events at the popup content root level. Marks the event as
    /// handled so it doesn't propagate to the main window (which would scroll its
    /// ScrollViewer). CalendarView and TimePickerView handle their own wheel events
    /// first (their handlers run before this one during child→parent bubbling), so
    /// this only catches events from empty space / padding areas.
    /// </summary>
    private void OnPopupContentPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        e.Handled = true;
    }

    private void OnPopup_Closed(object? sender, EventArgs e)
    {
        if (_isDisposed)
            return;

        if (_popup != null)
            _popup.IsLightDismissEnabled = true;
        _isPopupOpen = false;
        if (_toggleButton != null)
            _toggleButton.IsChecked = false;
    }

    private void OnCalendarView_DateSelected(object? sender, DateSelectedEventArgs e)
    {
        if (_isDisposed)
            return;

        if (e.SelectedDate.HasValue)
        {
            SelectedDate = e.SelectedDate;
            ClosePopup();
        }
    }

    private void OnCalendarView_DateRangeSelected(object? sender, DateRangeSelectedEventArgs e)
    {
        if (_isDisposed)
            return;

        RangeStart = e.RangeStart;
        RangeEnd = e.RangeEnd;
        // Calendar-day clicks don't carry time info, so re-use whatever range times are
        // currently cached (default to start-of-day / end-of-day when never set).
        DateRangeSelected?.Invoke(this, new DateRangeSelectedEventArgs(
            e.RangeStart, e.RangeEnd, RangeStartTime, RangeEndTime));
        ClosePopup();
    }

    private void OnCalendarView_TodayClicked(object? sender, TodayClickedEventArgs e)
    {
        if (_isDisposed)
            return;

        // Select today and close popup
        SelectedDate = e.Today;
        ClosePopup();
    }

    private void OnCalendarView_QuickRangeApplied(object? sender, QuickRangeSelectedEventArgs e)
    {
        if (_isDisposed)
            return;

        RangeStart = e.RangeStart;
        RangeEnd = e.RangeEnd;
        // Cache the user-selected times so the parent's display text and the next
        // opening of the Quick Range panel both reflect them.
        RangeStartTime = e.StartTime;
        RangeEndTime = e.EndTime;
        DateRangeSelected?.Invoke(this, new DateRangeSelectedEventArgs(
            e.RangeStart, e.RangeEnd, e.StartTime, e.EndTime));
        ClosePopup();
    }

    private void OnCalendarView_QuickRangeViewVisibilityChanged(object? sender, bool isVisible)
    {
        // Light dismiss stays enabled at all times so clicking outside
        // the popup closes it normally.
    }

    private void OpenPopup()
    {
        if (_isDisposed || _popup == null || _calendarView == null)
            return;

        try
        {
            // Reset any stale state from a previous session
            _calendarView.ResetQuickRangeView();

            if (SelectedDate.HasValue)
            {
                _calendarView.DisplayYear = SelectedDate.Value.Year;
                _calendarView.DisplayMonth = SelectedDate.Value.Month;
            }
            else if (GetEffectiveCalendarType() == CalendarType.Gregorian)
            {
                var today = DateTime.Today;
                _calendarView.DisplayYear = today.Year;
                _calendarView.DisplayMonth = today.Month;
            }
            else
            {
                var today = PersianCalendarHelper.Today();
                _calendarView.DisplayYear = today.Year;
                _calendarView.DisplayMonth = today.Month;
            }
            _calendarView.SelectedDate = SelectedDate;

            // At least as wide as the field (it used to hang off a wide field's corner).
            if (_popup.Child is Control popupContent)
                popupContent.MinWidth = Math.Max(MinimumPopupWidth, Bounds.Width);

            _popup.PlacementTarget = this;
            _popup.Placement = Avalonia.Controls.PlacementMode.Bottom;
            _popup.IsOpen = true;
            _isPopupOpen = true;

            if (_toggleButton != null)
                _toggleButton.IsChecked = true;
                
            // Focus the calendar for keyboard navigation
            _calendarView.Focus();
        }
        catch (Exception)
        {
            // Silently handle any popup errors
            _isPopupOpen = false;
            if (_toggleButton != null)
                _toggleButton.IsChecked = false;
        }
    }

    private void ClosePopup()
    {
        if (_isDisposed || _popup == null)
            return;

        try
        {
            _popup.IsLightDismissEnabled = true;
            _popup.IsOpen = false;
            _isPopupOpen = false;

            if (_toggleButton != null)
                _toggleButton.IsChecked = false;
        }
        catch (Exception)
        {
            _popup.IsLightDismissEnabled = true;
            _isPopupOpen = false;
            if (_toggleButton != null)
                _toggleButton.IsChecked = false;
        }
    }

    /// <summary>
    /// The watermark the field shows: the application's own <see cref="Watermark"/>,
    /// or - when none was set - the localized "select a date" label. The editable
    /// text box used to show the localized label ALWAYS, so a custom watermark
    /// worked only in display mode.
    /// </summary>
    private string EffectiveWatermark()
        => string.IsNullOrEmpty(Watermark) || Watermark == DefaultWatermark
            ? GetLocalization().SelectDateLabel
            : Watermark;

    /// <summary>A new <see cref="Watermark"/> reaches the editable box and the display text.</summary>
    private void ApplyWatermark()
    {
        if (_isDisposed)
            return;
        if (_editableTextBox != null)
            _editableTextBox.PlaceholderText = EffectiveWatermark();
        UpdateDisplayText();
    }

    private void UpdateDisplayText()
    {
        if (_isDisposed)
            return;

        try
        {
            if (_isUpdatingText)
                return;

            _isUpdatingText = true;

            string displayText;
            if (IsRangeMode)
            {
                if (RangeStart.HasValue && RangeEnd.HasValue)
                {
                    displayText = $"{RangeStart.Value.ToString("short")} ~ {RangeEnd.Value.ToString("short")}";
                    if (IsTimePickerEnabled)
                    {
                        displayText += $" {RangeStartTime.Hours:D2}:{RangeStartTime.Minutes:D2}:{RangeStartTime.Seconds:D2}" +
                            $" ~ {RangeEndTime.Hours:D2}:{RangeEndTime.Minutes:D2}:{RangeEndTime.Seconds:D2}";
                    }
                }
                else if (RangeStart.HasValue)
                {
                    displayText = $"{RangeStart.Value.ToString("short")} ~ ...";
                    if (IsTimePickerEnabled)
                        displayText += $" {RangeStartTime.Hours:D2}:{RangeStartTime.Minutes:D2}:{RangeStartTime.Seconds:D2}";
                }
                else
                {
                    displayText = EffectiveWatermark();
                }
            }
            else if (SelectedDate.HasValue)
            {
                // FormatPersianDate handles both Persian and Gregorian modes and uses
                // the localization's day/month names. In Gregorian mode it builds a
                // DateTime directly from the struct's values (avoiding PersianCalendar,
                // which would reject e.g. July 31st as Persian month 7 only has 30 days).
                displayText = PersianCalendarHelper.FormatPersianDate(
                    SelectedDate.Value, DisplayFormat, GetLocalization(), GetEffectiveCalendarType());
                if (IsTimePickerEnabled)
                {
                    var st = SelectedTime;
                    displayText += $" {st.Hours:D2}:{st.Minutes:D2}:{st.Seconds:D2}";
                }
            }
            else
            {
                displayText = EffectiveWatermark();
            }

            // Update TextBlock
            if (_displayTextBlock != null)
            {
                _displayTextBlock.Text = displayText;
                bool hasValue = IsRangeMode ? RangeStart.HasValue : SelectedDate.HasValue;
                if (hasValue)
                    _displayTextBlock.Classes.Remove("watermark");
                else
                    _displayTextBlock.Classes.Add("watermark");
            }

            // Update TextBox
            if (_editableTextBox != null)
            {
                if (IsRangeMode)
                {
                    if (RangeStart.HasValue && RangeEnd.HasValue)
                    {
                        _editableTextBox.Text = $"{RangeStart.Value.ToString("short")} ~ {RangeEnd.Value.ToString("short")}";
                        if (IsTimePickerEnabled)
                        {
                            _editableTextBox.Text += $" {RangeStartTime.Hours:D2}:{RangeStartTime.Minutes:D2}:{RangeStartTime.Seconds:D2}" +
                                $" ~ {RangeEndTime.Hours:D2}:{RangeEndTime.Minutes:D2}:{RangeEndTime.Seconds:D2}";
                        }
                    }
                    else
                        _editableTextBox.Text = "";
                }
                else if (SelectedDate.HasValue)
                {
                    var dateStr = SelectedDate.Value.ToString("short");
                    if (IsTimePickerEnabled)
                    {
                        var st = SelectedTime;
                        _editableTextBox.Text = $"{dateStr} {st.Hours:D2}:{st.Minutes:D2}:{st.Seconds:D2}";
                    }
                    else
                    {
                        _editableTextBox.Text = dateStr;
                    }
                }
                else
                {
                    _editableTextBox.Text = "";
                }
            }
        }
        finally
        {
            _isUpdatingText = false;
        }
    }

    // Public Methods

    /// <summary>
    /// Formats the current <see cref="SelectedDate"/> using the active calendar's
    /// day/month names and the picker's localization. Returns an empty string when
    /// no date is selected.
    /// </summary>
    /// <param name="format">"long", "short", or "month".</param>
    public string FormatSelectedDate(string format = "long")
    {
        if (!SelectedDate.HasValue) return string.Empty;
        return PersianCalendarHelper.FormatPersianDate(
            SelectedDate.Value, format, GetLocalization(), GetEffectiveCalendarType());
    }

    /// <summary>
    /// Formats the current <see cref="RangeStart"/> using the active calendar's
    /// day/month names and the picker's localization.
    /// </summary>
    public string FormatRangeStart(string format = "short")
    {
        if (!RangeStart.HasValue) return string.Empty;
        return PersianCalendarHelper.FormatPersianDate(
            RangeStart.Value, format, GetLocalization(), GetEffectiveCalendarType());
    }

    /// <summary>
    /// Formats the current <see cref="RangeEnd"/> using the active calendar's
    /// day/month names and the picker's localization.
    /// </summary>
    public string FormatRangeEnd(string format = "short")
    {
        if (!RangeEnd.HasValue) return string.Empty;
        return PersianCalendarHelper.FormatPersianDate(
            RangeEnd.Value, format, GetLocalization(), GetEffectiveCalendarType());
    }

    /// <summary>
    /// Empties the field: no date, no typed text, no invalid flag. When the date was
    /// already empty (text that is not a date typed into an empty field), setting it
    /// to null changed nothing, so the text and <see cref="HasInvalidInput"/> used to
    /// stay - a "Clear filters" button left the field reporting an error.
    /// </summary>
    public void Clear()
    {
        if (_isDisposed)
            return;

        SelectedDate = null;
        if (_editableTextBox != null)
            _editableTextBox.Text = string.Empty;
        HasInvalidInput = false;
        UpdateDisplayText();
    }

    public void SetToToday()
    {
        if (_isDisposed)
            return;

        SelectedDate = PersianCalendarHelper.TodayForCalendarType(CalendarType);
    }

    /// <summary>
    /// Forces the internal calendar view to re-apply all current settings (localization, calendar type, etc.).
    /// Useful when this control is embedded inside another popup and needs to refresh after becoming visible.
    /// </summary>
    public void ForceRefreshSettings()
    {
        if (_isDisposed)
            return;

        if (_calendarView != null)
        {
            _calendarView.LocalizationProvider = GetLocalization();
            _calendarView.CalendarType = CalendarType;
            _calendarView.WeeklyHolidays = WeeklyHolidays;
            _calendarView.Holidays = Holidays;
            _calendarView.HolidayBrush = HolidayBrush;
        }

        UpdateDisplayText();
    }
}
