using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

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

    // Constructors
    static ModernPersianDatePicker()
    {
        SelectedDateProperty.Changed.AddClassHandler<ModernPersianDatePicker>((x, e) => x.OnSelectedDateChanged(e));
        UseEnglishNamesProperty.Changed.AddClassHandler<ModernPersianDatePicker>((x, e) => x.OnLanguageChanged(e));
    }

    public ModernPersianDatePicker()
    {
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
            _calendarView.DateSelected += OnCalendarView_DateSelected;
            _calendarView.TodayClicked += OnCalendarView_TodayClicked;
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
