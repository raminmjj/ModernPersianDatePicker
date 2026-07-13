using System;
using Xunit;

namespace ModernPersianDatePicker.Tests;

public class TimeEventsTests
{
    // ──────────────────────────────────────────────────────
    // SelectedTimeChangedEventArgs
    // ──────────────────────────────────────────────────────

    [Fact]
    public void SelectedTimeChangedEventArgs_CarriesOldAndNewValues()
    {
        var oldTime = new TimeSpan(10, 0, 0);
        var newTime = new TimeSpan(11, 30, 0);
        var args = new SelectedTimeChangedEventArgs(oldTime, newTime);

        Assert.Equal(oldTime, args.OldTime);
        Assert.Equal(newTime, args.NewTime);
    }

    [Fact]
    public void SelectedTimeChangedEventArgs_AcceptsZeroValues()
    {
        var args = new SelectedTimeChangedEventArgs(TimeSpan.Zero, TimeSpan.Zero);
        Assert.Equal(TimeSpan.Zero, args.OldTime);
        Assert.Equal(TimeSpan.Zero, args.NewTime);
    }

    // ──────────────────────────────────────────────────────
    // SelectedTimeChanged event (single-date mode)
    // ──────────────────────────────────────────────────────

    [Fact]
    public void SelectedTimeChanged_FiresWhenSelectedTimeChanges()
    {
        var picker = new ModernPersianDatePicker { SelectedTime = TimeSpan.Zero };
        TimeSpan? capturedNew = null;
        TimeSpan? capturedOld = null;
        int fireCount = 0;

        picker.SelectedTimeChanged += (_, e) =>
        {
            capturedOld = e.OldTime;
            capturedNew = e.NewTime;
            fireCount++;
        };

        picker.SelectedTime = new TimeSpan(14, 30, 0);

        Assert.Equal(1, fireCount);
        Assert.Equal(TimeSpan.Zero, capturedOld);
        Assert.Equal(new TimeSpan(14, 30, 0), capturedNew);
    }

    [Fact]
    public void SelectedTimeChanged_DoesNotFireWhenValueUnchanged()
    {
        var picker = new ModernPersianDatePicker { SelectedTime = new TimeSpan(10, 0, 0) };
        int fireCount = 0;
        picker.SelectedTimeChanged += (_, _) => fireCount++;

        // Set to the same value — Avalonia StyledProperty skips notification when equal.
        picker.SelectedTime = new TimeSpan(10, 0, 0);

        Assert.Equal(0, fireCount);
    }

    [Fact]
    public void SelectedTimeChanged_FiresOnEachChange()
    {
        var picker = new ModernPersianDatePicker { SelectedTime = TimeSpan.Zero };
        int fireCount = 0;
        picker.SelectedTimeChanged += (_, _) => fireCount++;

        picker.SelectedTime = new TimeSpan(1, 0, 0);
        picker.SelectedTime = new TimeSpan(2, 0, 0);
        picker.SelectedTime = new TimeSpan(3, 0, 0);

        Assert.Equal(3, fireCount);
        Assert.Equal(new TimeSpan(3, 0, 0), picker.SelectedTime);
    }

    [Fact]
    public void SelectedTimeChanged_PassesOldAndNewCorrectly()
    {
        var picker = new ModernPersianDatePicker { SelectedTime = new TimeSpan(8, 0, 0) };
        TimeSpan? lastOld = null;
        TimeSpan? lastNew = null;

        picker.SelectedTimeChanged += (_, e) =>
        {
            lastOld = e.OldTime;
            lastNew = e.NewTime;
        };

        picker.SelectedTime = new TimeSpan(9, 15, 30);

        Assert.Equal(new TimeSpan(8, 0, 0), lastOld);
        Assert.Equal(new TimeSpan(9, 15, 30), lastNew);
    }

    [Fact]
    public void SelectedTimeChanged_UnsubscribeStopsEvents()
    {
        var picker = new ModernPersianDatePicker { SelectedTime = TimeSpan.Zero };
        int fireCount = 0;
        EventHandler<SelectedTimeChangedEventArgs> handler = (_, _) => fireCount++;

        picker.SelectedTimeChanged += handler;
        picker.SelectedTime = new TimeSpan(1, 0, 0);
        Assert.Equal(1, fireCount);

        picker.SelectedTimeChanged -= handler;
        picker.SelectedTime = new TimeSpan(2, 0, 0);
        Assert.Equal(1, fireCount); // still 1 — handler removed
    }

    // ──────────────────────────────────────────────────────
    // RangeTimeChanged event (range mode)
    // ──────────────────────────────────────────────────────

    [Fact]
    public void RangeTimeChanged_FiresWhenRangeStartTimeChanges()
    {
        var picker = new ModernPersianDatePicker { RangeStartTime = TimeSpan.Zero };
        int fireCount = 0;
        picker.RangeTimeChanged += (_, _) => fireCount++;

        picker.RangeStartTime = new TimeSpan(8, 0, 0);

        Assert.Equal(1, fireCount);
        Assert.Equal(new TimeSpan(8, 0, 0), picker.RangeStartTime);
    }

    [Fact]
    public void RangeTimeChanged_FiresWhenRangeEndTimeChanges()
    {
        var picker = new ModernPersianDatePicker
        {
            RangeEndTime = new TimeSpan(23, 59, 59)
        };
        int fireCount = 0;
        picker.RangeTimeChanged += (_, _) => fireCount++;

        picker.RangeEndTime = new TimeSpan(17, 0, 0);

        Assert.Equal(1, fireCount);
        Assert.Equal(new TimeSpan(17, 0, 0), picker.RangeEndTime);
    }

    [Fact]
    public void RangeTimeChanged_FiresForBothEndpoints()
    {
        var picker = new ModernPersianDatePicker();
        int fireCount = 0;
        picker.RangeTimeChanged += (_, _) => fireCount++;

        picker.RangeStartTime = new TimeSpan(9, 0, 0);
        picker.RangeEndTime = new TimeSpan(18, 0, 0);

        Assert.Equal(2, fireCount);
    }

    [Fact]
    public void RangeTimeChanged_DoesNotFireWhenValuesUnchanged()
    {
        var picker = new ModernPersianDatePicker
        {
            RangeStartTime = new TimeSpan(8, 0, 0),
            RangeEndTime = new TimeSpan(17, 0, 0)
        };
        int fireCount = 0;
        picker.RangeTimeChanged += (_, _) => fireCount++;

        // Same values — no event expected.
        picker.RangeStartTime = new TimeSpan(8, 0, 0);
        picker.RangeEndTime = new TimeSpan(17, 0, 0);

        Assert.Equal(0, fireCount);
    }

    [Fact]
    public void RangeTimeChanged_UnsubscribeStopsEvents()
    {
        var picker = new ModernPersianDatePicker();
        int fireCount = 0;
        EventHandler handler = (_, _) => fireCount++;

        picker.RangeTimeChanged += handler;
        picker.RangeStartTime = new TimeSpan(9, 0, 0);
        Assert.Equal(1, fireCount);

        picker.RangeTimeChanged -= handler;
        picker.RangeEndTime = new TimeSpan(20, 0, 0);
        Assert.Equal(1, fireCount); // unchanged
    }

    // ──────────────────────────────────────────────────────
    // DateRangeSelectedEventArgs with times
    // ──────────────────────────────────────────────────────

    [Fact]
    public void DateRangeSelectedEventArgs_DefaultsToStartAndEndOfDay()
    {
        var start = new PersianDate(1404, 1, 5, 2);
        var end = new PersianDate(1404, 1, 10, 0);
        var args = new DateRangeSelectedEventArgs(start, end);

        Assert.Equal(DateRangeSelectedEventArgs.DefaultStartTime, args.StartTime);
        Assert.Equal(DateRangeSelectedEventArgs.DefaultEndTime, args.EndTime);
        Assert.Equal(TimeSpan.Zero, args.StartTime);
        Assert.Equal(new TimeSpan(23, 59, 59), args.EndTime);
    }

    [Fact]
    public void DateRangeSelectedEventArgs_CarriesExplicitTimes()
    {
        var start = new PersianDate(1404, 1, 5, 2);
        var end = new PersianDate(1404, 1, 10, 0);
        var startTime = new TimeSpan(9, 0, 0);
        var endTime = new TimeSpan(17, 30, 0);

        var args = new DateRangeSelectedEventArgs(start, end, startTime, endTime);

        Assert.Equal(start, args.RangeStart);
        Assert.Equal(end, args.RangeEnd);
        Assert.Equal(startTime, args.StartTime);
        Assert.Equal(endTime, args.EndTime);
    }

    [Fact]
    public void DateRangeSelectedEventArgs_DefaultStartTime_IsZero()
    {
        Assert.Equal(TimeSpan.Zero, DateRangeSelectedEventArgs.DefaultStartTime);
    }

    [Fact]
    public void DateRangeSelectedEventArgs_DefaultEndTime_IsEndOfDay()
    {
        Assert.Equal(new TimeSpan(23, 59, 59), DateRangeSelectedEventArgs.DefaultEndTime);
    }

    // ──────────────────────────────────────────────────────
    // QuickRangeSelectedEventArgs with times
    // ──────────────────────────────────────────────────────

    [Fact]
    public void QuickRangeSelectedEventArgs_DefaultsToStartAndEndOfDay()
    {
        var start = new PersianDate(1404, 1, 5, 2);
        var end = new PersianDate(1404, 1, 10, 0);
        var args = new QuickRangeSelectedEventArgs(start, end, "custom");

        Assert.Equal(DateRangeSelectedEventArgs.DefaultStartTime, args.StartTime);
        Assert.Equal(DateRangeSelectedEventArgs.DefaultEndTime, args.EndTime);
    }

    [Fact]
    public void QuickRangeSelectedEventArgs_CarriesExplicitTimes()
    {
        var start = new PersianDate(1404, 1, 5, 2);
        var end = new PersianDate(1404, 1, 10, 0);
        var startTime = new TimeSpan(8, 30, 0);
        var endTime = new TimeSpan(19, 45, 0);

        var args = new QuickRangeSelectedEventArgs(start, end, "custom", startTime, endTime);

        Assert.Equal(start, args.RangeStart);
        Assert.Equal(end, args.RangeEnd);
        Assert.Equal("custom", args.RangeType);
        Assert.Equal(startTime, args.StartTime);
        Assert.Equal(endTime, args.EndTime);
    }

    [Fact]
    public void QuickRangeSelectedEventArgs_PreservesRangeType()
    {
        var start = new PersianDate(1404, 1, 5, 2);
        var end = new PersianDate(1404, 1, 5, 2);

        foreach (var rt in new[] { "today", "week", "month", "year", "season", "halfyear", "custom" })
        {
            var args = new QuickRangeSelectedEventArgs(start, end, rt);
            Assert.Equal(rt, args.RangeType);
        }
    }
}
