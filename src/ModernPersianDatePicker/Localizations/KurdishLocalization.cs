namespace ModernPersianDatePicker.Localizations;

/// <summary>
/// Kurdish (Sorani) localization for the calendar.
/// </summary>
public class KurdishLocalization : ICalendarLocalization
{
    public string[] MonthNames { get; } =
    {
        "", "خاکەلێوە", "گوڵان", "جۆەڕەحان",
        "پووشپەڕ", "گەلاوێژ", "خەرمانان",
        "ڕەزبەر", "سەرماوەز", "بەفرانبار",
        "ڕێبەندان", "گەڵاڕێزان", "چیاڕێستان"
    };

    public string[] DayNames { get; } =
    {
        "شەممە", "یەکشەممە", "دووشەممە",
        "سێشەممە", "چوارشەممە", "پێنجشەممە",
        "هەینی"
    };

    public string[] ShortDayNames { get; } =
    {
        "ش", "ی", "د", "س", "چ", "پ", "ه"
    };

    public string TodayLabel { get; } = "ئەمڕۆ";

    public string QuickRangeLabel { get; } = "هەڵبژاردنی خێرا";

    public string ApplyLabel { get; } = "جێبەجێکردن";

    public string CurrentWeekLabel { get; } = "ئەم هەفتەیە";

    public string CurrentMonthLabel { get; } = "ئەم مانگەیە";

    public string CurrentYearLabel { get; } = "ئەم ساڵەیە";

    public string MonthLabel { get; } = "مانگ";

    public string SeasonLabel { get; } = "وانان";

    public string HalfYearLabel { get; } = "نیوەساڵ";

    public string BackLabel { get; } = "گەڕانەوە";

    public string FromLabel { get; } = "لە...";

    public string ToLabel { get; } = "بۆ...";

    public string SelectDateLabel { get; } = "بەروارێک هەڵبژێرە...";

    public string GoToTodayLabel { get; } = "بۆ ئەمڕۆ بچۆ";

    public string[] SeasonNames { get; } = { "بەهار", "تابستان", "پاییز", "زستان" };
}
