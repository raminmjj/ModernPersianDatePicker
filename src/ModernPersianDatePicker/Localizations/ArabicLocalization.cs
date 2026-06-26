namespace ModernPersianDatePicker.Localizations;

/// <summary>
/// Arabic localization for the calendar.
/// </summary>
public class ArabicLocalization : ICalendarLocalization
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
        "السبت", "الأحد", "الاثنين",
        "الثلاثاء", "الأربعاء", "الخميس",
        "الجمعة"
    };

    public string[] ShortDayNames { get; } =
    {
        "س", "أ", "ث", "ر", "خ", "ج", "ج"
    };

    public string TodayLabel { get; } = "اليوم";
}
