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

    public static readonly StyledProperty<PersianDate?> MinDateProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, PersianDate?>(nameof(MinDate));

    public static readonly StyledProperty<PersianDate?> MaxDateProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, PersianDate?>(nameof(MaxDate));

    public static readonly StyledProperty<bool> ShowTodayButtonProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, bool>(nameof(ShowTodayButton), true);

    public static readonly StyledProperty<string> WatermarkProperty =
        AvaloniaProperty.Register<ModernPersianDatePicker, string>(nameof(Watermark), "Select date...");

    // Events
    public event EventHandler<SelectedDateChangedEventArgs>? SelectedDateChanged;

    // Private fields
    private Popup? _popup;
    private Button? _toggleButton;
    private TextBlock? _displayTextBlock;
    private CalendarView? _calendarView;
    private bool _isPopupOpen;

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

    public bool ShowTodayButton
    {
        get => GetValue(ShowTodayButtonProperty);
        set => SetValue(ShowTodayButtonProperty, value);
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

        // Detach previous event handlers
        if (_toggleButton != null)
            _toggleButton.Click -= OnToggleButton_Click;

        if (_calendarView != null)
            _calendarView.DateSelected -= OnCalendarView_DateSelected;

        // Get template parts
        _popup = e.NameScope.Find<Popup>("PART_Popup");
        _toggleButton = e.NameScope.Find<Button>("PART_ToggleButton");
        _displayTextBlock = e.NameScope.Find<TextBlock>("PART_DisplayText");
        _calendarView = e.NameScope.Find<CalendarView>("PART_CalendarView");

        // Attach new event handlers
        if (_toggleButton != null)
            _toggleButton.Click += OnToggleButton_Click;

        if (_calendarView != null)
        {
            _calendarView.UseEnglishNames = UseEnglishNames;
            _calendarView.DateSelected += OnCalendarView_DateSelected;
        }

        UpdateDisplayText();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        
        // Close popup when clicking outside
        if (_isPopupOpen && _popup != null)
        {
            var point = e.GetCurrentPoint(_popup);
            if (!point.Properties.IsLeftButtonPressed)
                ClosePopup();
        }
    }

    // Private Methods
    private void OnSelectedDateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        UpdateDisplayText();
        
        var oldValue = e.OldValue as PersianDate?;
        var newValue = e.NewValue as PersianDate?;

        SelectedDateChanged?.Invoke(this, new SelectedDateChangedEventArgs(oldValue, newValue));
    }

    private void OnLanguageChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_calendarView != null)
            _calendarView.UseEnglishNames = UseEnglishNames;

        UpdateDisplayText();
    }

    private void OnToggleButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_isPopupOpen)
            ClosePopup();
        else
            OpenPopup();
    }

    private void OnCalendarView_DateSelected(object? sender, DateSelectedEventArgs e)
    {
        SelectedDate = e.SelectedDate;
        ClosePopup();
    }

    private void OpenPopup()
    {
        if (_popup != null && _calendarView != null)
        {
            _calendarView.DisplayYear = SelectedDate?.Year ?? PersianCalendarHelper.Today().Year;
            _calendarView.DisplayMonth = SelectedDate?.Month ?? PersianCalendarHelper.Today().Month;
            _calendarView.SelectedDate = SelectedDate;
            
            _popup.IsOpen = true;
            _isPopupOpen = true;
        }
    }

    private void ClosePopup()
    {
        if (_popup != null)
        {
            _popup.IsOpen = false;
            _isPopupOpen = false;
        }
    }

    private void UpdateDisplayText()
    {
        if (_displayTextBlock != null)
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
    }

    // Public Methods
    public void Clear()
    {
        SelectedDate = null;
    }

    public void SetToToday()
    {
        SelectedDate = PersianCalendarHelper.Today();
    }
}
