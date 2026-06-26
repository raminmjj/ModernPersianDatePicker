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
}
