using System;
using System.Globalization;

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
        DayOfWeek dayOfWeek = persianCalendar.GetDayOfWeek(gregorianDate);

        return new PersianDate(year, month, day, (int)dayOfWeek);
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
        // Persian calendar leap year algorithm
        int r = year % 33;
        return r == 1 || r == 5 || r == 9 || r == 13 || r == 17 || r == 22 || r == 26 || r == 30;
    }

    /// <summary>
    /// Gets the first day of the week for a Persian month
    /// Returns 0 for Shanbeh (Saturday), 1 for Yekshanbeh, etc.
    /// </summary>
    public static int GetFirstDayOfWeek(int year, int month)
    {
        var firstDay = ToGregorianDate(new PersianDate(year, month, 1, 0));
        var persianCalendar = new PersianCalendar();
        int dayOfWeek = (int)persianCalendar.GetDayOfWeek(firstDay);
        
        // Convert from Sunday=0 format to Shanbeh=0 format
        // Persian calendar: Shanbeh=0, Yekshanbeh=1, ..., Jomeh=6
        // .NET DayOfWeek: Sunday=0, Monday=1, ..., Saturday=6
        // Persian calendar starts from Shanbeh (Saturday)
        return (dayOfWeek + 1) % 7;
    }

    /// <summary>
    /// Gets today's date in Persian calendar
    /// </summary>
    public static PersianDate Today()
    {
        return ToPersianDate(DateTime.Today);
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
