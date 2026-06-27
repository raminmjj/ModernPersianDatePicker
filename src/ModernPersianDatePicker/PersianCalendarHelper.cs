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
    /// Converts a Persian date to Gregorian date
    /// </summary>
    public static DateTime ToGregorianDate(PersianDate persianDate)
    {
        var persianCalendar = new PersianCalendar();
        return persianCalendar.ToDateTime(persianDate.Year, persianDate.Month, persianDate.Day, 0, 0, 0, 0);
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
        return format.ToLower() switch
        {
            "long" => $"{localization.DayNames[DayOfWeek]} {Day} {localization.MonthNames[Month]} {Year}",
            "short" => $"{Year}/{Month:D2}/{Day:D2}",
            "month" => $"{localization.MonthNames[Month]} {Year}",
            _ => ToString()
        };
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

    public DateTime ToDateTime()
    {
        return PersianCalendarHelper.ToGregorianDate(this);
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
