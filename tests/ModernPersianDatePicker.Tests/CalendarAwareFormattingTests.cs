using System;
using ModernPersianDatePicker.Localizations;
using Xunit;

namespace ModernPersianDatePicker.Tests;

public class CalendarAwareFormattingTests
{
    // ──────────────────────────────────────────────────────
    // FormatPersianDate: Persian mode uses localization.MonthNames / DayNames
    // ──────────────────────────────────────────────────────

    [Fact]
    public void FormatPersianDate_PersianMode_FarsiLong_ReturnsFarsiDayAndMonth()
    {
        // PersianDate(1404, 1, 1, 0): Saturday (Shanbeh=0), 1 Farvardin 1404
        var date = new PersianDate(1404, 1, 1, 0);
        var loc = new FarsiLocalization();
        var s = PersianCalendarHelper.FormatPersianDate(date, "long", loc, CalendarType.Persian);

        Assert.Equal("شنبه 1 فروردین 1404", s);
    }

    [Fact]
    public void FormatPersianDate_PersianMode_EnglishLong_ReturnsEnglishPersianMonthName()
    {
        // EnglishLocalization still uses Persian month names transliterated to English.
        var date = new PersianDate(1404, 1, 1, 0);
        var loc = new EnglishLocalization();
        var s = PersianCalendarHelper.FormatPersianDate(date, "long", loc, CalendarType.Persian);

        Assert.Equal("Shanbeh 1 Farvardin 1404", s);
    }

    [Fact]
    public void FormatPersianDate_PersianMode_ShortFormat_ReturnsNumeric()
    {
        var date = new PersianDate(1404, 7, 15, 2);
        var s = PersianCalendarHelper.FormatPersianDate(date, "short", new FarsiLocalization(), CalendarType.Persian);

        Assert.Equal("1404/07/15", s);
    }

    [Fact]
    public void FormatPersianDate_PersianMode_MonthFormat_ReturnsMonthNameAndYear()
    {
        var date = new PersianDate(1404, 7, 15, 2);
        var s = PersianCalendarHelper.FormatPersianDate(date, "month", new FarsiLocalization(), CalendarType.Persian);

        Assert.Equal("مهر 1404", s);
    }

    // ──────────────────────────────────────────────────────
    // FormatPersianDate: Gregorian mode uses GetGregorianDayName / GetGregorianMonthName
    // ──────────────────────────────────────────────────────

    [Fact]
    public void FormatPersianDate_GregorianMode_FarsiLong_ReturnsFarsiGregorianNames()
    {
        // 2025/07/31 is a Thursday.
        // Farsi localization's Gregorian day name for Thursday (.NET DayOfWeek=4) is "پنجشنبه".
        // Farsi localization's Gregorian month name for July (7) is "ژوئیه".
        var date = new PersianDate(2025, 7, 31, 0); // DayOfWeek ignored in Gregorian path
        var loc = new FarsiLocalization();
        var s = PersianCalendarHelper.FormatPersianDate(date, "long", loc, CalendarType.Gregorian);

        Assert.Equal("پنجشنبه 31 ژوئیه 2025", s);
    }

    [Fact]
    public void FormatPersianDate_GregorianMode_EnglishLong_ReturnsEnglishGregorianNames()
    {
        // 2025/07/31 is a Thursday.
        var date = new PersianDate(2025, 7, 31, 0);
        var loc = new EnglishLocalization();
        var s = PersianCalendarHelper.FormatPersianDate(date, "long", loc, CalendarType.Gregorian);

        Assert.Equal("Thursday 31 July 2025", s);
    }

    [Fact]
    public void FormatPersianDate_GregorianMode_ShortFormat_ReturnsNumeric()
    {
        var date = new PersianDate(2025, 7, 31, 0);
        var s = PersianCalendarHelper.FormatPersianDate(date, "short", new FarsiLocalization(), CalendarType.Gregorian);

        Assert.Equal("2025/07/31", s);
    }

    [Fact]
    public void FormatPersianDate_GregorianMode_MonthFormat_ReturnsGregorianMonthName()
    {
        var date = new PersianDate(2025, 7, 31, 0);
        var s = PersianCalendarHelper.FormatPersianDate(date, "month", new FarsiLocalization(), CalendarType.Gregorian);

        Assert.Equal("ژوئیه 2025", s);
    }

    // ──────────────────────────────────────────────────────
    // FormatPersianDate: Gregorian mode accepts July 31st (the original crash case)
    // ──────────────────────────────────────────────────────

    [Fact]
    public void FormatPersianDate_GregorianMode_AcceptsJuly31()
    {
        var date = new PersianDate(2025, 7, 31, 0);
        var ex = Record.Exception(() =>
            PersianCalendarHelper.FormatPersianDate(date, "long", new FarsiLocalization(), CalendarType.Gregorian));
        Assert.Null(ex);
    }

    // ──────────────────────────────────────────────────────
    // PersianDate.ToString(format, localization, calendarType)
    // ──────────────────────────────────────────────────────

    [Fact]
    public void PersianDate_ToString_WithCalendarType_Persian_ReturnsPersianNames()
    {
        var date = new PersianDate(1404, 1, 1, 0);
        var loc = new FarsiLocalization();
        Assert.Equal("شنبه 1 فروردین 1404", date.ToString("long", loc, CalendarType.Persian));
    }

    [Fact]
    public void PersianDate_ToString_WithCalendarType_Gregorian_ReturnsGregorianNames()
    {
        var date = new PersianDate(2025, 7, 31, 0);
        var loc = new EnglishLocalization();
        Assert.Equal("Thursday 31 July 2025", date.ToString("long", loc, CalendarType.Gregorian));
    }

    [Fact]
    public void PersianDate_ToString_BackwardCompat_WithoutCalendarType_StillPersian()
    {
        // The 2-arg overload must continue to behave Persian-style for backward compatibility.
        var date = new PersianDate(1404, 1, 1, 0);
        var loc = new FarsiLocalization();
        Assert.Equal("شنبه 1 فروردین 1404", date.ToString("long", loc));
    }

    // ──────────────────────────────────────────────────────
    // CalendarType conversion (the original "year stays 2026" bug)
    // ──────────────────────────────────────────────────────

    [Fact]
    public void PersianCalendarHelper_ToPersianDate_ConvertsGregorian2026ToPersian1405()
    {
        // 2026-07-31 Gregorian = 1405-05-09 Persian (Aban — wait, let me think)
        // 1405 Persian year starts on 2026-03-21 Gregorian.
        // 1405/01/01 = 2026/03/21
        // 1405/05/01 = ?
        //   Months 1-3 (Farvardin, Ordibehesht, Khordad): 31 days each = 93 days
        //   Month 4 (Tir): 31 days = 124 days total
        //   Month 5 (Mordad): start of Mordad 1405 = 2026/03/21 + 124 days
        //   2026/03/21 + 124 days = 2026/07/23
        //   So 1405/05/09 = 2026/07/23 + 8 days = 2026/07/31. ✓
        var gregorian = new DateTime(2026, 7, 31);
        var persian = PersianCalendarHelper.ToPersianDate(gregorian);

        Assert.Equal(1405, persian.Year);
        Assert.Equal(5, persian.Month); // Mordad, not Aban
        Assert.Equal(9, persian.Day);
    }

    [Fact]
    public void PersianCalendarHelper_ToGregorianDate_ConvertsPersian1405ToGregorian2026()
    {
        // Round-trip check
        var persian = new PersianDate(1405, 5, 9, 0);
        var gregorian = PersianCalendarHelper.ToGregorianDate(persian);

        Assert.Equal(2026, gregorian.Year);
        Assert.Equal(7, gregorian.Month);
        Assert.Equal(31, gregorian.Day);
    }
}
