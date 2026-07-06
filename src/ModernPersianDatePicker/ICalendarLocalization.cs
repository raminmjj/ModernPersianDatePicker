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

    /// <summary>
    /// Label for the "Quick Range" button.
    /// </summary>
    string QuickRangeLabel { get; }

    /// <summary>
    /// Label for the "Apply" button.
    /// </summary>
    string ApplyLabel { get; }

    /// <summary>
    /// Label for the "Current Week" preset.
    /// </summary>
    string CurrentWeekLabel { get; }

    /// <summary>
    /// Label for the "Current Month" preset.
    /// </summary>
    string CurrentMonthLabel { get; }

    /// <summary>
    /// Label for the "Current Year" preset.
    /// </summary>
    string CurrentYearLabel { get; }

    /// <summary>
    /// Label for the "Month" group selector.
    /// </summary>
    string MonthLabel { get; }

    /// <summary>
    /// Label for the "Season" group selector.
    /// </summary>
    string SeasonLabel { get; }

    /// <summary>
    /// Label for the "Half-Year" group selector.
    /// </summary>
    string HalfYearLabel { get; }

    /// <summary>
    /// Label for the "Back" button.
    /// </summary>
    string BackLabel { get; }

    /// <summary>
    /// Label for the "From" watermark.
    /// </summary>
    string FromLabel { get; }

    /// <summary>
    /// Label for the "To" watermark.
    /// </summary>
    string ToLabel { get; }

    /// <summary>
    /// Default watermark text for the date picker.
    /// </summary>
    string SelectDateLabel { get; }

    /// <summary>
    /// Tooltip for the "Go to today" button.
    /// </summary>
    string GoToTodayLabel { get; }

    /// <summary>
    /// Season names indexed 0-3 (Spring, Summer, Autumn, Winter).
    /// </summary>
    string[] SeasonNames { get; }
}
