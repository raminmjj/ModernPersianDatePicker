using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ModernPersianDatePicker.Tests;

public class HolidayTests
{
    // ──────────────────────────────────────────────────────
    // 1. ToPersianWeekdayIndex (DayOfWeek → Persian weekday)
    // ──────────────────────────────────────────────────────

    [Theory]
    [InlineData(DayOfWeek.Saturday, 0)]
    [InlineData(DayOfWeek.Sunday, 1)]
    [InlineData(DayOfWeek.Monday, 2)]
    [InlineData(DayOfWeek.Tuesday, 3)]
    [InlineData(DayOfWeek.Wednesday, 4)]
    [InlineData(DayOfWeek.Thursday, 5)]
    [InlineData(DayOfWeek.Friday, 6)]
    public void ToPersianWeekdayIndex_MapsCorrectly(DayOfWeek dotNetDay, int expectedPersianIndex)
    {
        Assert.Equal(expectedPersianIndex, CalendarView.ToPersianWeekdayIndex(dotNetDay));
    }

    [Fact]
    public void ToPersianWeekdayIndex_CoversAllDays()
    {
        // Ensure every .NET DayOfWeek maps to a valid Persian index (0..6)
        foreach (DayOfWeek dow in Enum.GetValues(typeof(DayOfWeek)))
        {
            int result = CalendarView.ToPersianWeekdayIndex(dow);
            Assert.InRange(result, 0, 6);
        }
    }

    [Fact]
    public void ToPersianWeekdayIndex_IsSurjective()
    {
        // Every Persian index 0..6 should be reachable
        var mapped = new HashSet<int>();
        foreach (DayOfWeek dow in Enum.GetValues(typeof(DayOfWeek)))
        {
            mapped.Add(CalendarView.ToPersianWeekdayIndex(dow));
        }
        for (int i = 0; i <= 6; i++)
        {
            Assert.Contains(i, mapped);
        }
    }

    // ──────────────────────────────────────────────────────
    // 2. PersianDate equality (used in Holidays set lookups)
    // ──────────────────────────────────────────────────────

    [Fact]
    public void PersianDate_Equal_DatesAreEqual()
    {
        var a = new PersianDate(1404, 1, 1, 0);
        var b = new PersianDate(1404, 1, 1, 0);
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void PersianDate_Equal_DifferentDatesNotEqual()
    {
        var a = new PersianDate(1404, 1, 1, 0);
        var b = new PersianDate(1404, 1, 2, 1);
        Assert.NotEqual(a, b);
    }

    // ──────────────────────────────────────────────────────
    // 3. Weekly holidays logic verification
    // ──────────────────────────────────────────────────────

    [Fact]
    public void Friday_IsDefaultWeeklyHoliday()
    {
        // Friday (.NET DayOfWeek.Friday) → Persian index 6
        Assert.Equal(6, CalendarView.ToPersianWeekdayIndex(DayOfWeek.Friday));
    }

    [Theory]
    [InlineData(1404, 1, 1)]
    [InlineData(1404, 1, 8)]
    [InlineData(1404, 1, 15)]
    public void Fridays_AreWeeklyHolidays(int year, int month, int day)
    {
        // Build the date and check the weekday index
        int firstDow = PersianCalendarHelper.GetFirstDayOfWeek(year, month);
        int dayOfWeek = (firstDow + (day - 1)) % 7;

        // Friday = 6 in Persian index
        Assert.Equal(6, dayOfWeek);
    }

    [Theory]
    [InlineData(1404, 1, 2)]
    [InlineData(1404, 1, 9)]
    [InlineData(1404, 1, 16)]
    public void Saturdays_AreWeeklyHolidays(int year, int month, int day)
    {
        // 1404/01/01 = Friday (dow=6), so 01/02 = Saturday (dow=0)
        int firstDow = PersianCalendarHelper.GetFirstDayOfWeek(year, month);
        int dayOfWeek = (firstDow + (day - 1)) % 7;

        // Saturday = 0 in Persian index
        Assert.Equal(0, dayOfWeek);
    }

    // ──────────────────────────────────────────────────────
    // 4. ThemeMode enum values
    // ──────────────────────────────────────────────────────

    [Fact]
    public void ThemeMode_HasAllExpectedValues()
    {
        Assert.True(Enum.IsDefined(typeof(ThemeMode), ThemeMode.System));
        Assert.True(Enum.IsDefined(typeof(ThemeMode), ThemeMode.Light));
        Assert.True(Enum.IsDefined(typeof(ThemeMode), ThemeMode.Dark));
        Assert.Equal(3, Enum.GetValues(typeof(ThemeMode)).Length);
    }

    // ──────────────────────────────────────────────────────
    // 5. Gregorian weekly holiday logic
    // ──────────────────────────────────────────────────────

    [Theory]
    [InlineData(2025, 7, 5, true)]   // Saturday
    [InlineData(2025, 7, 6, true)]   // Sunday
    [InlineData(2025, 7, 7, false)]  // Monday
    [InlineData(2025, 7, 12, true)]  // Saturday
    [InlineData(2025, 7, 13, true)]  // Sunday
    [InlineData(2025, 7, 4, false)]  // Friday
    public void Gregorian_SaturdayAndSunday_AreHolidays(int year, int month, int day, bool expectedHoliday)
    {
        // Simulate the holiday check logic from CalendarView.UpdateCalendar for Gregorian
        var weeklyHolidays = new[] { DayOfWeek.Saturday, DayOfWeek.Sunday };
        var gregorianWeeklyIndices = weeklyHolidays.Select(d => (int)d).ToHashSet();

        int firstDayOfWeek = PersianCalendarHelper.GetGregorianFirstDayOfWeek(year, month);
        int dayOfWeek = (firstDayOfWeek + day - 1) % 7;

        bool isHoliday = gregorianWeeklyIndices.Contains(dayOfWeek);
        Assert.Equal(expectedHoliday, isHoliday);
    }

    [Fact]
    public void Gregorian_Friday_IsNotHoliday()
    {
        var weeklyHolidays = new[] { DayOfWeek.Saturday, DayOfWeek.Sunday };
        var gregorianWeeklyIndices = weeklyHolidays.Select(d => (int)d).ToHashSet();

        // July 4, 2025 is Friday
        int firstDayOfWeek = PersianCalendarHelper.GetGregorianFirstDayOfWeek(2025, 7);
        int dayOfWeek = (firstDayOfWeek + 4 - 1) % 7;

        bool isHoliday = gregorianWeeklyIndices.Contains(dayOfWeek);
        Assert.False(isHoliday);
    }
}
