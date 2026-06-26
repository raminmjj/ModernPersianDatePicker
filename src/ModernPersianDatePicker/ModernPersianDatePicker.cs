using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;

namespace ModernPersianDatePicker;

/// <summary>
/// Modern Persian Date Picker control for Avalonia UI
/// Provides a calendar dropdown for selecting Persian (Jalali/Shamsi) dates
/// </summary>
public class ModernPersianDatePicker : TemplatedControl
{
    // Styled Properties
    public static readonly StyledProperty<PersianDate?> SelectedDateProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, PersianDate?>(nameof(SelectedDate));

    public static readonly StyledProperty<string> DisplayFormatProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, string>(nameof(DisplayFormat), "long");

    public static readonly StyledProperty<bool> UseEnglishNamesProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, bool>(nameof(UseEnglishNames));

    public static readonly StyledProperty<bool> IsEditableProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, bool>(nameof(IsEditable));

    public static readonly StyledProperty<InvalidValueAction> InvalidValueActionProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, InvalidValueAction>(nameof(InvalidValueAction), InvalidValueAction.SetToNull);

    public static readonly StyledProperty<PersianDate?> MinDateProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, PersianDate?>(nameof(MinDate));

    public static readonly StyledProperty<PersianDate?> MaxDateProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, PersianDate?>(nameof(MaxDate));

    public static readonly StyledProperty<string> WatermarkProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, string>(nameof(Watermark), "Select date...");

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
    /// Selected hour (0-23). Only meaningful when <see cref="IsTimePickerEnabled"/> is true.
    /// </summary>
    public static readonly StyledProperty<int> HourProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, int>(nameof(Hour),
            validate: v => v >= 0 && v <= 23);

    /// <summary>
    /// Selected minute (0-59). Only meaningful when <see cref="IsTimePickerEnabled"/> is true.
    /// </summary>
    public static readonly StyledProperty<int> MinuteProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, int>(nameof(Minute),
            validate: v => v >= 0 && v <= 59);

    /// <summary>
    /// Selected second (0-59). Only meaningful when <see cref="IsTimePickerEnabled"/> is true.
    /// </summary>
    public static readonly StyledProperty<int> SecondProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, int>(nameof(Second),
            validate: v => v >= 0 && v <= 59);

    // Events
    public event EventHandler<SelectedDateChangedEventArgs>? SelectedDateChanged;
    public event EventHandler<DateRangeSelectedEventArgs>? DateRangeSelected;

    // Private fields
    private Popup? _popup;
    private ToggleButton? _toggleButton;
    private TextBlock? _displayTextBlock;
    private TextBox? _editableTextBox;
    private CalendarView? _calendarView;
    private TimePickerView? _timePickerView;
    private bool _isPopupOpen;
    private bool _isDisposed;
    private bool _isUpdatingText;
    private bool _hasThemeOverrides;

    // Constructors
    static ModernPersianDatePicker()
    {
        SelectedDateProperty.Changed.AddClassHandler<ModernPersianDatePicker>((x, e) => x.OnSelectedDateChanged(e));
        UseEnglishNamesProperty.Changed.AddClassHandler<ModernPersianDatePicker>((x, e) => x.OnLanguageChanged(e));
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
        HourProperty.Changed.AddClassHandler<ModernPersianDatePicker>((x, e) => x.OnTimeComponentChanged(e));
        MinuteProperty.Changed.AddClassHandler<ModernPersianDatePicker>((x, e) => x.OnTimeComponentChanged(e));
        SecondProperty.Changed.AddClassHandler<ModernPersianDatePicker>((x, e) => x.OnTimeComponentChanged(e));
    }

    public ModernPersianDatePicker()
    {
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

    public string DisplayFormat
    {
        get => GetValue(DisplayFormatProperty);
        set => SetValue(DisplayFormatProperty, value);
    }

    public bool UseEnglishNames
    {
        get => GetValue(UseEnglishNamesProperty);
        set => SetValue(UseEnglishNamesProperty, value);
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
    /// Selected hour (0-23). Only meaningful when <see cref="IsTimePickerEnabled"/> is true.
    /// </summary>
    public int Hour
    {
        get => GetValue(HourProperty);
        set => SetValue(HourProperty, value);
    }

    /// <summary>
    /// Selected minute (0-59). Only meaningful when <see cref="IsTimePickerEnabled"/> is true.
    /// </summary>
    public int Minute
    {
        get => GetValue(MinuteProperty);
        set => SetValue(MinuteProperty, value);
    }

    /// <summary>
    /// Selected second (0-59). Only meaningful when <see cref="IsTimePickerEnabled"/> is true.
    /// </summary>
    public int Second
    {
        get => GetValue(SecondProperty);
        set => SetValue(SecondProperty, value);
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

        if (_calendarView != null)
        {
            _calendarView.DateSelected -= OnCalendarView_DateSelected;
            _calendarView.DateRangeSelected -= OnCalendarView_DateRangeSelected;
            _calendarView.TodayClicked -= OnCalendarView_TodayClicked;
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
            _calendarView.UseEnglishNames = UseEnglishNames;
            _calendarView.HolidayBrush = HolidayBrush;
            _calendarView.WeeklyHolidays = WeeklyHolidays;
            _calendarView.Holidays = Holidays;
            _calendarView.IsRangeMode = IsRangeMode;
            _calendarView.RangeStart = RangeStart;
            _calendarView.RangeEnd = RangeEnd;
            _calendarView.DateSelected += OnCalendarView_DateSelected;
            _calendarView.DateRangeSelected += OnCalendarView_DateRangeSelected;
            _calendarView.TodayClicked += OnCalendarView_TodayClicked;
        }

        if (_timePickerView != null)
        {
            _timePickerView.Hour = Hour;
            _timePickerView.Minute = Minute;
            _timePickerView.Second = Second;
            _timePickerView.TimeChanged += OnTimePickerView_TimeChanged;
            _timePickerView.IsVisible = IsTimePickerEnabled;
        }

        UpdateEditMode();
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

    private void OnEditableText_LostFocus(object? sender, RoutedEventArgs e)
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
            if (InvalidValueAction == InvalidValueAction.SetToToday)
            {
                SelectedDate = PersianCalendarHelper.Today();
            }
            else if (InvalidValueAction == InvalidValueAction.SetToNull)
            {
                SelectedDate = null;
            }
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
                SelectedDate = PersianCalendarHelper.Today();
                break;
            case InvalidValueAction.SetToNull:
                SelectedDate = null;
                break;
            case InvalidValueAction.Keep:
                // Keep the current selected date (don't change)
                break;
        }
    }

    private PersianDate? ParsePersianDate(string input)
    {
        try
        {
            // Split date and time parts (time is optional)
            var dateTimeParts = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
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
                year = 1400 + year;
            }

            // Parse month
            if (!int.TryParse(parts[1].Trim(), out int month) || month < 1 || month > 12)
                return null;

            // Parse day
            if (!int.TryParse(parts[2].Trim(), out int day) || day < 1 || day > 31)
                return null;

            // Validate day for the specific month
            int daysInMonth = PersianCalendarHelper.GetDaysInMonth(year, month);
            if (day > daysInMonth)
                return null;

            // Parse time component if present
            if (IsTimePickerEnabled && dateTimeParts.Length > 1)
            {
                var timeParts = dateTimeParts[1].Split(':');
                if (timeParts.Length >= 2)
                {
                    if (int.TryParse(timeParts[0], out int h) && h >= 0 && h <= 23)
                        Hour = h;
                    if (int.TryParse(timeParts[1], out int m) && m >= 0 && m <= 59)
                        Minute = m;
                }
                if (timeParts.Length >= 3)
                {
                    if (int.TryParse(timeParts[2], out int s) && s >= 0 && s <= 59)
                        Second = s;
                }
            }

            // Calculate day of week
            var firstDayOfWeek = PersianCalendarHelper.GetFirstDayOfWeek(year, month);
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

    // Private Methods
    private void OnSelectedDateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_isDisposed)
            return;

        UpdateDisplayText();

        var oldValue = e.OldValue as PersianDate?;
        var newValue = e.NewValue as PersianDate?;

        SelectedDateChanged?.Invoke(this, new SelectedDateChangedEventArgs(oldValue, newValue));
    }

    private void OnLanguageChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_isDisposed)
            return;

        if (_calendarView != null)
            _calendarView.UseEnglishNames = UseEnglishNames;

        UpdateDisplayText();
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

    /// <summary>
    /// Resolves <see cref="Theme"/> into a concrete Light/Dark flag and applies colour
    /// resource overrides on this control so that <c>DynamicResource</c> lookups
    /// (inside the template) pick the right palette.
    ///
    /// For <see cref="ThemeMode.System"/> no overrides are applied — the template's
    /// <c>DynamicResource</c> bindings resolve automatically via the active
    /// <c>ThemeDictionaries</c> in PersianDatePickerColors.xaml.
    /// </summary>
    private void ApplyTheme()
    {
        if (Theme == ThemeMode.System)
        {
            // Inherit the app/system theme via DynamicResource — no overrides needed.
            ClearThemeOverrides();
        }
        else
        {
            bool isDark = Theme == ThemeMode.Dark;
            ApplyThemeColors(isDark);
        }
    }

    /// <summary>
    /// Overrides the control-level resources so that all
    /// <c>DynamicResource</c> bindings inside the template resolve to the correct
    /// Light or Dark palette.  When <paramref name="isDark"/> is false, overrides are
    /// removed so the default (Light) theme colours from <c>PersianDatePickerColors.xaml</c> apply.
    /// </summary>
    private void ApplyThemeColors(bool isDark)
    {
        if (isDark)
        {
            Resources["PersianDatePickerBackgroundBrush"] = new SolidColorBrush(Color.Parse("#FF2D2D2D"));
            Resources["PersianDatePickerBorderBrush"] = new SolidColorBrush(Color.Parse("#FF555555"));
            Resources["PersianDatePickerTextForegroundBrush"] = new SolidColorBrush(Color.Parse("#FFCCCCCC"));
            Resources["PersianDatePickerWatermarkForegroundBrush"] = new SolidColorBrush(Color.Parse("#FF888888"));
            Resources["PersianDatePickerAccentBrush"] = new SolidColorBrush(Color.Parse("#FF64B5F6"));
            Resources["PersianDatePickerAccentHoverBrush"] = new SolidColorBrush(Color.Parse("#FF2196F3"));
            Resources["PersianDatePickerFocusBorderBrush"] = new SolidColorBrush(Color.Parse("#FF64B5F6"));
            Resources["PersianDatePickerPopupBackgroundBrush"] = new SolidColorBrush(Color.Parse("#FF2D2D2D"));
            Resources["PersianDatePickerPopupBorderBrush"] = new SolidColorBrush(Color.Parse("#FF555555"));
            Resources["PersianDatePickerHeaderForegroundBrush"] = new SolidColorBrush(Color.Parse("#FFAAAAAA"));
            Resources["PersianDatePickerDayForegroundBrush"] = new SolidColorBrush(Color.Parse("#FFDDDDDD"));
            Resources["PersianDatePickerHolidayForegroundBrush"] = new SolidColorBrush(Color.Parse("#FFEF5350"));
            Resources["PersianDatePickerDayHoverBackgroundBrush"] = new SolidColorBrush(Color.Parse("#FF404040"));
            Resources["PersianDatePickerSelectedForegroundBrush"] = new SolidColorBrush(Color.Parse("#FFFFFFFF"));
            Resources["PersianDatePickerMonthYearHoverBackgroundBrush"] = new SolidColorBrush(Color.Parse("#FF404040"));
            Resources["PersianDatePickerDayFocusBackgroundBrush"] = new SolidColorBrush(Color.Parse("#FF404040"));
            Resources["PersianDatePickerSelectedFocusBorderBrush"] = new SolidColorBrush(Color.Parse("#FFFFFFFF"));
            Resources["PersianDatePickerSelectedFocusBackgroundBrush"] = new SolidColorBrush(Color.Parse("#FF2196F3"));
            _hasThemeOverrides = true;
        }
        else
        {
            ClearThemeOverrides();
        }

        // Reapply accent brush overrides (if set) so they aren't shadowed by the theme swap.
        if (_hasThemeOverrides)
            UpdateAccentResources(AccentBrush);
    }

    private void ClearThemeOverrides()
    {
        if (!_hasThemeOverrides)
            return;

        var keys = new[]
        {
            "PersianDatePickerBackgroundBrush",
            "PersianDatePickerBorderBrush",
            "PersianDatePickerTextForegroundBrush",
            "PersianDatePickerWatermarkForegroundBrush",
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
        foreach (var key in keys)
            Resources.Remove(key);

        _hasThemeOverrides = false;
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
            _calendarView.IsRangeMode = IsRangeMode;

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

        if (_timePickerView != null)
            _timePickerView.IsVisible = IsTimePickerEnabled;

        UpdateDisplayText();
    }

    private void OnTimeComponentChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_isDisposed)
            return;

        if (_timePickerView != null)
        {
            if (e.Property == HourProperty)
                _timePickerView.Hour = Hour;
            else if (e.Property == MinuteProperty)
                _timePickerView.Minute = Minute;
            else if (e.Property == SecondProperty)
                _timePickerView.Second = Second;
        }

        UpdateDisplayText();
    }

    private void OnTimePickerView_TimeChanged(object? sender, EventArgs e)
    {
        if (_isDisposed || _timePickerView == null)
            return;

        _isUpdatingText = true;
        Hour = _timePickerView.Hour;
        Minute = _timePickerView.Minute;
        Second = _timePickerView.Second;
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

    private void OnPopup_Closed(object? sender, EventArgs e)
    {
        if (_isDisposed)
            return;

        // Sync internal state when the popup is dismissed (e.g. by light dismiss).
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
        DateRangeSelected?.Invoke(this, e);
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

    private void OpenPopup()
    {
        if (_isDisposed || _popup == null || _calendarView == null)
            return;

        try
        {
            _calendarView.DisplayYear = SelectedDate?.Year ?? PersianCalendarHelper.Today().Year;
            _calendarView.DisplayMonth = SelectedDate?.Month ?? PersianCalendarHelper.Today().Month;
            _calendarView.SelectedDate = SelectedDate;

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
            _popup.IsOpen = false;
            _isPopupOpen = false;

            if (_toggleButton != null)
                _toggleButton.IsChecked = false;
        }
        catch (Exception)
        {
            // Silently handle any popup errors
            _isPopupOpen = false;
            if (_toggleButton != null)
                _toggleButton.IsChecked = false;
        }
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
                    displayText = $"{RangeStart.Value.ToString("short")} ~ {RangeEnd.Value.ToString("short")}";
                else if (RangeStart.HasValue)
                    displayText = $"{RangeStart.Value.ToString("short")} ~ ...";
                else
                    displayText = Watermark;
            }
            else if (SelectedDate.HasValue)
            {
                displayText = SelectedDate.Value.ToString(DisplayFormat);
                if (IsTimePickerEnabled)
                    displayText += $" {Hour:D2}:{Minute:D2}:{Second:D2}";
            }
            else
            {
                displayText = Watermark;
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
                        _editableTextBox.Text = $"{RangeStart.Value.ToString("short")} ~ {RangeEnd.Value.ToString("short")}";
                    else
                        _editableTextBox.Text = "";
                }
                else if (SelectedDate.HasValue)
                {
                    var dateStr = SelectedDate.Value.ToString("short");
                    _editableTextBox.Text = IsTimePickerEnabled
                        ? $"{dateStr} {Hour:D2}:{Minute:D2}:{Second:D2}"
                        : dateStr;
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
    public void Clear()
    {
        if (_isDisposed)
            return;

        SelectedDate = null;
    }

    public void SetToToday()
    {
        if (_isDisposed)
            return;

        SelectedDate = PersianCalendarHelper.Today();
    }
}
