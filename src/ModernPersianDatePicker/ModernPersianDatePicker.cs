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
    private CalendarView? _calendarView;
    private bool _isPopupOpen;
    private bool _isDisposed;

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
        }

        // Get template parts
        _popup = e.NameScope.Find<Popup>("PART_Popup");
        _toggleButton = e.NameScope.Find<ToggleButton>("PART_ToggleButton");
        _displayTextBlock = e.NameScope.Find<TextBlock>("PART_DisplayText");
        _calendarView = e.NameScope.Find<CalendarView>("PART_CalendarView");

        // Attach new event handlers
        if (_toggleButton != null)
        {
            _toggleButton.Click += OnToggleButton_Click;
        }

        // Add preview click handler to display text
        if (_displayTextBlock != null)
        {
            _displayTextBlock.PointerPressed += OnDisplayText_PointerPressed;
        }

        if (_calendarView != null)
        {
            _calendarView.UseEnglishNames = UseEnglishNames;
            _calendarView.DateSelected += OnCalendarView_DateSelected;
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
                else if (IsEditable && _displayTextBlock != null)
                {
                    // Try to parse manual input
                    TryParseManualInput(_displayTextBlock.Text);
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

    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);

        // Close popup when losing focus (unless focus moved to popup)
        if (_isPopupOpen && IsEditable && _displayTextBlock != null)
        {
            TryParseManualInput(_displayTextBlock.Text);
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
        if (_isDisposed || _displayTextBlock == null)
            return;

        try
        {
            if (SelectedDate.HasValue)
            {
                _displayTextBlock.Text = SelectedDate.Value.ToString(DisplayFormat);
                _displayTextBlock.Classes.Remove("watermark");
            }
            else
            {
                _displayTextBlock.Text = Watermark;
                _displayTextBlock.Classes.Add("watermark");
            }
        }
        catch (Exception)
        {
            // Handle any formatting errors gracefully
            _displayTextBlock.Text = Watermark;
        }
    }

    private void TryParseManualInput(string? input)
    {
        if (string.IsNullOrWhiteSpace(input) || input == Watermark)
            return;

        try
        {
            // Try to parse Persian date format: YYYY/MM/DD or YYYY-MM-DD
            var normalizedInput = input.Replace('-', '/').Trim();
            var parts = normalizedInput.Split('/');

            if (parts.Length == 3 &&
                int.TryParse(parts[0], out int year) &&
                int.TryParse(parts[1], out int month) &&
                int.TryParse(parts[2], out int day))
            {
                var parsedDate = new PersianDate(year, month, day, 0);
                
                // Validate min/max date
                if (MinDate.HasValue && parsedDate < MinDate.Value)
                    return;
                if (MaxDate.HasValue && parsedDate > MaxDate.Value)
                    return;

                SelectedDate = parsedDate;
            }
        }
        catch (Exception)
        {
            // Invalid input, ignore
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
