using ModernPersianDatePicker.Localizations;
using Xunit;

namespace ModernPersianDatePicker.Tests;

public class GregorianCalendarTests
{
    [Fact]
    public void CalendarType_DefaultIsPersian()
    {
        var picker = new ModernPersianDatePicker();
        Assert.Equal(CalendarType.Persian, picker.CalendarType);
    }

    [Fact]
    public void CalendarType_CanBeSetToGregorian()
    {
        var picker = new ModernPersianDatePicker { CalendarType = CalendarType.Gregorian };
        Assert.Equal(CalendarType.Gregorian, picker.CalendarType);
    }

    [Fact]
    public void CalendarView_CalendarType_CanBeToggled()
    {
        var view = new CalendarView();
        Assert.Equal(CalendarType.Persian, view.CalendarType);
        view.CalendarType = CalendarType.Gregorian;
        Assert.Equal(CalendarType.Gregorian, view.CalendarType);
    }

    [Theory]
    [InlineData(2025, 1, 31)]
    [InlineData(2025, 2, 28)]
    [InlineData(2024, 2, 29)]
    [InlineData(2025, 4, 30)]
    public void GregorianDaysInMonth_Correct(int year, int month, int expected)
    {
        Assert.Equal(expected, PersianCalendarHelper.GetGregorianDaysInMonth(year, month));
    }

    [Theory]
    [InlineData(2024, true)]
    [InlineData(2025, false)]
    [InlineData(2000, true)]
    [InlineData(1900, false)]
    public void IsGregorianLeapYear_Correct(int year, bool expected)
    {
        Assert.Equal(expected, PersianCalendarHelper.IsGregorianLeapYear(year));
    }

    [Fact]
    public void GetGregorianFirstDayOfWeek_ReturnsCorrectDay()
    {
        // 2025-01-01 is Wednesday (3)
        Assert.Equal(3, PersianCalendarHelper.GetGregorianFirstDayOfWeek(2025, 1));
    }

    [Theory]
    [InlineData(1, "January")]
    [InlineData(6, "June")]
    [InlineData(12, "December")]
    public void GetGregorianMonthName_English(int month, string expected)
    {
        var loc = new EnglishLocalization();
        Assert.Equal(expected, PersianCalendarHelper.GetGregorianMonthName(month, loc));
    }

    [Theory]
    [InlineData(0, "Sunday")]
    [InlineData(3, "Wednesday")]
    [InlineData(6, "Saturday")]
    public void GetGregorianDayName_English(int dayIndex, string expected)
    {
        var loc = new EnglishLocalization();
        Assert.Equal(expected, PersianCalendarHelper.GetGregorianDayName(dayIndex, loc));
    }

    [Theory]
    [InlineData(0, "Sun")]
    [InlineData(5, "Fri")]
    [InlineData(6, "Sat")]
    public void GetGregorianShortDayName_English(int dayIndex, string expected)
    {
        var loc = new EnglishLocalization();
        Assert.Equal(expected, PersianCalendarHelper.GetGregorianShortDayName(dayIndex, loc));
    }

    [Fact]
    public void FirstDayOfWeek_DefaultIsZero()
    {
        var view = new CalendarView();
        Assert.Equal(0, view.FirstDayOfWeek);
    }

    [Fact]
    public void FirstDayOfWeek_CanBeSet()
    {
        var view = new CalendarView { FirstDayOfWeek = 1 };
        Assert.Equal(1, view.FirstDayOfWeek);
    }
}
