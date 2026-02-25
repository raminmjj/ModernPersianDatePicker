using Avalonia.Controls;
using Avalonia.Interactivity;
using ModernPersianDatePicker;

namespace ModernPersianDatePicker.Demo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Subscribe to date changed events
        BasicDatePicker.SelectedDateChanged += OnBasicDateChanged;
        EnglishDatePicker.SelectedDateChanged += OnEnglishDateChanged;
        CustomDatePicker.SelectedDateChanged += OnCustomDateChanged;
        
        // Set default date for basic picker
        BasicDatePicker.SelectedDate = PersianCalendarHelper.Today();
    }
    
    private void OnBasicDateChanged(object? sender, SelectedDateChangedEventArgs e)
    {
        if (e.NewDate.HasValue)
        {
            BasicDateText.Text = $"Selected: {e.NewDate.Value.ToString("long")}";
        }
        else
        {
            BasicDateText.Text = "Selected: (none)";
        }
    }
    
    private void OnEnglishDateChanged(object? sender, SelectedDateChangedEventArgs e)
    {
        if (e.NewDate.HasValue)
        {
            EnglishDateText.Text = $"Selected: {e.NewDate.Value.ToString("short")}";
        }
        else
        {
            EnglishDateText.Text = "Selected: (none)";
        }
    }
    
    private void OnCustomDateChanged(object? sender, SelectedDateChangedEventArgs e)
    {
        if (e.NewDate.HasValue)
        {
            CustomDateText.Text = $"Selected: {e.NewDate.Value.ToString("long")}";
        }
        else
        {
            CustomDateText.Text = "Selected: (none)";
        }
    }
    
    private void OnSetToTodayClick(object? sender, RoutedEventArgs e)
    {
        CustomDatePicker.SetToToday();
    }
    
    private void OnClearClick(object? sender, RoutedEventArgs e)
    {
        CustomDatePicker.Clear();
    }
    
    private void OnShowGregorianClick(object? sender, RoutedEventArgs e)
    {
        if (CustomDatePicker.SelectedDate.HasValue)
        {
            var gregorianDate = CustomDatePicker.SelectedDate.Value.ToDateTime();
            GregorianDateText.Text = $"Gregorian: {gregorianDate:yyyy/MM/dd} ({gregorianDate:MMMM dd, yyyy})";
        }
        else
        {
            GregorianDateText.Text = "Gregorian: (no date selected)";
        }
    }
}
