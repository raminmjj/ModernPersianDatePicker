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

    public string QuickRangeLabel { get; } = "اختيار سريع";

    public string ApplyLabel { get; } = "تطبيق";

    public string CurrentWeekLabel { get; } = "هذا الأسبوع";

    public string CurrentMonthLabel { get; } = "هذا الشهر";

    public string CurrentYearLabel { get; } = "هذا العام";

    public string MonthLabel { get; } = "شهر";

    public string SeasonLabel { get; } = "فصول";

    public string HalfYearLabel { get; } = "نصف سنة";

    public string BackLabel { get; } = "رجوع";

    public string FromLabel { get; } = "من...";

    public string ToLabel { get; } = "إلى...";

    public string SelectDateLabel { get; } = "اختر تاريخاً...";

    public string GoToTodayLabel { get; } = "انتقل إلى اليوم";

    public string[] SeasonNames { get; } = { "ربيع", "صيف", "خريف", "شتاء" };
}
