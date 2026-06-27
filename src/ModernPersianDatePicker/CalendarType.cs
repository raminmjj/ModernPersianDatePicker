namespace ModernPersianDatePicker;

/// <summary>
/// Determines which calendar system to display.
/// </summary>
public enum CalendarType
{
    /// <summary>
    /// Auto-detect from the current thread's culture.
    /// Persian cultures (fa-IR, fa, ps, etc.) show the Persian calendar;
    /// all others show the Gregorian calendar.
    /// </summary>
    Auto,

    Persian,
    Gregorian
}
