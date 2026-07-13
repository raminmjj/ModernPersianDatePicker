using System;
using Xunit;

namespace ModernPersianDatePicker.Tests;

public class TimePickerTests
{
    // ──────────────────────────────────────────────────────
    // 1. Default values
    // ──────────────────────────────────────────────────────

    [Fact]
    public void TimePickerView_DefaultTime_IsZero()
    {
        var picker = new TimePickerView();
        Assert.Equal(TimeSpan.Zero, picker.Time);
    }

    [Fact]
    public void ModernPersianDatePicker_IsTimePickerEnabled_DefaultIsFalse()
    {
        var picker = new ModernPersianDatePicker();
        Assert.False(picker.IsTimePickerEnabled);
    }

    [Fact]
    public void ModernPersianDatePicker_SelectedTime_DefaultIsZero()
    {
        var picker = new ModernPersianDatePicker();
        Assert.Equal(TimeSpan.Zero, picker.SelectedTime);
    }

    // ──────────────────────────────────────────────────────
    // 2. TimePickerView property validation
    // ──────────────────────────────────────────────────────

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(12, 30, 45)]
    [InlineData(23, 59, 59)]
    public void TimePickerView_Time_Accepted(int h, int m, int s)
    {
        var value = new TimeSpan(h, m, s);
        var picker = new TimePickerView { Time = value };
        Assert.Equal(value, picker.Time);
    }

    // ──────────────────────────────────────────────────────
    // 3. TimeChanged event
    // ──────────────────────────────────────────────────────

    [Fact]
    public void TimePickerView_TimeChanged_FiresOnPropertyChange()
    {
        var picker = new TimePickerView();
        bool fired = false;
        picker.TimeChanged += (_, _) => fired = true;

        picker.Time = new TimeSpan(10, 30, 45);
        Assert.True(fired);
    }

    // ──────────────────────────────────────────────────────
    // 4. ModernPersianDatePicker SelectedTime
    // ──────────────────────────────────────────────────────

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(14, 30, 15)]
    [InlineData(23, 59, 59)]
    public void ModernPersianDatePicker_SelectedTime_Accepted(int h, int m, int s)
    {
        var value = new TimeSpan(h, m, s);
        var picker = new ModernPersianDatePicker
        {
            IsTimePickerEnabled = true,
            SelectedTime = value
        };

        Assert.True(picker.IsTimePickerEnabled);
        Assert.Equal(value, picker.SelectedTime);
    }

    // ──────────────────────────────────────────────────────
    // 5. Range time properties (kept from previous behaviour)
    // ──────────────────────────────────────────────────────

    [Fact]
    public void ModernPersianDatePicker_RangeStartTime_DefaultIsZero()
    {
        var picker = new ModernPersianDatePicker();
        Assert.Equal(TimeSpan.Zero, picker.RangeStartTime);
    }

    [Fact]
    public void ModernPersianDatePicker_RangeEndTime_DefaultIsEndOfDay()
    {
        var picker = new ModernPersianDatePicker();
        Assert.Equal(new TimeSpan(23, 59, 59), picker.RangeEndTime);
    }
}
