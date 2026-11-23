using System;
using System.Globalization;
using ModernPersianDatePicker.Localizations;

namespace ModernPersianDatePicker;

/// <summary>
/// Provides Persian (Jalali/Shamsi) calendar conversion utilities
/// Based on the algorithm used in FarsiLibrary and CodeProject
/// </summary>
public static class PersianCalendarHelper
{
    // Persian month names in Farsi
    private static readonly string[] PersianMonthNames =
    {
        "فروردین", "اردیبهشت", "خرداد",
        "تیر", "مرداد", "شهریور",
        "مهر", "آبان", "آذر",
        "دی", "بهمن", "اسفند"
    };

    // Persian month names in English
    private static readonly string[] PersianMonthNamesEn =
    {
        "Farvardin", "Ordibehesht", "Khordad",
        "Tir", "Mordad", "Shahrivar",
        "Mehr", "Aban", "Azar",
        "Dey", "Bahman", "Esfand"
    };

    // Persian day names in Farsi
    private static readonly string[] PersianDayNames =
    {
        "شنبه", "یکشنبه", "دوشنبه",
        "سه‌شنبه", "چهارشنبه", "پنجشنبه",
        "جمعه"
    };

    // Persian day names in English
    private static readonly string[] PersianDayNamesEn =
    {
        "Shanbeh", "Yekshanbeh", "Doshanbeh",
        "Seshanbeh", "Chaharshanbeh", "Panjshanbeh",
        "Jomeh"
    };

    // Short day names
    private static readonly string[] PersianShortDayNames =
    {
        "ش", "ی", "د", "س", "چ", "پ", "ج"
    };

    /// <summary>
    /// Converts a Gregorian date to Persian (Jalali) date
    /// </summary>
    public static PersianDate ToPersianDate(DateTime gregorianDate)
    {
        var persianCalendar = new PersianCalendar();
        int year = persianCalendar.GetYear(gregorianDate);
        int month = persianCalendar.GetMonth(gregorianDate);
        int day = persianCalendar.GetDayOfMonth(gregorianDate);
        DayOfWeek dotNetDayOfWeek = persianCalendar.GetDayOfWeek(gregorianDate);
        
        // Convert from .NET DayOfWeek to Persian day index
        // .NET: Sunday=0, Monday=1, Tuesday=2, Wednesday=3, Thursday=4, Friday=5, Saturday=6
        // Persian: Shanbeh(Sat)=0, Yekshanbeh(Sun)=1, Doshanbeh(Mon)=2,
        //          Seshanbeh(Tue)=3, Chaharshanbeh(Wed)=4, Panjshanbeh(Thu)=5, Jomeh(Fri)=6
        int persianDayOfWeek = ((int)dotNetDayOfWeek + 1) % 7;

        return new PersianDate(year, month, day, persianDayOfWeek);
    }

    /// <summary>
    /// Converts a Persian date to Gregorian date.
    /// Use this when the <see cref="PersianDate"/> struct holds an actual Persian (Jalali)
    /// date that needs to be converted to its Gregorian equivalent.
    /// </summary>
    public static DateTime ToGregorianDate(PersianDate persianDate)
    {
        var persianCalendar = new PersianCalendar();
        return persianCalendar.ToDateTime(persianDate.Year, persianDate.Month, persianDate.Day, 0, 0, 0, 0);
    }

    /// <summary>
    /// Constructs a <see cref="DateTime"/> directly from a <see cref="PersianDate"/> struct
    /// whose Year/Month/Day values are already in the Gregorian calendar.
    /// Use this when the picker is in Gregorian mode and the struct holds a Gregorian date
    /// (e.g. July 31st). Calling <see cref="ToGregorianDate"/> in that case would fail because
    /// <c>PersianCalendar</c> rejects day=31 for month=7 (Mehr only has 30 days).
    /// </summary>
    public static DateTime ToGregorianDateTime(PersianDate persianDate)
    {
        return new DateTime(persianDate.Year, persianDate.Month, persianDate.Day);
    }

    /// <summary>
    /// Formats a <see cref="PersianDate"/> using the appropriate day/month names for the
    /// active calendar. Use this instead of <see cref="PersianDate.ToString(string)"/> when
    /// you need the formatted string to respect both the localization and the calendar type
    /// (e.g. an English-localized picker in Gregorian mode should produce "Monday 14 July 2025",
    /// not Persian "Doshanbeh 14 Tir 1404").
    /// </summary>
    /// <param name="date">The date struct (holds either Persian or Gregorian values).</param>
    /// <param name="format">"long", "short", or "month".</param>
    /// <param name="localization">Localization provider (Farsi/English/Arabic/Kurdish).</param>
    /// <param name="calendarType">Effective calendar type (Persian or Gregorian).</param>
    public static string FormatPersianDate(PersianDate date, string format,
        ICalendarLocalization localization, CalendarType calendarType)
    {
        if (calendarType == CalendarType.Gregorian)
        {
            // Build a real DateTime from the struct's Gregorian values so we can
            // use .NET's DayOfWeek (Sunday=0..Saturday=6) for name lookup.
            var d = ToGregorianDateTime(date);
            int dotNetDow = (int)d.DayOfWeek;
            return format.ToLower() switch
            {
                "long"  => $"{GetGregorianDayName(dotNetDow, localization)} {d.Day} {GetGregorianMonthName(d.Month, localization)} {d.Year}",
                "month" => $"{GetGregorianMonthName(d.Month, localization)} {d.Year}",
                _       => $"{d.Year}/{d.Month:D2}/{d.Day:D2}"
            };
        }
        else
        {
            // Persian: struct's DayOfWeek is Persian-week-indexed (Sat=0..Fri=6).
            return format.ToLower() switch
            {
                "long"  => $"{localization.DayNames[date.DayOfWeek]} {date.Day} {localization.MonthNames[date.Month]} {date.Year}",
                "month" => $"{localization.MonthNames[date.Month]} {date.Year}",
                _       => $"{date.Year}/{date.Month:D2}/{date.Day:D2}"
            };
        }
    }

    /// <summary>
    /// Gets the Persian month name
    /// </summary>
    public static string GetMonthName(int month, bool useEnglish = false)
    {
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12");

        return useEnglish ? PersianMonthNamesEn[month - 1] : PersianMonthNames[month - 1];
    }

    /// <summary>
    /// Gets the Persian day name
    /// </summary>
    public static string GetDayName(int dayIndex, bool useEnglish = false, bool shortFormat = false)
    {
        if (dayIndex < 0 || dayIndex > 6)
            throw new ArgumentOutOfRangeException(nameof(dayIndex), "Day index must be between 0 and 6");

        if (shortFormat)
            return PersianShortDayNames[dayIndex];

        return useEnglish ? PersianDayNamesEn[dayIndex] : PersianDayNames[dayIndex];
    }

    /// <summary>
    /// Gets the number of days in a Persian month
    /// </summary>
    public static int GetDaysInMonth(int year, int month)
    {
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12");

        if (month <= 6)
            return 31;

        if (month <= 11)
            return 30;

        // Esfand - check for leap year
        return IsLeapYear(year) ? 30 : 29;
    }

    /// <summary>
    /// Determines if a Persian year is a leap year
    /// </summary>
    public static bool IsLeapYear(int year)
    {
        PersianCalendar pc = new PersianCalendar();
        return pc.IsLeapYear(year);
    }

    /// <summary>
    /// Gets the first day of the week for a Persian month
    /// Returns 0 for Shanbeh (Saturday), 1 for Yekshanbeh, etc.
    /// </summary>
    public static int GetFirstDayOfWeek(int year, int month)
    {
        var firstDay = ToGregorianDate(new PersianDate(year, month, 1, 0));
        var persianCalendar = new PersianCalendar();
        DayOfWeek dotNetDay = persianCalendar.GetDayOfWeek(firstDay);
        
        // Convert from .NET DayOfWeek to Persian day index
        // .NET: Sunday=0, Monday=1, Tuesday=2, Wednesday=3, Thursday=4, Friday=5, Saturday=6
        // Persian: Shanbeh(Sat)=0, Yekshanbeh(Sun)=1, Doshanbeh(Mon)=2, 
        //          Seshanbeh(Tue)=3, Chaharshanbeh(Wed)=4, Panjshanbeh(Thu)=5, Jomeh(Fri)=6
        int persianDayIndex = ((int)dotNetDay + 1) % 7;
        
        return persianDayIndex;
    }

    /// <summary>
    /// Gets today's date in Persian calendar
    /// </summary>
    public static PersianDate Today()
    {
        return ToPersianDate(DateTime.Today);
    }

    /// <summary>
    /// Resolves a (possibly Auto) CalendarType to a concrete Persian or Gregorian.
    /// Auto is resolved using the current thread's UI culture: fa/ps → Persian,
    /// everything else → Gregorian. This mirrors the picker's own resolution logic.
    /// </summary>
    public static CalendarType ResolveEffectiveCalendarType(CalendarType calendarType)
    {
        if (calendarType != CalendarType.Auto)
            return calendarType;

        var culture = System.Globalization.CultureInfo.CurrentUICulture;
        return culture.TwoLetterISOLanguageName is "fa" or "ps"
            ? CalendarType.Persian
            : CalendarType.Gregorian;
    }

    /// <summary>
    /// Returns today's date as a PersianDate struct whose Year/Month/Day values
    /// are in the specified calendar system. For Auto, resolves via
    /// <see cref="ResolveEffectiveCalendarType"/>.
    /// </summary>
    public static PersianDate TodayForCalendarType(CalendarType calendarType)
    {
        var effective = ResolveEffectiveCalendarType(calendarType);
        if (effective == CalendarType.Gregorian)
        {
            var today = DateTime.Today;
            return new PersianDate(today.Year, today.Month, today.Day, (int)today.DayOfWeek);
        }
        return Today();
    }

    /// <summary>
    /// Returns two mid-week days (Tuesday and Wednesday) from the current month
    /// in the specified calendar system. For Auto, resolves via
    /// <see cref="ResolveEffectiveCalendarType"/>. The returned PersianDate structs
    /// hold values in the resolved calendar and are suitable for use as holiday
    /// examples that match the picker's active calendar.
    /// </summary>
    public static PersianDate[] GetMidWeekDaysOfCurrentMonth(CalendarType calendarType)
    {
        var effective = ResolveEffectiveCalendarType(calendarType);

        if (effective == CalendarType.Gregorian)
        {
            var today = DateTime.Today;
            return FindMidWeekDays(today.Year, today.Month, effective);
        }
        else
        {
            var today = Today();
            return FindMidWeekDays(today.Year, today.Month, effective);
        }
    }

    /// <summary>
    /// Finds the first Tuesday and Wednesday in the given month/year.
    /// In the Persian week (Sat=0..Fri=6), Tuesday=3 and Wednesday=4.
    /// In the .NET week (Sun=0..Sat=6), Tuesday=2 and Wednesday=3.
    /// </summary>
    private static PersianDate[] FindMidWeekDays(int year, int month, CalendarType calendarType)
    {
        var result = new System.Collections.Generic.List<PersianDate>();

        if (calendarType == CalendarType.Gregorian)
        {
            int daysInMonth = DateTime.DaysInMonth(year, month);
            for (int day = 1; day <= daysInMonth && result.Count < 2; day++)
            {
                var dt = new DateTime(year, month, day);
                if (dt.DayOfWeek == DayOfWeek.Tuesday || dt.DayOfWeek == DayOfWeek.Wednesday)
                    result.Add(new PersianDate(year, month, day, (int)dt.DayOfWeek));
            }
        }
        else
        {
            int firstDayOfWeek = GetFirstDayOfWeek(year, month);
            int daysInMonth = GetDaysInMonth(year, month);
            for (int day = 1; day <= daysInMonth && result.Count < 2; day++)
            {
                int persianDow = (firstDayOfWeek + day - 1) % 7;
                // Persian week: 0=Sat, 1=Sun, 2=Mon, 3=Tue, 4=Wed, 5=Thu, 6=Fri
                if (persianDow == 3 || persianDow == 4)
                    result.Add(new PersianDate(year, month, day, persianDow));
            }
        }

        return result.ToArray();
    }

    // ── Gregorian helpers ──

    public static int GetGregorianDaysInMonth(int year, int month)
    {
        return DateTime.DaysInMonth(year, month);
    }

    public static int GetGregorianFirstDayOfWeek(int year, int month)
    {
        var firstDay = new DateTime(year, month, 1);
        return (int)firstDay.DayOfWeek;
    }

    public static bool IsGregorianLeapYear(int year)
    {
        return DateTime.IsLeapYear(year);
    }

    public static string GetGregorianMonthName(int month, ICalendarLocalization localization)
    {
        if (localization is Localizations.FarsiLocalization)
        {
            return month switch
            {
                1 => "ژانویه", 2 => "فوریه", 3 => "مارس",
                4 => "آوریل", 5 => "مه", 6 => "ژوئن",
                7 => "ژوئیه", 8 => "اوت", 9 => "سپتامبر",
                10 => "اکتبر", 11 => "نوامبر", 12 => "دسامبر",
                _ => ""
            };
        }
        if (localization is Localizations.KurdishLocalization)
        {
            return month switch
            {
                1 => "کانوونی یەکەم", 2 => "کانوونی دووەم", 3 => "ئازار",
                4 => "نیسان", 5 => "ئایار", 6 => "حوزەیران",
                7 => "تەمموز", 8 => "ئاب", 9 => "ئەیلوول",
                10 => "تشرینی یەکەم", 11 => "تشرینی دووەم", 12 => "کانوونی یەکەم",
                _ => ""
            };
        }
        return month switch
        {
            1 => "January", 2 => "February", 3 => "March",
            4 => "April", 5 => "May", 6 => "June",
            7 => "July", 8 => "August", 9 => "September",
            10 => "October", 11 => "November", 12 => "December",
            _ => ""
        };
    }

    public static string GetGregorianDayName(int dayIndex, ICalendarLocalization localization)
    {
        if (localization is Localizations.FarsiLocalization)
        {
            return dayIndex switch
            {
                0 => "یکشنبه", 1 => "دوشنبه", 2 => "سه‌شنبه",
                3 => "چهارشنبه", 4 => "پنجشنبه", 5 => "جمعه",
                6 => "شنبه",
                _ => ""
            };
        }
        if (localization is Localizations.KurdishLocalization)
        {
            return dayIndex switch
            {
                0 => "یەکشەممە", 1 => "دووشەممە", 2 => "سێشەممە",
                3 => "چوارشەممە", 4 => "پێنجشەممە", 5 => "هەینی",
                6 => "شەممە",
                _ => ""
            };
        }
        return dayIndex switch
        {
            0 => "Sunday", 1 => "Monday", 2 => "Tuesday",
            3 => "Wednesday", 4 => "Thursday", 5 => "Friday",
            6 => "Saturday",
            _ => ""
        };
    }

    public static string GetGregorianShortDayName(int dayIndex, ICalendarLocalization localization)
    {
        if (localization is Localizations.FarsiLocalization)
        {
            return dayIndex switch
            {
                0 => "ی", 1 => "د", 2 => "س",
                3 => "چ", 4 => "پ", 5 => "ج",
                6 => "ش",
                _ => ""
            };
        }
        if (localization is Localizations.KurdishLocalization)
        {
            return dayIndex switch
            {
                0 => "ی", 1 => "د", 2 => "س",
                3 => "چ", 4 => "پ", 5 => "ه",
                6 => "ش",
                _ => ""
            };
        }
        return dayIndex switch
        {
            0 => "Sun", 1 => "Mon", 2 => "Tue",
            3 => "Wed", 4 => "Thu", 5 => "Fri",
            6 => "Sat",
            _ => ""
        };
    }
}

/// <summary>
/// Represents a Persian (Jalali/Shamsi) date
/// </summary>
public readonly struct PersianDate : IEquatable<PersianDate>, IComparable<PersianDate>
{
    public int Year { get; }
    public int Month { get; }
    public int Day { get; }
    public int DayOfWeek { get; }

    public PersianDate(int year, int month, int day, int dayOfWeek)
    {
        if (year < 1)
            throw new ArgumentOutOfRangeException(nameof(year), "Year must be greater than 0");
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12");
        if (day < 1 || day > 31)
            throw new ArgumentOutOfRangeException(nameof(day), "Day must be between 1 and 31");
        if (dayOfWeek < 0 || dayOfWeek > 6)
            throw new ArgumentOutOfRangeException(nameof(dayOfWeek), "Day of week must be between 0 and 6");

        Year = year;
        Month = month;
        Day = day;
        DayOfWeek = dayOfWeek;
    }

    public override string ToString()
    {
        return $"{Year}/{Month:D2}/{Day:D2}";
    }

    public string ToString(string format)
    {
        return format.ToLower() switch
        {
            "long" => $"{PersianCalendarHelper.GetDayName(DayOfWeek)} {Day} {PersianCalendarHelper.GetMonthName(Month)} {Year}",
            "short" => $"{Year}/{Month:D2}/{Day:D2}",
            "month" => $"{PersianCalendarHelper.GetMonthName(Month)} {Year}",
            _ => ToString()
        };
    }

    public string ToString(string format, ICalendarLocalization localization)
    {
        // Backward-compatible overload: assumes Persian calendar semantics.
        // Use ToString(format, localization, calendarType) for calendar-aware formatting.
        return format.ToLower() switch
        {
            "long" => $"{localization.DayNames[DayOfWeek]} {Day} {localization.MonthNames[Month]} {Year}",
            "short" => $"{Year}/{Month:D2}/{Day:D2}",
            "month" => $"{localization.MonthNames[Month]} {Year}",
            _ => ToString()
        };
    }

    /// <summary>
    /// Calendar-aware formatting. Produces day/month names appropriate for the active
    /// calendar type and localization. Use this in place of <see cref="ToString(string, ICalendarLocalization)"/>
    /// when the picker might be in Gregorian mode.
    /// </summary>
    public string ToString(string format, ICalendarLocalization localization, CalendarType calendarType)
    {
        return PersianCalendarHelper.FormatPersianDate(this, format, localization, calendarType);
    }

    public bool Equals(PersianDate other)
    {
        return Year == other.Year && Month == other.Month && Day == other.Day;
    }

    public override bool Equals(object? obj)
    {
        return obj is PersianDate other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Year, Month, Day);
    }

    public static bool operator ==(PersianDate left, PersianDate right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PersianDate left, PersianDate right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Converts this date to a .NET <see cref="DateTime"/> assuming the struct holds
    /// an actual Persian (Jalali) date. Use <see cref="ToDateTimeAsGregorian"/> instead when
    /// the struct holds a Gregorian date (i.e. the picker is in Gregorian mode).
    /// </summary>
    public DateTime ToDateTime()
    {
        return PersianCalendarHelper.ToGregorianDate(this);
    }

    /// <summary>
    /// Constructs a <see cref="DateTime"/> directly from this struct's Year/Month/Day,
    /// treating them as already Gregorian. Use this when the picker is in Gregorian mode.
    /// </summary>
    public DateTime ToDateTimeAsGregorian()
    {
        return PersianCalendarHelper.ToGregorianDateTime(this);
    }

    public int CompareTo(PersianDate other)
    {
        if (Year != other.Year)
            return Year.CompareTo(other.Year);
        if (Month != other.Month)
            return Month.CompareTo(other.Month);
        return Day.CompareTo(other.Day);
    }

    public static bool operator <(PersianDate left, PersianDate right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator >(PersianDate left, PersianDate right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator <=(PersianDate left, PersianDate right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >=(PersianDate left, PersianDate right)
    {
        return left.CompareTo(right) >= 0;
    }
}
