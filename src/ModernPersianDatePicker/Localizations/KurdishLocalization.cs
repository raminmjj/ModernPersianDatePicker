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
}
