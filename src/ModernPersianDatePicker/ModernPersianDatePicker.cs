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

    // Events
    public event EventHandler<SelectedDateChangedEventArgs>? SelectedDateChanged;

    // Private fields
    private Popup? _popup;
    private ToggleButton? _toggleButton;
    private TextBlock? _displayTextBlock;
    private TextBox? _editableTextBox;
    private CalendarView? _calendarView;
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

    // Protected Methods
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // Detach previous event handlers safely
        if (_toggleButton != null)
            _toggleButton.Click -= OnToggleButton_Click;

        if (_calendarView != null)
        {
            _calendarView.DateSelected -= OnCalendarView_DateSelected;
            _calendarView.TodayClicked -= OnCalendarView_TodayClicked;
        }

        // Get template parts
        _popup = e.NameScope.Find<Popup>("PART_Popup");
        _toggleButton = e.NameScope.Find<ToggleButton>("PART_ToggleButton");
        _displayTextBlock = e.NameScope.Find<TextBlock>("PART_DisplayText");
        _editableTextBox = e.NameScope.Find<TextBox>("PART_EditableText");
        _calendarView = e.NameScope.Find<CalendarView>("PART_CalendarView");

        // Attach event handlers
        if (_toggleButton != null)
        {
            _toggleButton.Click += OnToggleButton_Click;
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
            _calendarView.DateSelected += OnCalendarView_DateSelected;
            _calendarView.TodayClicked += OnCalendarView_TodayClicked;
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
            // Normalize separators
            var normalized = input.Replace('-', '/').Replace('_', '/').Trim();
            var parts = normalized.Split('/');

            if (parts.Length != 3)
                return null;

            // Parse year
            if (!int.TryParse(parts[0].Trim(), out int year))
                return null;

            // Handle 2-digit years
            if (year >= 0 && year < 100)
            {
                // Assume 14xx for 2-digit years
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
            if (SelectedDate.HasValue)
            {
                displayText = SelectedDate.Value.ToString(DisplayFormat);
            }
            else
            {
                displayText = Watermark;
            }

            // Update TextBlock
            if (_displayTextBlock != null)
            {
                _displayTextBlock.Text = displayText;
                if (SelectedDate.HasValue)
                    _displayTextBlock.Classes.Remove("watermark");
                else
                    _displayTextBlock.Classes.Add("watermark");
            }

            // Update TextBox
            if (_editableTextBox != null)
            {
                if (SelectedDate.HasValue)
                {
                    _editableTextBox.Text = SelectedDate.Value.ToString("short");
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
