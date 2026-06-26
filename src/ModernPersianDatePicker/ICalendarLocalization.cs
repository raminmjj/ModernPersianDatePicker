namespace ModernPersianDatePicker;

/// <summary>
/// Provides localized strings for calendar month names, day names, and UI labels.
/// Implement this interface to add support for custom languages.
/// </summary>
public interface ICalendarLocalization
{
    /// <summary>
    /// Month names indexed 1-12 (index 0 is unused).
    /// </summary>
    string[] MonthNames { get; }

    /// <summary>
    /// Full day names indexed 0-6 (Saturday=0 through Friday=6).
    /// </summary>
    string[] DayNames { get; }

    /// <summary>
    /// Short day names indexed 0-6 (Saturday=0 through Friday=6).
    /// </summary>
    string[] ShortDayNames { get; }

    /// <summary>
    /// Label for the "Today" button.
    /// </summary>
    string TodayLabel { get; }
}
