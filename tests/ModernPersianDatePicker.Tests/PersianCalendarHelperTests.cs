using System;
using Xunit;

namespace ModernPersianDatePicker.Tests;

public class PersianCalendarHelperTests
{
    // ──────────────────────────────────────────────────────
    // 1. GetMonthName
    // ──────────────────────────────────────────────────────

    [Theory]
    [InlineData(1, false, "فروردین")]
    [InlineData(7, false, "مهر")]
    [InlineData(12, false, "اسفند")]
    public void GetMonthName_Farsi_ReturnsCorrectName(int month, bool english, string expected)
    {
        Assert.Equal(expected, PersianCalendarHelper.GetMonthName(month, english));
    }

    [Theory]
    [InlineData(1, true, "Farvardin")]
    [InlineData(7, true, "Mehr")]
    [InlineData(12, true, "Esfand")]
    public void GetMonthName_English_ReturnsCorrectName(int month, bool english, string expected)
    {
        Assert.Equal(expected, PersianCalendarHelper.GetMonthName(month, english));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void GetMonthName_InvalidMonth_Throws(int month)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PersianCalendarHelper.GetMonthName(month));
    }

    // ──────────────────────────────────────────────────────
    // 2. GetDayName
    // ──────────────────────────────────────────────────────

    [Theory]
    [InlineData(0, false, false, "شنبه")]
    [InlineData(3, false, false, "سه‌شنبه")]
    [InlineData(6, false, false, "جمعه")]
    public void GetDayName_Farsi_ReturnsCorrectName(int dayIndex, bool english, bool shortFormat, string expected)
    {
        Assert.Equal(expected, PersianCalendarHelper.GetDayName(dayIndex, english, shortFormat));
    }

    [Theory]
    [InlineData(0, true, "Shanbeh")]
    [InlineData(3, true, "Seshanbeh")]
    [InlineData(6, true, "Jomeh")]
    public void GetDayName_English_ReturnsCorrectName(int dayIndex, bool english, string expected)
    {
        Assert.Equal(expected, PersianCalendarHelper.GetDayName(dayIndex, english));
    }

    [Theory]
    [InlineData(0, "ش")]
    [InlineData(6, "ج")]
    public void GetDayName_ShortFormat_ReturnsCorrectName(int dayIndex, string expected)
    {
        Assert.Equal(expected, PersianCalendarHelper.GetDayName(dayIndex, shortFormat: true));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(7)]
    public void GetDayName_InvalidIndex_Throws(int dayIndex)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PersianCalendarHelper.GetDayName(dayIndex));
    }

    // ──────────────────────────────────────────────────────
    // 3. GetDaysInMonth
    // ──────────────────────────────────────────────────────

    [Theory]
    [InlineData(1403, 1)]
    [InlineData(1403, 2)]
    [InlineData(1403, 3)]
    [InlineData(1403, 4)]
    [InlineData(1403, 5)]
    [InlineData(1403, 6)]
    public void GetDaysInMonth_First6Months_All31(int year, int month)
    {
        Assert.Equal(31, PersianCalendarHelper.GetDaysInMonth(year, month));
    }

    [Theory]
    [InlineData(1403, 7)]
    [InlineData(1403, 8)]
    [InlineData(1403, 9)]
    [InlineData(1403, 10)]
    [InlineData(1403, 11)]
    public void GetDaysInMonth_Months7to11_All30(int year, int month)
    {
        Assert.Equal(30, PersianCalendarHelper.GetDaysInMonth(year, month));
    }

    [Fact]
    public void GetDaysInMonth_Esfand_NormalYear_Returns29()
    {
        // 1404 is confirmed non-leap
        Assert.Equal(29, PersianCalendarHelper.GetDaysInMonth(1404, 12));
    }

    [Fact]
    public void GetDaysInMonth_Esfand_LeapYear_Returns30()
    {
        // 1403 and 1408 are confirmed leap years
        Assert.Equal(30, PersianCalendarHelper.GetDaysInMonth(1403, 12));
        Assert.Equal(30, PersianCalendarHelper.GetDaysInMonth(1408, 12));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void GetDaysInMonth_InvalidMonth_Throws(int month)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PersianCalendarHelper.GetDaysInMonth(1403, month));
    }

    // ──────────────────────────────────────────────────────
    // 4. IsLeapYear
    // ──────────────────────────────────────────────────────

    [Theory]
    [InlineData(1403)]
    [InlineData(1408)]
    public void IsLeapYear_KnownLeapYears_ReturnsTrue(int year)
    {
        Assert.True(PersianCalendarHelper.IsLeapYear(year));
    }

    [Theory]
    [InlineData(1402)]
    [InlineData(1404)]
    [InlineData(1405)]
    public void IsLeapYear_KnownNonLeapYears_ReturnsFalse(int year)
    {
        Assert.False(PersianCalendarHelper.IsLeapYear(year));
    }

    // ──────────────────────────────────────────────────────
    // 5. GetFirstDayOfWeek
    // ──────────────────────────────────────────────────────

    [Fact]
    public void GetFirstDayOfWeek_AlwaysReturns0to6()
    {
        for (int y = 1400; y <= 1410; y++)
        {
            for (int m = 1; m <= 12; m++)
            {
                int dow = PersianCalendarHelper.GetFirstDayOfWeek(y, m);
                Assert.InRange(dow, 0, 6);
            }
        }
    }

    [Fact]
    public void GetFirstDayOfWeek_KnownValues_AreCorrect()
    {
        // Verified against .NET PersianCalendar
        Assert.Equal(4, PersianCalendarHelper.GetFirstDayOfWeek(1403, 1));  // 1403/01/01 -> persian dow 4 (پنجشنبه)
        Assert.Equal(0, PersianCalendarHelper.GetFirstDayOfWeek(1403, 2));  // 1403/02/01 -> persian dow 0 (شنبه)
        Assert.Equal(6, PersianCalendarHelper.GetFirstDayOfWeek(1403, 4));  // 1403/04/01 -> persian dow 6 (جمعه)
    }

    // ──────────────────────────────────────────────────────
    // 6. ToPersianDate / ToGregorianDate
    // ──────────────────────────────────────────────────────

    [Fact]
    public void ToPersianDate_KnownGregorian_ReturnsCorrectPersian()
    {
        // Gregorian 2025-01-01 -> Persian 1403/10/12, dotnet dow=3 -> persian dow=(3+1)%7=4
        var result = PersianCalendarHelper.ToPersianDate(new DateTime(2025, 1, 1));
        Assert.Equal(1403, result.Year);
        Assert.Equal(10, result.Month);
        Assert.Equal(12, result.Day);
    }

    [Fact]
    public void ToGregorianDate_KnownPersian_ReturnsCorrectGregorian()
    {
        // Persian 1403/10/12 -> Gregorian 2025-01-01
        var persianDate = new PersianDate(1403, 10, 12, 4);
        var gregorian = PersianCalendarHelper.ToGregorianDate(persianDate);
        Assert.Equal(new DateTime(2025, 1, 1), gregorian);
    }

    [Fact]
    public void ToPersianDate_RoundTrip_PreservesDate()
    {
        var original = DateTime.Today;
        var persian = PersianCalendarHelper.ToPersianDate(original);
        var roundTrip = PersianCalendarHelper.ToGregorianDate(persian);

        Assert.Equal(original, roundTrip);
    }

    [Fact]
    public void ToPersianDate_RoundTrip_PreservesDate_ForMultipleDates()
    {
        var dates = new[]
        {
            new DateTime(2024, 3, 20),  // 1403/01/01
            new DateTime(2024, 6, 21),  // 1403/04/01
            new DateTime(2025, 1, 1),   // 1403/10/12
            new DateTime(2025, 2, 19),  // 1403/12/01
        };

        foreach (var dt in dates)
        {
            var persian = PersianCalendarHelper.ToPersianDate(dt);
            var roundTrip = PersianCalendarHelper.ToGregorianDate(persian);
            Assert.Equal(dt, roundTrip);
        }
    }

    // ──────────────────────────────────────────────────────
    // 7. Today
    // ──────────────────────────────────────────────────────

    [Fact]
    public void Today_ReturnsReasonableDate()
    {
        var today = PersianCalendarHelper.Today();
        Assert.InRange(today.Year, 1400, 1420);
        Assert.InRange(today.Month, 1, 12);
        Assert.InRange(today.Day, 1, 31);
        Assert.InRange(today.DayOfWeek, 0, 6);
    }
}
