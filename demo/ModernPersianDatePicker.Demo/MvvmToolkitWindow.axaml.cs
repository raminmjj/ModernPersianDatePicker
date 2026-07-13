using Avalonia.Controls;
using Avalonia.Interactivity;
using ModernPersianDatePicker.Demo.ViewModels;

namespace ModernPersianDatePicker.Demo;

public partial class MvvmToolkitWindow : Window
{
    private readonly MvvmToolkitWindowViewModel _vm;

    public MvvmToolkitWindow()
    {
        InitializeComponent();
        _vm = (MvvmToolkitWindowViewModel)DataContext!;

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
        TimePickerDatePicker.SelectedTimeChanged += (_, _) => _vm.OnTimePickerDateChanged(TimePickerDatePicker.SelectedDate);
        RangeDatePicker.DateRangeSelected += (_, e) => _vm.OnRangeDateSelected(e.RangeStart, e.RangeEnd, e.StartTime, e.EndTime);
        RangeDatePicker.RangeTimeChanged += (_, _) => _vm.OnRangeDateSelected(
            RangeDatePicker.RangeStart, RangeDatePicker.RangeEnd,
            RangeDatePicker.RangeStartTime, RangeDatePicker.RangeEndTime);

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
