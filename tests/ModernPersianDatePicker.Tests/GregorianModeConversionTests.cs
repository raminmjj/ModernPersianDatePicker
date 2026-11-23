using System;
using Xunit;

namespace ModernPersianDatePicker.Tests;

public class GregorianModeConversionTests
{
    // ──────────────────────────────────────────────────────
    // ToGregorianDate (Persian -> Gregorian) — original method
    // ──────────────────────────────────────────────────────

    [Fact]
    public void ToGregorianDate_ConvertsPersianDateCorrectly()
    {
        // 1404/01/01 Persian = 2025/03/21 Gregorian (Nowruz)
        var persian = new PersianDate(1404, 1, 1, 0);
        var gregorian = PersianCalendarHelper.ToGregorianDate(persian);

        Assert.Equal(2025, gregorian.Year);
        Assert.Equal(3, gregorian.Month);
        Assert.Equal(21, gregorian.Day);
    }

    [Fact]
    public void ToGregorianDate_RejectsInvalidPersianDayForMonth()
    {
        // Persian month 7 (Mehr) has only 30 days. Day 31 should throw.
        var pd = new PersianDate(1404, 7, 31, 0);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PersianCalendarHelper.ToGregorianDate(pd));
    }

    // ──────────────────────────────────────────────────────
    // ToGregorianDateTime (identity for Gregorian values)
    // ──────────────────────────────────────────────────────

    [Fact]
    public void ToGregorianDateTime_ConstructsDirectlyFromStructValues()
    {
        // July 4, 2025 Gregorian
        var pd = new PersianDate(2025, 7, 4, 0);
        var dt = PersianCalendarHelper.ToGregorianDateTime(pd);

        Assert.Equal(2025, dt.Year);
        Assert.Equal(7, dt.Month);
        Assert.Equal(4, dt.Day);
    }

    [Fact]
    public void ToGregorianDateTime_AcceptsJuly31()
    {
        // This is the exact case that crashed before the fix:
        // July has 31 days in Gregorian, but Persian month 7 (Mehr) only has 30.
        var pd = new PersianDate(2025, 7, 31, 0);
        var dt = PersianCalendarHelper.ToGregorianDateTime(pd);

        Assert.Equal(2025, dt.Year);
        Assert.Equal(7, dt.Month);
        Assert.Equal(31, dt.Day);
    }

    [Theory]
    [InlineData(2024, 1, 1)]
    [InlineData(2025, 7, 31)]   // July 31
    [InlineData(2025, 8, 31)]   // August 31
    [InlineData(2025, 10, 31)]  // October 31
    [InlineData(2025, 12, 31)]  // December 31
    [InlineData(2024, 2, 29)]   // Feb 29 in leap year
    public void ToGregorianDateTime_AcceptsAllValidGregorianDates(int y, int m, int d)
    {
        var pd = new PersianDate(y, m, d, 0);
        var dt = PersianCalendarHelper.ToGregorianDateTime(pd);

        Assert.Equal(y, dt.Year);
        Assert.Equal(m, dt.Month);
        Assert.Equal(d, dt.Day);
    }

    [Fact]
    public void ToGregorianDateTime_DoesNotRouteThroughPersianCalendar()
    {
        // Sanity check: if the method accidentally routed through PersianCalendar,
        // year 2025 would be misinterpreted as Persian year 2025 (≈ 2646 Gregorian).
        var pd = new PersianDate(2025, 1, 1, 0);
        var dt = PersianCalendarHelper.ToGregorianDateTime(pd);

        Assert.Equal(2025, dt.Year); // NOT ~2646
    }

    // ──────────────────────────────────────────────────────
    // PersianDate.ToDateTime() vs ToDateTimeAsGregorian()
    // ──────────────────────────────────────────────────────

    [Fact]
    public void PersianDate_ToDateTime_ConvertsPersianToGregorian()
    {
        var persian = new PersianDate(1404, 1, 1, 0);
        var dt = persian.ToDateTime();

        Assert.Equal(2025, dt.Year);
        Assert.Equal(3, dt.Month);
        Assert.Equal(21, dt.Day);
    }

    [Fact]
    public void PersianDate_ToDateTimeAsGregorian_ReturnsStructValuesAsIs()
    {
        var pd = new PersianDate(2025, 7, 31, 0);
        var dt = pd.ToDateTimeAsGregorian();

        Assert.Equal(2025, dt.Year);
        Assert.Equal(7, dt.Month);
        Assert.Equal(31, dt.Day);
    }

    [Fact]
    public void PersianDate_ToDateTime_ThrowsForJuly31()
    {
        // This was the original crash. Confirm the original method
        // still throws (so callers know to use ToDateTimeAsGregorian
        // when the struct holds Gregorian values).
        var pd = new PersianDate(2025, 7, 31, 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => pd.ToDateTime());
    }

    // ──────────────────────────────────────────────────────
    // Round-trip checks
    // ──────────────────────────────────────────────────────

    [Fact]
    public void PersianDate_RoundTrip_PersianThroughToGregorianDate()
    {
        var original = new PersianDate(1404, 7, 15, 0);
        var gregorian = PersianCalendarHelper.ToGregorianDate(original);
        var roundTrip = PersianCalendarHelper.ToPersianDate(gregorian);

        Assert.Equal(original.Year, roundTrip.Year);
        Assert.Equal(original.Month, roundTrip.Month);
        Assert.Equal(original.Day, roundTrip.Day);
    }

    [Fact]
    public void GregorianDate_RoundTrip_ThroughToGregorianDateTime()
    {
        var original = new PersianDate(2025, 7, 31, 0);
        var dt = PersianCalendarHelper.ToGregorianDateTime(original);

        // Round-trip: construct a new PersianDate from the DateTime's Gregorian parts
        // (no Persian calendar conversion).
        var roundTrip = new PersianDate(dt.Year, dt.Month, dt.Day, 0);

        Assert.Equal(original.Year, roundTrip.Year);
        Assert.Equal(original.Month, roundTrip.Month);
        Assert.Equal(original.Day, roundTrip.Day);
    }
}
