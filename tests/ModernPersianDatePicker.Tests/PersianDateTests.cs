using System;
using Xunit;

namespace ModernPersianDatePicker.Tests;

public class PersianDateTests
{
    // ──────────────────────────────────────────────────────
    // 1. Constructor validation
    // ──────────────────────────────────────────────────────

    [Fact]
    public void Constructor_ValidDate_Succeeds()
    {
        var date = new PersianDate(1403, 10, 19, 3);
        Assert.Equal(1403, date.Year);
        Assert.Equal(10, date.Month);
        Assert.Equal(19, date.Day);
        Assert.Equal(3, date.DayOfWeek);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_InvalidYear_Throws(int year)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PersianDate(year, 1, 1, 0));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void Constructor_InvalidMonth_Throws(int month)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PersianDate(1403, month, 1, 0));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(32)]
    public void Constructor_InvalidDay_Throws(int day)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PersianDate(1403, 1, day, 0));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(7)]
    public void Constructor_InvalidDayOfWeek_Throws(int dayOfWeek)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PersianDate(1403, 1, 1, dayOfWeek));
    }

    // ──────────────────────────────────────────────────────
    // 2. ToString formats
    // ──────────────────────────────────────────────────────

    [Fact]
    public void ToString_DefaultFormat_ReturnsSlashSeparated()
    {
        var date = new PersianDate(1403, 10, 19, 3);
        Assert.Equal("1403/10/19", date.ToString());
    }

    [Fact]
    public void ToString_ShortFormat_PadsWithZeros()
    {
        var date = new PersianDate(1403, 1, 5, 0);
        Assert.Equal("1403/01/05", date.ToString("short"));
    }

    [Fact]
    public void ToString_LongFormat_ContainsDayAndMonthNames()
    {
        var date = new PersianDate(1403, 1, 5, 0); // شنبه ۵ فروردین
        var longForm = date.ToString("long");
        Assert.Contains("فروردین", longForm);
        Assert.Contains("شنبه", longForm);
        Assert.Contains("5", longForm);
        Assert.Contains("1403", longForm);
    }

    [Fact]
    public void ToString_MonthFormat_ContainsMonthNameAndYear()
    {
        var date = new PersianDate(1403, 1, 5, 0);
        var monthForm = date.ToString("month");
        Assert.Contains("فروردین", monthForm);
        Assert.Contains("1403", monthForm);
        // Should NOT contain the day number
        Assert.DoesNotContain("۵", monthForm);
    }

    // ──────────────────────────────────────────────────────
    // 3. Equality
    // ──────────────────────────────────────────────────────

    [Fact]
    public void Equality_SameDate_AreEqual()
    {
        var a = new PersianDate(1403, 10, 19, 3);
        var b = new PersianDate(1403, 10, 19, 3);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentDay_AreNotEqual()
    {
        var a = new PersianDate(1403, 10, 19, 3);
        var b = new PersianDate(1403, 10, 20, 4);
        Assert.True(a != b);
        Assert.False(a == b);
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void Equality_DifferentMonth_AreNotEqual()
    {
        var a = new PersianDate(1403, 10, 19, 3);
        var b = new PersianDate(1403, 11, 19, 0);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Equality_DifferentYear_AreNotEqual()
    {
        var a = new PersianDate(1403, 10, 19, 3);
        var b = new PersianDate(1404, 10, 19, 0);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    // ──────────────────────────────────────────────────────
    // 4. Comparison / Ordering
    // ──────────────────────────────────────────────────────

    [Fact]
    public void CompareTo_SameDate_ReturnsZero()
    {
        var a = new PersianDate(1403, 10, 19, 3);
        var b = new PersianDate(1403, 10, 19, 3);
        Assert.Equal(0, a.CompareTo(b));
    }

    [Fact]
    public void CompareTo_EarlierVsLater_ReturnsNegative()
    {
        var earlier = new PersianDate(1403, 1, 1, 0);
        var later = new PersianDate(1403, 10, 19, 3);
        Assert.True(earlier.CompareTo(later) < 0);
        Assert.True(later.CompareTo(earlier) > 0);
    }

    [Fact]
    public void LessThan_GreaterThan_Operators()
    {
        var earlier = new PersianDate(1403, 1, 1, 0);
        var later = new PersianDate(1403, 10, 19, 3);

        Assert.True(earlier < later);
        Assert.False(earlier > later);
        Assert.True(later > earlier);
        Assert.False(later < earlier);
        Assert.True(earlier <= later);
        Assert.True(later >= earlier);
    }

    // ──────────────────────────────────────────────────────
    // 5. ToDateTime round-trip
    // ──────────────────────────────────────────────────────

    [Fact]
    public void ToDateTime_RoundTrip_PreservesDate()
    {
        // 1403/10/12 -> 2025-01-01 (verified)
        var original = new PersianDate(1403, 10, 12, 4);
        var gregorian = original.ToDateTime();
        var roundTrip = PersianCalendarHelper.ToPersianDate(gregorian);

        Assert.Equal(original.Year, roundTrip.Year);
        Assert.Equal(original.Month, roundTrip.Month);
        Assert.Equal(original.Day, roundTrip.Day);
    }
}
