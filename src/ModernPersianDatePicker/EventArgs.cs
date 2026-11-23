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
/// Event arguments for selected time-of-day changed
/// </summary>
public class SelectedTimeChangedEventArgs : EventArgs
{
    public TimeSpan OldTime { get; }
    public TimeSpan NewTime { get; }

    public SelectedTimeChangedEventArgs(TimeSpan oldTime, TimeSpan newTime)
    {
        OldTime = oldTime;
        NewTime = newTime;
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

/// <summary>
/// Event arguments for date range selection
/// </summary>
public class DateRangeSelectedEventArgs : EventArgs
{
    /// <summary>
    /// Default start-of-day time (00:00:00) used when no explicit start time is provided.
    /// </summary>
    public static readonly TimeSpan DefaultStartTime = TimeSpan.Zero;

    /// <summary>
    /// Default end-of-day time (23:59:59) used when no explicit end time is provided.
    /// </summary>
    public static readonly TimeSpan DefaultEndTime = new TimeSpan(23, 59, 59);

    public PersianDate? RangeStart { get; }
    public PersianDate? RangeEnd { get; }

    /// <summary>
    /// Time-of-day component associated with <see cref="RangeStart"/>.
    /// Defaults to <see cref="DefaultStartTime"/> (00:00:00) when not explicitly supplied.
    /// </summary>
    public TimeSpan StartTime { get; }

    /// <summary>
    /// Time-of-day component associated with <see cref="RangeEnd"/>.
    /// Defaults to <see cref="DefaultEndTime"/> (23:59:59) when not explicitly supplied.
    /// </summary>
    public TimeSpan EndTime { get; }

    public DateRangeSelectedEventArgs(PersianDate? rangeStart, PersianDate? rangeEnd)
        : this(rangeStart, rangeEnd, DefaultStartTime, DefaultEndTime)
    {
    }

    public DateRangeSelectedEventArgs(PersianDate? rangeStart, PersianDate? rangeEnd,
        TimeSpan startTime, TimeSpan endTime)
    {
        RangeStart = rangeStart;
        RangeEnd = rangeEnd;
        StartTime = startTime;
        EndTime = endTime;
    }
}

/// <summary>
/// Event arguments for quick range selection
/// </summary>
public class QuickRangeSelectedEventArgs : EventArgs
{
    public PersianDate RangeStart { get; }
    public PersianDate RangeEnd { get; }
    public string RangeType { get; }

    /// <summary>
    /// Time-of-day component associated with <see cref="RangeStart"/>.
    /// For preset ranges (Today/Week/Month/...) this defaults to 00:00:00.
    /// For custom ranges it reflects the user-selected start time from the From picker.
    /// </summary>
    public TimeSpan StartTime { get; }

    /// <summary>
    /// Time-of-day component associated with <see cref="RangeEnd"/>.
    /// For preset ranges (Today/Week/Month/...) this defaults to 23:59:59.
    /// For custom ranges it reflects the user-selected end time from the To picker.
    /// </summary>
    public TimeSpan EndTime { get; }

    public QuickRangeSelectedEventArgs(PersianDate rangeStart, PersianDate rangeEnd, string rangeType)
        : this(rangeStart, rangeEnd, rangeType,
            DateRangeSelectedEventArgs.DefaultStartTime,
            DateRangeSelectedEventArgs.DefaultEndTime)
    {
    }

    public QuickRangeSelectedEventArgs(PersianDate rangeStart, PersianDate rangeEnd, string rangeType,
        TimeSpan startTime, TimeSpan endTime)
    {
        RangeStart = rangeStart;
        RangeEnd = rangeEnd;
        RangeType = rangeType;
        StartTime = startTime;
        EndTime = endTime;
    }
}
