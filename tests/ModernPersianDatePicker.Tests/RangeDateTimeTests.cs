using System;
using System.Globalization;
using Xunit;

namespace ModernPersianDatePicker.Tests;

/// <summary>
/// The calendar-neutral range values <see cref="ModernPersianDatePicker.RangeStartDateTime"/> /
/// <see cref="ModernPersianDatePicker.RangeEndDateTime"/>: kept in sync with
/// <see cref="ModernPersianDatePicker.RangeStart"/> / <see cref="ModernPersianDatePicker.RangeEnd"/>
/// both ways, unchanged by a calendar / culture / language switch, cleared by
/// <see cref="ModernPersianDatePicker.Clear"/>; and typed range text is parsed, with an
/// invalid entry reported under <see cref="InvalidValueAction.Keep"/>, never replaced.
/// </summary>
public class RangeDateTimeTests
{
    private static readonly PersianCalendar Persian = new();

    // 1405/09/01 and 1405/09/30 (Azar 1405).
    private static readonly DateTime AzarFirst = Persian.ToDateTime(1405, 9, 1, 0, 0, 0, 0);
    private static readonly DateTime AzarLast = Persian.ToDateTime(1405, 9, 30, 0, 0, 0, 0);

    private static void AssertDate(PersianDate? actual, int year, int month, int day)
    {
        Assert.True(actual.HasValue, "Expected a date, got null.");
        Assert.Equal((year, month, day), (actual!.Value.Year, actual.Value.Month, actual.Value.Day));
    }

    private static void AssertGregorian(PersianDate? actual, DateTime expected)
        => AssertDate(actual, expected.Year, expected.Month, expected.Day);

    private static T WithCulture<T>(string name, Func<T> action)
    {
        var previous = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo(name);
        try
        {
            return action();
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    private static ModernPersianDatePicker PersianRangePicker(InvalidValueAction action = InvalidValueAction.Keep)
        => new()
        {
            CalendarType = CalendarType.Persian,
            IsRangeMode = true,
            InvalidValueAction = action,
        };

    // ──────────────────────────────────────────────────────
    // Sync in both directions
    // ──────────────────────────────────────────────────────

    [Fact]
    public void Defaults_AreNull()
    {
        var picker = new ModernPersianDatePicker();
        Assert.Null(picker.RangeStartDateTime);
        Assert.Null(picker.RangeEndDateTime);
    }

    [Fact]
    public void SettingTheRange_SetsTheDateTimes()
    {
        var picker = PersianRangePicker();

        picker.RangeStart = new PersianDate(1405, 9, 1, 0);
        picker.RangeEnd = new PersianDate(1405, 9, 30, 0);

        Assert.Equal(AzarFirst, picker.RangeStartDateTime);
        Assert.Equal(AzarLast, picker.RangeEndDateTime);
    }

    [Fact]
    public void SettingTheDateTimes_SetsTheRange_InPersian()
    {
        var picker = PersianRangePicker();

        picker.RangeStartDateTime = AzarFirst;
        picker.RangeEndDateTime = AzarLast;

        AssertDate(picker.RangeStart, 1405, 9, 1);
        AssertDate(picker.RangeEnd, 1405, 9, 30);
    }

    [Fact]
    public void SettingTheDateTimes_SetsTheRange_InGregorian()
    {
        var picker = new ModernPersianDatePicker { CalendarType = CalendarType.Gregorian, IsRangeMode = true };

        picker.RangeStartDateTime = new DateTime(2026, 11, 22);
        picker.RangeEndDateTime = new DateTime(2026, 12, 21);

        AssertDate(picker.RangeStart, 2026, 11, 22);
        AssertDate(picker.RangeEnd, 2026, 12, 21);
    }

    [Fact]
    public void TheTimeOfDayOfASetDateTime_IsNotInterpreted()
    {
        var picker = PersianRangePicker();

        picker.RangeStartDateTime = AzarFirst.AddHours(23).AddMinutes(30);

        AssertDate(picker.RangeStart, 1405, 9, 1);
    }

    [Fact]
    public void TheRangeAndTheSingleDate_AreIndependent()
    {
        var picker = PersianRangePicker();

        picker.RangeStartDateTime = AzarFirst;

        Assert.Null(picker.SelectedDate);
        Assert.Null(picker.SelectedDateTime);
        Assert.Null(picker.RangeEndDateTime);
    }

    // ──────────────────────────────────────────────────────
    // Calendar / culture / language switches keep the days
    // ──────────────────────────────────────────────────────

    [Fact]
    public void ACalendarSwitch_KeepsTheDays_AndConvertsTheRange()
    {
        var picker = PersianRangePicker();
        picker.RangeStart = new PersianDate(1405, 9, 1, 0);
        picker.RangeEnd = new PersianDate(1405, 9, 30, 0);

        picker.CalendarType = CalendarType.Gregorian;

        Assert.Equal(AzarFirst, picker.RangeStartDateTime);
        Assert.Equal(AzarLast, picker.RangeEndDateTime);
        AssertGregorian(picker.RangeStart, AzarFirst);
        AssertGregorian(picker.RangeEnd, AzarLast);

        picker.CalendarType = CalendarType.Persian;

        Assert.Equal(AzarFirst, picker.RangeStartDateTime);
        Assert.Equal(AzarLast, picker.RangeEndDateTime);
        AssertDate(picker.RangeStart, 1405, 9, 1);
        AssertDate(picker.RangeEnd, 1405, 9, 30);
    }

    [Fact]
    public void ACalendarSwitch_DoesNotRaiseADateTimeChange()
    {
        var picker = PersianRangePicker();
        picker.RangeStartDateTime = AzarFirst;
        picker.RangeEndDateTime = AzarLast;
        int changes = 0;
        picker.PropertyChanged += (_, e) =>
        {
            if (e.Property == ModernPersianDatePicker.RangeStartDateTimeProperty
                || e.Property == ModernPersianDatePicker.RangeEndDateTimeProperty)
                changes++;
        };

        picker.CalendarType = CalendarType.Gregorian;
        picker.CalendarType = CalendarType.Persian;

        Assert.Equal(0, changes);
    }

    [Fact]
    public void Auto_ACultureSwitch_ConvertsTheRange_AndKeepsTheDays()
    {
        var picker = WithCulture("fa-IR", () =>
        {
            var p = new ModernPersianDatePicker { CalendarType = CalendarType.Auto, IsRangeMode = true };
            p.RangeStartDateTime = AzarFirst;
            p.RangeEndDateTime = AzarLast;
            return p;
        });
        AssertDate(picker.RangeStart, 1405, 9, 1);
        AssertDate(picker.RangeEnd, 1405, 9, 30);

        WithCulture("en-US", () =>
        {
            picker.RefreshCalendar();
            return 0;
        });

        Assert.Equal(CalendarType.Gregorian, picker.SelectedDateCalendar);
        AssertGregorian(picker.RangeStart, AzarFirst);
        AssertGregorian(picker.RangeEnd, AzarLast);
        Assert.Equal(AzarFirst, picker.RangeStartDateTime);
        Assert.Equal(AzarLast, picker.RangeEndDateTime);

        WithCulture("fa-IR", () =>
        {
            picker.RefreshCalendar();
            return 0;
        });

        AssertDate(picker.RangeStart, 1405, 9, 1);
        AssertDate(picker.RangeEnd, 1405, 9, 30);
        Assert.Equal(AzarFirst, picker.RangeStartDateTime);
        Assert.Equal(AzarLast, picker.RangeEndDateTime);
    }

    [Theory]
    [InlineData(CalendarLanguage.English)]
    [InlineData(CalendarLanguage.Arabic)]
    [InlineData(CalendarLanguage.Kurdish)]
    public void ALanguageSwitch_ChangesNeitherTheRangeNorTheDays(CalendarLanguage language)
    {
        var picker = PersianRangePicker();
        picker.RangeStartDateTime = AzarFirst;
        picker.RangeEndDateTime = AzarLast;

        picker.Language = language;
        picker.Language = CalendarLanguage.Farsi;

        AssertDate(picker.RangeStart, 1405, 9, 1);
        AssertDate(picker.RangeEnd, 1405, 9, 30);
        Assert.Equal(AzarFirst, picker.RangeStartDateTime);
        Assert.Equal(AzarLast, picker.RangeEndDateTime);
    }

    // ──────────────────────────────────────────────────────
    // Clearing
    // ──────────────────────────────────────────────────────

    [Fact]
    public void Clear_EmptiesTheRange_AndBothDateTimes()
    {
        var picker = PersianRangePicker();
        picker.RangeStartDateTime = AzarFirst;
        picker.RangeEndDateTime = AzarLast;

        picker.Clear();

        Assert.Null(picker.RangeStart);
        Assert.Null(picker.RangeEnd);
        Assert.Null(picker.RangeStartDateTime);
        Assert.Null(picker.RangeEndDateTime);
    }

    [Fact]
    public void ANullRange_ClearsTheDateTimes_AndBack()
    {
        var picker = PersianRangePicker();
        picker.RangeStartDateTime = AzarFirst;
        picker.RangeEndDateTime = AzarLast;

        picker.RangeStart = null;
        picker.RangeEndDateTime = null;

        Assert.Null(picker.RangeStartDateTime);
        Assert.Null(picker.RangeEnd);
    }

    [Fact]
    public void AnEmptyBox_ClearsTheRange()
    {
        var picker = PersianRangePicker();
        picker.RangeStartDateTime = AzarFirst;
        picker.RangeEndDateTime = AzarLast;

        picker.CommitText("   ");

        Assert.Null(picker.RangeStartDateTime);
        Assert.Null(picker.RangeEndDateTime);
        Assert.False(picker.HasInvalidInput);
    }

    // ──────────────────────────────────────────────────────
    // Typed range text
    // ──────────────────────────────────────────────────────

    [Theory]
    [InlineData("1405/09/01 ~ 1405/09/30")]
    [InlineData("1405/9/1~1405/9/30")]
    [InlineData("۱۴۰۵/۰۹/۰۱ ~ ۱۴۰۵/۰۹/۳۰")]
    [InlineData("1405-09-01 ~ 1405-09-30")]
    public void AValidTypedRange_SetsTheRange_AndTheDateTimes(string text)
    {
        var picker = PersianRangePicker();

        picker.CommitText(text);

        AssertDate(picker.RangeStart, 1405, 9, 1);
        AssertDate(picker.RangeEnd, 1405, 9, 30);
        Assert.Equal(AzarFirst, picker.RangeStartDateTime);
        Assert.Equal(AzarLast, picker.RangeEndDateTime);
        Assert.False(picker.HasInvalidInput);
        Assert.Null(picker.SelectedDate);
    }

    [Fact]
    public void AValidTypedRange_RaisesDateRangeSelected()
    {
        var picker = PersianRangePicker();
        DateRangeSelectedEventArgs? raised = null;
        picker.DateRangeSelected += (_, e) => raised = e;

        picker.CommitText("1405/09/01 ~ 1405/09/30");

        Assert.NotNull(raised);
        AssertDate(raised!.RangeStart, 1405, 9, 1);
        AssertDate(raised.RangeEnd, 1405, 9, 30);
    }

    [Fact]
    public void ATypedRangeWithTimes_SetsTheRangeTimes()
    {
        var picker = PersianRangePicker();
        picker.IsTimePickerEnabled = true;

        picker.CommitText("1405/09/01 ~ 1405/09/30 08:30:00 ~ 17:45");

        AssertDate(picker.RangeStart, 1405, 9, 1);
        AssertDate(picker.RangeEnd, 1405, 9, 30);
        Assert.Equal(new TimeSpan(8, 30, 0), picker.RangeStartTime);
        Assert.Equal(new TimeSpan(17, 45, 0), picker.RangeEndTime);
        // The DateTime values stay the date part.
        Assert.Equal(AzarFirst, picker.RangeStartDateTime);
        Assert.False(picker.HasInvalidInput);
    }

    [Theory]
    [InlineData("1405/13/01 ~ 1405/09/30")]   // no month 13
    [InlineData("1405/09/01 ~ 1405/09/31")]   // Azar has 30 days
    [InlineData("1405/09/01")]                // only one end
    [InlineData("1405/09/30 ~ 1405/09/01")]   // end before start
    [InlineData("1405/09/01 ~ 1405/09/30 ~ 1405/10/01")]
    [InlineData("from 1405/09/01 ~ 1405/09/30")]
    [InlineData("1405/09/01 ~ 1405/09/30 08:00 ~ 17:00")] // times without IsTimePickerEnabled
    public void Keep_AnInvalidTypedRange_KeepsTheOldRange_AndReportsIt(string text)
    {
        var picker = PersianRangePicker();
        picker.RangeStartDateTime = AzarFirst;
        picker.RangeEndDateTime = AzarLast;

        picker.CommitText(text);

        Assert.True(picker.HasInvalidInput);
        Assert.Equal(AzarFirst, picker.RangeStartDateTime);
        Assert.Equal(AzarLast, picker.RangeEndDateTime);
    }

    [Fact]
    public void Keep_AnInvalidTypedRange_IntoAnEmptyField_IsNotReplacedByToday()
    {
        var picker = PersianRangePicker();

        picker.CommitText("not a range");

        Assert.True(picker.HasInvalidInput);
        Assert.Null(picker.RangeStart);
        Assert.Null(picker.RangeEnd);
        Assert.Null(picker.RangeStartDateTime);
        Assert.Null(picker.RangeEndDateTime);
    }

    [Fact]
    public void Keep_AnOutOfRangeTypedRange_IsReported()
    {
        var picker = PersianRangePicker();
        picker.MinDate = new PersianDate(1405, 9, 5, 0);

        picker.CommitText("1405/09/01 ~ 1405/09/30");

        Assert.True(picker.HasInvalidInput);
        Assert.Null(picker.RangeStartDateTime);
    }

    [Fact]
    public void Keep_AValidEntry_ClearsTheReport_EvenWhenTheRangeDidNotChange()
    {
        var picker = PersianRangePicker();
        picker.RangeStartDateTime = AzarFirst;
        picker.RangeEndDateTime = AzarLast;
        picker.CommitText("garbage");
        Assert.True(picker.HasInvalidInput);

        picker.CommitText("1405/09/01 ~ 1405/09/30");

        Assert.False(picker.HasInvalidInput);
    }

    [Fact]
    public void Keep_ARangeSetFromOutside_ClearsTheReport()
    {
        var picker = PersianRangePicker();
        picker.CommitText("garbage");
        Assert.True(picker.HasInvalidInput);

        picker.RangeStartDateTime = AzarFirst;

        Assert.False(picker.HasInvalidInput);
    }

    [Fact]
    public void Clear_ResetsTheReport()
    {
        var picker = PersianRangePicker();
        picker.CommitText("garbage");

        picker.Clear();

        Assert.False(picker.HasInvalidInput);
    }

    [Fact]
    public void SetToNull_AnInvalidTypedRange_ClearsTheRange()
    {
        var picker = PersianRangePicker(InvalidValueAction.SetToNull);
        picker.RangeStartDateTime = AzarFirst;
        picker.RangeEndDateTime = AzarLast;

        picker.CommitText("garbage");

        Assert.Null(picker.RangeStartDateTime);
        Assert.Null(picker.RangeEndDateTime);
        Assert.False(picker.HasInvalidInput);
    }

    [Fact]
    public void SingleDateMode_TypedText_StillSetsTheSelectedDate_NotTheRange()
    {
        var picker = new ModernPersianDatePicker { CalendarType = CalendarType.Persian, InvalidValueAction = InvalidValueAction.Keep };

        picker.CommitText("1405/09/01");

        AssertDate(picker.SelectedDate, 1405, 9, 1);
        Assert.Null(picker.RangeStart);
        Assert.Null(picker.RangeStartDateTime);
    }

    [Fact]
    public void SingleDateMode_ARangeChange_DoesNotResetTheSingleDateReport()
    {
        var picker = new ModernPersianDatePicker { CalendarType = CalendarType.Persian, InvalidValueAction = InvalidValueAction.Keep };
        picker.CommitText("garbage");
        Assert.True(picker.HasInvalidInput);

        picker.RangeStartDateTime = AzarFirst;

        Assert.True(picker.HasInvalidInput);
    }
}
