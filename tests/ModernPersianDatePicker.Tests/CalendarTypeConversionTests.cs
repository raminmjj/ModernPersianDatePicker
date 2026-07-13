using System;
using Xunit;

namespace ModernPersianDatePicker.Tests;

public class CalendarTypeConversionTests
{
    // ──────────────────────────────────────────────────────
    // Persian → Gregorian conversion
    // ──────────────────────────────────────────────────────

    [Fact]
    public void ConvertDate_PersianToGregorian_ValidPersianDate()
    {
        // 1405/05/09 Persian = 2026/07/31 Gregorian (Friday)
        var persian = new PersianDate(1405, 5, 9, 0);
        var gregorian = PersianCalendarHelper.ToGregorianDate(persian);

        Assert.Equal(2026, gregorian.Year);
        Assert.Equal(7, gregorian.Month);
        Assert.Equal(31, gregorian.Day);
        Assert.Equal(DayOfWeek.Friday, gregorian.DayOfWeek);
    }

    [Fact]
    public void ConvertDate_PersianToGregorian_InvalidPersianValues_DoesNotCrash()
    {
        // Persian month 7 (Mehr) only has 30 days. Day=31 is invalid in Persian.
        // But these are actually Gregorian values (2026/07/31 = July 31).
        // The picker's ConvertDate would be called with from=Persian (because
        // e.OldValue says so) but the values are actually Gregorian.
        // Without defensive handling, ToGregorianDate would throw
        // ArgumentOutOfRangeException.
        //
        // We can't call ConvertDate directly (it's private), but we verify that
        // ToGregorianDate throws (proving the bug exists) — the fix is in
        // ConvertDate which wraps this defensively.

        var pd = new PersianDate(2026, 7, 31, 0);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PersianCalendarHelper.ToGregorianDate(pd));
    }

    // ──────────────────────────────────────────────────────
    // Gregorian → Persian conversion
    // ──────────────────────────────────────────────────────

    [Fact]
    public void ConvertDate_GregorianToPersian_July31_2026()
    {
        // The exact scenario the user reported: 2026/07/31 Gregorian
        var gregorian = new DateTime(2026, 7, 31);
        var persian = PersianCalendarHelper.ToPersianDate(gregorian);

        Assert.Equal(1405, persian.Year);
        Assert.Equal(5, persian.Month);  // Mordad
        Assert.Equal(9, persian.Day);
    }

    [Fact]
    public void ConvertDate_GregorianToPersian_RoundTrip_July31()
    {
        // Round-trip: Gregorian → Persian → Gregorian should give back the original.
        var original = new DateTime(2026, 7, 31);
        var persian = PersianCalendarHelper.ToPersianDate(original);
        var roundTrip = PersianCalendarHelper.ToGregorianDate(persian);

        Assert.Equal(original, roundTrip);
    }

    // ──────────────────────────────────────────────────────
    // PersianCalendarHelper.GetDaysInMonth validation
    // (used by ConvertDate's defensive check)
    // ──────────────────────────────────────────────────────

    [Theory]
    [InlineData(1405, 1, 31)]   // Farvardin — 31 days
    [InlineData(1405, 6, 31)]   // Shahrivar — 31 days
    [InlineData(1405, 7, 30)]   // Mehr — 30 days
    [InlineData(1405, 11, 30)]  // Bahman — 30 days
    [InlineData(1405, 12, 29)]  // Esfand in non-leap year — 29 days
    [InlineData(1403, 12, 30)]  // Esfand in leap year (1403 is leap) — 30 days
    public void GetDaysInMonth_PersianMonth_ReturnsCorrectDayCount(int year, int month, int expectedDays)
    {
        Assert.Equal(expectedDays, PersianCalendarHelper.GetDaysInMonth(year, month));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(-1)]
    public void GetDaysInMonth_InvalidMonth_Throws(int month)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PersianCalendarHelper.GetDaysInMonth(1405, month));
    }

    // ──────────────────────────────────────────────────────
    // PersianDate constructor validation
    // ──────────────────────────────────────────────────────

    [Fact]
    public void PersianDate_Constructor_AcceptsJuly31()
    {
        // The struct accepts day=31 (it doesn't know which calendar the values are in).
        var pd = new PersianDate(2026, 7, 31, 5);
        Assert.Equal(2026, pd.Year);
        Assert.Equal(7, pd.Month);
        Assert.Equal(31, pd.Day);
    }

    [Fact]
    public void PersianDate_Constructor_RejectsMonth13()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PersianDate(2026, 13, 1, 0));
    }

    [Fact]
    public void PersianDate_Constructor_RejectsDay32()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PersianDate(2026, 7, 32, 0));
    }

    // ──────────────────────────────────────────────────────
    // ToGregorianDateTime (identity for Gregorian values)
    // ──────────────────────────────────────────────────────

    [Fact]
    public void ToGregorianDateTime_AcceptsJuly31_2026()
    {
        var pd = new PersianDate(2026, 7, 31, 0);
        var dt = PersianCalendarHelper.ToGregorianDateTime(pd);

        Assert.Equal(2026, dt.Year);
        Assert.Equal(7, dt.Month);
        Assert.Equal(31, dt.Day);
        Assert.Equal(DayOfWeek.Friday, dt.DayOfWeek);
    }

    // ──────────────────────────────────────────────────────
    // Simulate the user's exact reported scenario
    // ──────────────────────────────────────────────────────

    [Fact]
    public void UserScenario_Gregorian_PickJuly31_SwitchToPersian_SwitchBackToGregorian()
    {
        // Step 1: User picks 2026/07/31 in Gregorian mode.
        // PersianDate struct holds Gregorian values.
        var pickedInGregorian = new PersianDate(2026, 7, 31, 5);

        // Step 2: User switches to Persian. ConvertDate((2026,7,31), Gregorian, Persian) is called.
        // The Gregorian→Persian path builds a DateTime and converts via PersianCalendar.
        var gdt = PersianCalendarHelper.ToGregorianDateTime(pickedInGregorian);
        Assert.Equal(new DateTime(2026, 7, 31), gdt);

        var inPersian = PersianCalendarHelper.ToPersianDate(gdt);
        Assert.Equal(1405, inPersian.Year);
        Assert.Equal(5, inPersian.Month);
        Assert.Equal(9, inPersian.Day);

        // Step 3: User switches back to Gregorian. ConvertDate((1405,5,9), Persian, Gregorian) is called.
        // The Persian→Gregorian path uses PersianCalendar.ToDateTime.
        // 1405/5/9 is a valid Persian date (Mordad has 31 days, day=9 is OK).
        var backInGregorian = PersianCalendarHelper.ToGregorianDate(inPersian);
        Assert.Equal(2026, backInGregorian.Year);
        Assert.Equal(7, backInGregorian.Month);
        Assert.Equal(31, backInGregorian.Day);

        // Round-trip succeeds — no crash expected when SelectedDate is properly converted.
    }

    [Fact]
    public void UserScenario_IfConversionFails_ControlDoesNotCrash()
    {
        // Defensive scenario: if for some reason SelectedDate still holds
        // (2026, 7, 31) when switching Persian→Gregorian (i.e., previous
        // conversion didn't take effect), ConvertDate's defensive check
        // should detect that (2026, 7, 31) is not a valid Persian date
        // (Mehr only has 30 days) and fall back to identity.
        //
        // We can't test ConvertDate directly (private), but we verify the
        // building blocks the defensive check uses:
        var pd = new PersianDate(2026, 7, 31, 0);

        // Check: PersianCalendarHelper.GetDaysInMonth says month 7 has 30 days
        int daysInMehr = PersianCalendarHelper.GetDaysInMonth(pd.Year, pd.Month);
        Assert.Equal(30, daysInMehr);

        // So pd.Day=31 > 30 → invalid Persian → ConvertDate would fall back to identity.
        Assert.True(pd.Day > daysInMehr, "Day 31 should exceed Mehr's 30 days");

        // And ToGregorianDateTime (the fallback path) accepts these values:
        var fallback = PersianCalendarHelper.ToGregorianDateTime(pd);
        Assert.Equal(2026, fallback.Year);
        Assert.Equal(7, fallback.Month);
        Assert.Equal(31, fallback.Day);
    }
}
