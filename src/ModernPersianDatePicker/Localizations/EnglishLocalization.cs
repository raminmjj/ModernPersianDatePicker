namespace ModernPersianDatePicker.Localizations;

/// <summary>
/// English localization for the calendar.
/// </summary>
public class EnglishLocalization : ICalendarLocalization
{
    public string[] MonthNames { get; } =
    {
        "", "Farvardin", "Ordibehesht", "Khordad",
        "Tir", "Mordad", "Shahrivar",
        "Mehr", "Aban", "Azar",
        "Dey", "Bahman", "Esfand"
    };

    public string[] DayNames { get; } =
    {
        "Shanbeh", "Yekshanbeh", "Doshanbeh",
        "Seshanbeh", "Chaharshanbeh", "Panjshanbeh",
        "Jomeh"
    };

    public string[] ShortDayNames { get; } =
    {
        "Sh", "Ye", "Do", "Se", "Ch", "Pa", "Jo"
    };

    public string TodayLabel { get; } = "Today";

    public string QuickRangeLabel { get; } = "Quick Range";

    public string ApplyLabel { get; } = "Apply";

    public string CurrentWeekLabel { get; } = "Current Week";

    public string CurrentMonthLabel { get; } = "Current Month";

    public string CurrentYearLabel { get; } = "Current Year";

    public string MonthLabel { get; } = "Month";

    public string SeasonLabel { get; } = "Season";

    public string HalfYearLabel { get; } = "Half-Year";

    public string BackLabel { get; } = "Back";
}
