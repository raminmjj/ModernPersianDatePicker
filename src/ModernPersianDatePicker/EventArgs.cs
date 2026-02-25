using System;

namespace ModernPersianDatePicker;

/// <summary>
/// Event arguments for date selection
/// </summary>
public class DateSelectedEventArgs : EventArgs
{
    public PersianDate? SelectedDate { get; }

    public DateSelectedEventArgs(PersianDate? selectedDate)
    {
        SelectedDate = selectedDate;
    }
}

/// <summary>
/// Event arguments for selected date changed
/// </summary>
public class SelectedDateChangedEventArgs : EventArgs
{
    public PersianDate? OldDate { get; }
    public PersianDate? NewDate { get; }

    public SelectedDateChangedEventArgs(PersianDate? oldDate, PersianDate? newDate)
    {
        OldDate = oldDate;
        NewDate = newDate;
    }
}

/// <summary>
/// Event arguments for today button click
/// </summary>
public class TodayClickedEventArgs : EventArgs
{
    public PersianDate Today { get; }

    public TodayClickedEventArgs(PersianDate today)
    {
        Today = today;
    }
}
