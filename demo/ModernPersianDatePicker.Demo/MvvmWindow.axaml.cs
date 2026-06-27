using Avalonia.Controls;
using Avalonia.Interactivity;
using ModernPersianDatePicker.Demo.ViewModels;

namespace ModernPersianDatePicker.Demo;

public partial class MvvmWindow : Window
{
    private readonly MainWindowViewModel _vm;

    public MvvmWindow()
    {
        InitializeComponent();
        _vm = (MainWindowViewModel)DataContext!;

        BasicDatePicker.SelectedDateChanged += (_, e) => _vm.OnBasicDateChanged(e.NewDate);
        EnglishDatePicker.SelectedDateChanged += (_, e) => _vm.OnEnglishDateChanged(e.NewDate);
        CustomDatePicker.SelectedDateChanged += (_, e) => _vm.OnCustomDateChanged(e.NewDate);
        GreenAccentDatePicker.SelectedDateChanged += (_, e) => _vm.OnAccentDateChanged(e.NewDate);
        PurpleAccentDatePicker.SelectedDateChanged += (_, e) => _vm.OnAccentDateChanged(e.NewDate);
        EditableDatePicker.SelectedDateChanged += (_, e) => _vm.OnEditableDateChanged(e.NewDate);
        ThemedDatePicker.SelectedDateChanged += (_, e) => _vm.OnThemedDateChanged(e.NewDate);
        WeeklyHolidayDatePicker.SelectedDateChanged += (_, e) => _vm.OnWeeklyHolidayDateChanged(e.NewDate);
        SpecificDatePicker.SelectedDateChanged += (_, e) => _vm.OnSpecificHolidayDateChanged(e.NewDate);
        CustomHolidayBrushDatePicker.SelectedDateChanged += (_, e) => _vm.OnCustomHolidayBrushDateChanged(e.NewDate);
        TimePickerDatePicker.SelectedDateChanged += (_, e) => _vm.OnTimePickerDateChanged(e.NewDate);
        RangeDatePicker.DateRangeSelected += (_, e) => _vm.OnRangeDateSelected(e.RangeStart, e.RangeEnd);
        LocalizedDatePicker.SelectedDateChanged += (_, e) => _vm.OnLocalizedDateChanged(e.NewDate);
        GregorianDatePicker.SelectedDateChanged += (_, e) => _vm.OnGregorianDateChanged(e.NewDate);

        RbSetToNull.Click += OnInvalidActionChanged;
        RbSetToToday.Click += OnInvalidActionChanged;
        RbKeep.Click += OnInvalidActionChanged;
    }

    private void OnInvalidActionChanged(object? sender, RoutedEventArgs e)
    {
        if (RbSetToNull.IsChecked == true)
            _vm.InvalidAction = "SetToNull";
        else if (RbSetToToday.IsChecked == true)
            _vm.InvalidAction = "SetToToday";
        else if (RbKeep.IsChecked == true)
            _vm.InvalidAction = "Keep";
    }
}
