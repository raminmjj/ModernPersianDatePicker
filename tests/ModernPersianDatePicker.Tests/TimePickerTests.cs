using Xunit;

namespace ModernPersianDatePicker.Tests;

public class TimePickerTests
{
    // ──────────────────────────────────────────────────────
    // 1. Default values
    // ──────────────────────────────────────────────────────

    [Fact]
    public void TimePickerView_DefaultValues_AreZero()
    {
        var picker = new TimePickerView();
        Assert.Equal(0, picker.Hour);
        Assert.Equal(0, picker.Minute);
        Assert.Equal(0, picker.Second);
    }

    [Fact]
    public void ModernPersianDatePicker_IsTimePickerEnabled_DefaultIsFalse()
    {
        var picker = new ModernPersianDatePicker();
        Assert.False(picker.IsTimePickerEnabled);
    }

    [Fact]
    public void ModernPersianDatePicker_TimeProperties_DefaultAreZero()
    {
        var picker = new ModernPersianDatePicker();
        Assert.Equal(0, picker.Hour);
        Assert.Equal(0, picker.Minute);
        Assert.Equal(0, picker.Second);
    }

    // ──────────────────────────────────────────────────────
    // 2. Property validation
    // ──────────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(12)]
    [InlineData(23)]
    public void HourProperty_ValidValues_Accepted(int hour)
    {
        var picker = new ModernPersianDatePicker { Hour = hour };
        Assert.Equal(hour, picker.Hour);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(30)]
    [InlineData(59)]
    public void MinuteProperty_ValidValues_Accepted(int minute)
    {
        var picker = new ModernPersianDatePicker { Minute = minute };
        Assert.Equal(minute, picker.Minute);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(45)]
    [InlineData(59)]
    public void SecondProperty_ValidValues_Accepted(int second)
    {
        var picker = new ModernPersianDatePicker { Second = second };
        Assert.Equal(second, picker.Second);
    }

    // ──────────────────────────────────────────────────────
    // 3. TimePickerView property validation
    // ──────────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    [InlineData(23)]
    public void TimePickerView_Hour_Accepted(int hour)
    {
        var picker = new TimePickerView { Hour = hour };
        Assert.Equal(hour, picker.Hour);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(30)]
    [InlineData(59)]
    public void TimePickerView_Minute_Accepted(int minute)
    {
        var picker = new TimePickerView { Minute = minute };
        Assert.Equal(minute, picker.Minute);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(59)]
    public void TimePickerView_Second_Accepted(int second)
    {
        var picker = new TimePickerView { Second = second };
        Assert.Equal(second, picker.Second);
    }

    // ──────────────────────────────────────────────────────
    // 4. TimeChanged event
    // ──────────────────────────────────────────────────────

    [Fact]
    public void TimePickerView_TimeChanged_FiresOnPropertyChange()
    {
        var picker = new TimePickerView();
        bool fired = false;
        picker.TimeChanged += (_, _) => fired = true;

        picker.Hour = 10;
        Assert.True(fired);

        fired = false;
        picker.Minute = 30;
        Assert.True(fired);

        fired = false;
        picker.Second = 45;
        Assert.True(fired);
    }

    // ──────────────────────────────────────────────────────
    // 5. Integration: time properties forward to TimePickerView
    // ──────────────────────────────────────────────────────

    [Fact]
    public void TimeProperties_CanBeSetTogether()
    {
        var picker = new ModernPersianDatePicker
        {
            IsTimePickerEnabled = true,
            Hour = 14,
            Minute = 30,
            Second = 15
        };

        Assert.True(picker.IsTimePickerEnabled);
        Assert.Equal(14, picker.Hour);
        Assert.Equal(30, picker.Minute);
        Assert.Equal(15, picker.Second);
    }
}
