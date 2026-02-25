using System;

namespace ModernPersianDatePicker;

/// <summary>
/// Action to take when an invalid date is entered
/// </summary>
public enum InvalidValueAction
{
    /// <summary>
    /// Set the date to null when invalid value is entered
    /// </summary>
    SetToNull,
    
    /// <summary>
    /// Set the date to today when invalid value is entered
    /// </summary>
    SetToToday,
    
    /// <summary>
    /// Keep the invalid value (no automatic correction)
    /// </summary>
    Keep
}

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
