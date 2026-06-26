using Xunit;

namespace ModernPersianDatePicker.Tests;

public class RangePickerTests
{
    [Fact]
    public void IsRangeMode_DefaultIsFalse()
    {
        var picker = new ModernPersianDatePicker();
        Assert.False(picker.IsRangeMode);
    }

    [Fact]
    public void RangeStart_DefaultIsNull()
    {
        var picker = new ModernPersianDatePicker();
        Assert.Null(picker.RangeStart);
    }

    [Fact]
    public void RangeEnd_DefaultIsNull()
    {
        var picker = new ModernPersianDatePicker();
        Assert.Null(picker.RangeEnd);
    }

    [Fact]
    public void RangeStart_CanBeSet()
    {
        var start = new PersianDate(1404, 1, 1, 6);
        var picker = new ModernPersianDatePicker { RangeStart = start };
        Assert.Equal(start, picker.RangeStart);
    }

    [Fact]
    public void RangeEnd_CanBeSet()
    {
        var end = new PersianDate(1404, 1, 10, 0);
        var picker = new ModernPersianDatePicker { RangeEnd = end };
        Assert.Equal(end, picker.RangeEnd);
    }

    [Fact]
    public void IsRangeMode_CanBeEnabled()
    {
        var picker = new ModernPersianDatePicker { IsRangeMode = true };
        Assert.True(picker.IsRangeMode);
    }

    [Fact]
    public void DateRangeSelectedEventArgs_CarriesCorrectValues()
    {
        var start = new PersianDate(1404, 1, 5, 2);
        var end = new PersianDate(1404, 1, 10, 0);
        var args = new DateRangeSelectedEventArgs(start, end);

        Assert.Equal(start, args.RangeStart);
        Assert.Equal(end, args.RangeEnd);
    }

    [Fact]
    public void DateRangeSelectedEventArgs_SingleDate()
    {
        var start = new PersianDate(1404, 1, 5, 2);
        var args = new DateRangeSelectedEventArgs(start, null);

        Assert.Equal(start, args.RangeStart);
        Assert.Null(args.RangeEnd);
    }

    [Fact]
    public void CalendarView_RangeStart_CanBeSet()
    {
        var view = new CalendarView();
        var start = new PersianDate(1404, 3, 15, 3);
        view.RangeStart = start;
        Assert.Equal(start, view.RangeStart);
    }

    [Fact]
    public void CalendarView_RangeEnd_CanBeSet()
    {
        var view = new CalendarView();
        var end = new PersianDate(1404, 3, 20, 1);
        view.RangeEnd = end;
        Assert.Equal(end, view.RangeEnd);
    }

    [Fact]
    public void CalendarView_IsRangeMode_CanBeToggled()
    {
        var view = new CalendarView();
        Assert.False(view.IsRangeMode);
        view.IsRangeMode = true;
        Assert.True(view.IsRangeMode);
    }
}
