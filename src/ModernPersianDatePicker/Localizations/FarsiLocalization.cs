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

    public string QuickRangeLabel { get; } = "انتخاب سریع";

    public string ApplyLabel { get; } = "اعمال";

    public string CurrentWeekLabel { get; } = "این هفته";

    public string CurrentMonthLabel { get; } = "این ماه";

    public string CurrentYearLabel { get; } = "امسال";

    public string MonthLabel { get; } = "ماه";

    public string SeasonLabel { get; } = "فصل";

    public string HalfYearLabel { get; } = "نیم‌سال";

    public string BackLabel { get; } = "بازگشت";

    public string FromLabel { get; } = "از...";

    public string ToLabel { get; } = "تا...";

    public string SelectDateLabel { get; } = "یک تاریخ انتخاب کنید...";

    public string GoToTodayLabel { get; } = "برو به امروز";
}
