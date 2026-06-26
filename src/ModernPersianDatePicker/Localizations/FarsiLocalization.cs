namespace ModernPersianDatePicker.Localizations;

/// <summary>
/// Farsi (Persian) localization for the calendar.
/// </summary>
public class FarsiLocalization : ICalendarLocalization
{
    public string[] MonthNames { get; } =
    {
        "", "فروردین", "اردیبهشت", "خرداد",
        "تیر", "مرداد", "شهریور",
        "مهر", "آبان", "آذر",
        "دی", "بهمن", "اسفند"
    };

    public string[] DayNames { get; } =
    {
        "شنبه", "یکشنبه", "دوشنبه",
        "سه‌شنبه", "چهارشنبه", "پنجشنبه",
        "جمعه"
    };

    public string[] ShortDayNames { get; } =
    {
        "ش", "ی", "د", "س", "چ", "پ", "ج"
    };

    public string TodayLabel { get; } = "امروز";
}
