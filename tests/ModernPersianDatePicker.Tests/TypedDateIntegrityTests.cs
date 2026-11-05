using System;
using System.Globalization;
using Xunit;

namespace ModernPersianDatePicker.Tests;

/// <summary>
/// A typed date is never silently replaced: an invalid entry under
/// <see cref="InvalidValueAction.Keep"/> is reported (<see cref="ModernPersianDatePicker.HasInvalidInput"/>),
/// an empty box clears the date, Persian / Arabic-Indic digits parse, and the
/// calendar-neutral <see cref="ModernPersianDatePicker.SelectedDateTime"/> keeps the same
/// day across a calendar or culture switch.
/// </summary>
public class TypedDateIntegrityTests
{
    private static void AssertDate(PersianDate? actual, int year, int month, int day)
    {
        Assert.True(actual.HasValue, "Expected a date, got null.");
        Assert.Equal((year, month, day), (actual!.Value.Year, actual.Value.Month, actual.Value.Day));
    }

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

    // ──────────────────────────────────────────────────────
    // Persian / Arabic-Indic digits
    // ──────────────────────────────────────────────────────

    [Theory]
    [InlineData("۱۴۰۴/۰۱/۱۵", "1404/01/15")]   // Persian digits
    [InlineData("١٤٠٤/٠١/١٥", "1404/01/15")]   // Arabic-Indic digits
    [InlineData("1404/01/15", "1404/01/15")]
    public void NormalizeDigits_MapsNativeDigitsToAscii(string input, string expected)
        => Assert.Equal(expected, ModernPersianDatePicker.NormalizeDigits(input));

    [Fact]
    public void ADateTypedInPersianDigits_Parses()
    {
        var picker = new ModernPersianDatePicker { CalendarType = CalendarType.Persian, InvalidValueAction = InvalidValueAction.Keep };

        picker.CommitText("۱۴۰۴/۰۱/۱۵");

        AssertDate(picker.SelectedDate, 1404, 1, 15);
        Assert.False(picker.HasInvalidInput);
    }

    // ──────────────────────────────────────────────────────
    // Keep: an invalid entry is reported, never silently replaced
    // ──────────────────────────────────────────────────────

    [Fact]
    public void Keep_AnInvalidEntry_KeepsTheOldDate_AndReportsIt()
    {
        var picker = new ModernPersianDatePicker
        {
            CalendarType = CalendarType.Persian,
            InvalidValueAction = InvalidValueAction.Keep,
            SelectedDate = new PersianDate(1404, 1, 1, 0),
        };

        picker.CommitText("1404/13/45");

        AssertDate(picker.SelectedDate, 1404, 1, 1);
        Assert.True(picker.HasInvalidInput);
    }

    [Fact]
    public void Keep_AValidEntry_ClearsTheReport_EvenWhenTheDateDidNotChange()
    {
        var picker = new ModernPersianDatePicker { CalendarType = CalendarType.Persian, InvalidValueAction = InvalidValueAction.Keep };

        picker.CommitText("1404/02/02");
        picker.CommitText("nope");
        Assert.True(picker.HasInvalidInput);

        // The same date again raises no SelectedDate change - the report must still clear.
        picker.CommitText("1404/02/02");

        AssertDate(picker.SelectedDate, 1404, 2, 2);
        Assert.False(picker.HasInvalidInput);
    }

    [Fact]
    public void Keep_ADateOutsideMinMax_IsReported()
    {
        var picker = new ModernPersianDatePicker
        {
            CalendarType = CalendarType.Persian,
            InvalidValueAction = InvalidValueAction.Keep,
            MinDate = new PersianDate(1404, 1, 1, 0),
        };

        picker.CommitText("1403/12/01");

        Assert.Null(picker.SelectedDate);
        Assert.True(picker.HasInvalidInput);
    }

    [Fact]
    public void SettingTheDateFromOutside_ClearsTheReport()
    {
        var picker = new ModernPersianDatePicker { CalendarType = CalendarType.Persian, InvalidValueAction = InvalidValueAction.Keep };
        picker.CommitText("nope");
        Assert.True(picker.HasInvalidInput);

        picker.SelectedDate = new PersianDate(1404, 3, 3, 0);

        Assert.False(picker.HasInvalidInput);
    }

    [Theory]
    [InlineData(InvalidValueAction.SetToNull)]
    [InlineData(InvalidValueAction.SetToToday)]
    public void TheCorrectingActions_NeverReportAnInvalidEntry(InvalidValueAction action)
    {
        var picker = new ModernPersianDatePicker { CalendarType = CalendarType.Persian, InvalidValueAction = action };

        picker.CommitText("nope");

        Assert.False(picker.HasInvalidInput);
    }

    // ──────────────────────────────────────────────────────
    // An empty box
    // ──────────────────────────────────────────────────────

    [Theory]
    [InlineData(InvalidValueAction.Keep)]
    [InlineData(InvalidValueAction.SetToNull)]
    public void AnEmptyBox_ClearsTheDate(InvalidValueAction action)
    {
        var picker = new ModernPersianDatePicker
        {
            CalendarType = CalendarType.Persian,
            InvalidValueAction = action,
            SelectedDate = new PersianDate(1404, 1, 1, 0),
        };

        picker.CommitText("  ");

        Assert.Null(picker.SelectedDate);
        Assert.Null(picker.SelectedDateTime);
        Assert.False(picker.HasInvalidInput);
    }

    [Fact]
    public void AnEmptyBox_UnderSetToToday_IsToday()
    {
        var picker = new ModernPersianDatePicker { CalendarType = CalendarType.Persian, InvalidValueAction = InvalidValueAction.SetToToday };

        picker.CommitText("");

        Assert.Equal(DateTime.Today, picker.SelectedDateTime);
    }

    // ──────────────────────────────────────────────────────
    // SelectedDateTime: calendar-neutral, in sync both ways
    // ──────────────────────────────────────────────────────

    [Fact]
    public void SelectedDateTime_FollowsSelectedDate_InThePersianCalendar()
    {
        var picker = new ModernPersianDatePicker { CalendarType = CalendarType.Persian };

        picker.SelectedDate = new PersianDate(1404, 1, 1, 0);   // Nowruz

        Assert.Equal(new DateTime(2025, 3, 21), picker.SelectedDateTime);
    }

    [Fact]
    public void SelectedDate_FollowsSelectedDateTime_InEitherCalendar()
    {
        var persian = new ModernPersianDatePicker { CalendarType = CalendarType.Persian };
        var gregorian = new ModernPersianDatePicker { CalendarType = CalendarType.Gregorian };

        persian.SelectedDateTime = new DateTime(2025, 7, 31, 14, 30, 0);
        gregorian.SelectedDateTime = new DateTime(2025, 7, 31);

        AssertDate(persian.SelectedDate, 1404, 5, 9);
        AssertDate(gregorian.SelectedDate, 2025, 7, 31);

        persian.SelectedDateTime = null;
        Assert.Null(persian.SelectedDate);
    }

    [Fact]
    public void SwitchingTheCalendar_KeepsTheSameDay()
    {
        var picker = new ModernPersianDatePicker { CalendarType = CalendarType.Persian };
        picker.SelectedDateTime = new DateTime(2025, 7, 31);

        picker.CalendarType = CalendarType.Gregorian;
        AssertDate(picker.SelectedDate, 2025, 7, 31);
        Assert.Equal(new DateTime(2025, 7, 31), picker.SelectedDateTime);

        picker.CalendarType = CalendarType.Persian;
        AssertDate(picker.SelectedDate, 1404, 5, 9);
        Assert.Equal(new DateTime(2025, 7, 31), picker.SelectedDateTime);
    }

    // ──────────────────────────────────────────────────────
    // Auto mode and a culture switch
    // ──────────────────────────────────────────────────────

    [Fact]
    public void Auto_ACultureSwitch_ConvertsTheHeldDate_OnRefresh()
    {
        var picker = WithCulture("fa-IR", () =>
        {
            var p = new ModernPersianDatePicker { CalendarType = CalendarType.Auto };
            p.SelectedDateTime = new DateTime(2025, 3, 21);
            return p;
        });
        AssertDate(picker.SelectedDate, 1404, 1, 1);

        WithCulture("en-US", () =>
        {
            picker.RefreshCalendar();
            return 0;
        });

        // The same day, now in the Gregorian calendar - not "the year 1404".
        AssertDate(picker.SelectedDate, 2025, 3, 21);
        Assert.Equal(new DateTime(2025, 3, 21), picker.SelectedDateTime);
    }

    [Fact]
    public void Auto_ARefreshWithoutACultureChange_ChangesNothing()
    {
        WithCulture("fa-IR", () =>
        {
            var picker = new ModernPersianDatePicker { CalendarType = CalendarType.Auto };
            picker.SelectedDate = new PersianDate(1404, 1, 1, 0);

            picker.RefreshCalendar();

            AssertDate(picker.SelectedDate, 1404, 1, 1);
            return 0;
        });
    }

    [Fact]
    public void AnExplicitCalendar_AfterACultureSwitch_ConvertsFromTheCalendarTheValuesAreIn()
    {
        // Created under Persian (Auto), then the culture moved to English with no
        // refresh. Setting CalendarType=Persian must not convert: the value already IS
        // Persian. (The old code guessed the previous calendar from the CURRENT culture
        // - Gregorian - and turned 1404/01/01 into a Persian date of the year 782.)
        var picker = WithCulture("fa-IR", () =>
        {
            var p = new ModernPersianDatePicker { CalendarType = CalendarType.Auto };
            p.SelectedDate = new PersianDate(1404, 1, 1, 0);
            return p;
        });

        WithCulture("en-US", () =>
        {
            picker.CalendarType = CalendarType.Persian;
            return 0;
        });

        AssertDate(picker.SelectedDate, 1404, 1, 1);
        Assert.Equal(new DateTime(2025, 3, 21), picker.SelectedDateTime);
    }

    private sealed class FormState
    {
        public bool DateIsInvalid { get; set; }
    }

    [Fact]
    public void HasInvalidInput_CanBeBoundOneWayToSource()
    {
        // The README's pattern: the read-only property pushed into the view model.
        var state = new FormState();
        var picker = new ModernPersianDatePicker
        {
            CalendarType = CalendarType.Persian,
            InvalidValueAction = InvalidValueAction.Keep,
            DataContext = state,
        };
        picker.Bind(ModernPersianDatePicker.HasInvalidInputProperty,
            new Avalonia.Data.Binding(nameof(FormState.DateIsInvalid)) { Mode = Avalonia.Data.BindingMode.OneWayToSource });

        picker.CommitText("nope");
        Assert.True(state.DateIsInvalid);

        picker.CommitText("1404/01/01");
        Assert.False(state.DateIsInvalid);
    }

    [Fact]
    public void NotifyCultureChanged_WithNoLoadedPicker_DoesNothing()
        => ModernPersianDatePicker.NotifyCultureChanged();
}
