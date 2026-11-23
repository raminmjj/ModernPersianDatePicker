using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ModernPersianDatePicker.Localizations;
using Xunit;

namespace ModernPersianDatePicker.Tests.Headless;

/// <summary>
/// The redesigned Quick Range panel: presets and Month / Season / Half-year periods fill
/// From / To (a draft), Apply commits it, Cancel drops it, Clear empties it; the grid
/// has a year stepper; whatever matches the draft is highlighted and summarised.
/// </summary>
[Collection(HeadlessCollection.Name)]
public sealed class QuickRangeViewTests
{
    private readonly HeadlessSession _session;

    public QuickRangeViewTests(HeadlessSession session) => _session = session;

    private static PersianDate Persian(DateTime d) => PersianCalendarHelper.ToPersianDate(d);

    private static (int, int, int) Ymd(PersianDate? d)
    {
        Assert.True(d.HasValue, "Expected a date, got null.");
        return (d!.Value.Year, d.Value.Month, d.Value.Day);
    }

    private static (int, int, int) Ymd(PersianDate d) => (d.Year, d.Month, d.Day);

    private static QuickRangeView PersianView(PersianDate? start = null, PersianDate? end = null)
        => new()
        {
            CalendarType = CalendarType.Persian,
            LocalizationProvider = new FarsiLocalization(),
            RangeStart = start,
            RangeEnd = end,
        };

    /// <summary>Shows the view in a window, runs <paramref name="work"/>, closes the window.</summary>
    private T With<T>(Func<QuickRangeView> create, Func<QuickRangeView, T> work)
        => _session.Ui(() =>
        {
            var view = create();
            var window = new Window { Width = 700, Height = 500, Content = view };
            window.Show();
            Dispatcher.UIThread.RunJobs();
            try
            {
                return work(view);
            }
            finally
            {
                window.Close();
            }
        });

    private static void Click(Button button)
    {
        button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();
    }

    private static Button Part(QuickRangeView view, string name)
        => view.GetVisualDescendants().OfType<Button>().First(b => b.Name == name);

    private static bool Selected(Button b) => b.Classes.Contains("selected");

    // ──────────────────────────────────────────────────────
    // Presets
    // ──────────────────────────────────────────────────────

    [Fact]
    public void ThePresetList_HasEightPresets_InOrder()
    {
        var keys = With(() => PersianView(), v => v.PresetButtons.Select(b => (string)b.Tag!).ToList());

        Assert.Equal(new[] { "today", "yesterday", "week", "last7days", "month", "lastmonth", "last30days", "year" }, keys);
    }

    [Fact]
    public void APresetClick_FillsFromAndTo_ButDoesNotApply()
    {
        var result = With(() => PersianView(), v =>
        {
            int applied = 0;
            v.QuickRangeApplied += (_, _) => applied++;
            Click(v.PresetButtons.First(b => (string)b.Tag! == "last7days"));
            return (applied, v.DraftStart, v.DraftEnd);
        });

        Assert.Equal(0, result.applied);
        Assert.Equal(Ymd(Persian(DateTime.Today.AddDays(-6))), Ymd(result.DraftStart));
        Assert.Equal(Ymd(Persian(DateTime.Today)), Ymd(result.DraftEnd));
    }

    [Theory]
    [InlineData("today", 0, 0)]
    [InlineData("yesterday", -1, -1)]
    [InlineData("last7days", -6, 0)]
    [InlineData("last30days", -29, 0)]
    public void RelativePresets_AreCountedFromToday(string key, int fromOffset, int toOffset)
    {
        var result = With(() => PersianView(), v =>
        {
            Assert.True(v.SelectPreset(key));
            return (v.DraftStart, v.DraftEnd);
        });

        Assert.Equal(Ymd(Persian(DateTime.Today.AddDays(fromOffset))), Ymd(result.DraftStart));
        Assert.Equal(Ymd(Persian(DateTime.Today.AddDays(toOffset))), Ymd(result.DraftEnd));
    }

    [Fact]
    public void LastMonth_IsThePreviousWholeMonth()
    {
        var today = Persian(DateTime.Today);
        int year = today.Month == 1 ? today.Year - 1 : today.Year;
        int month = today.Month == 1 ? 12 : today.Month - 1;

        var result = With(() => PersianView(), v =>
        {
            v.SelectPreset("lastmonth");
            return (v.DraftStart, v.DraftEnd);
        });

        Assert.Equal((year, month, 1), Ymd(result.DraftStart));
        Assert.Equal((year, month, PersianCalendarHelper.GetDaysInMonth(year, month)), Ymd(result.DraftEnd));
    }

    [Fact]
    public void Gregorian_TheWeekRunsSundayToSaturday()
    {
        var result = With(() => new QuickRangeView { CalendarType = CalendarType.Gregorian }, v =>
        {
            v.SelectPreset("week");
            return (v.DraftStart, v.DraftEnd);
        });

        var start = new DateTime(result.DraftStart!.Value.Year, result.DraftStart.Value.Month, result.DraftStart.Value.Day);
        var end = new DateTime(result.DraftEnd!.Value.Year, result.DraftEnd.Value.Month, result.DraftEnd.Value.Day);
        Assert.Equal(DayOfWeek.Sunday, start.DayOfWeek);
        Assert.Equal(6, (end - start).Days);
        Assert.InRange(DateTime.Today, start, end);
    }

    // ──────────────────────────────────────────────────────
    // Apply / Cancel / Clear
    // ──────────────────────────────────────────────────────

    [Fact]
    public void Apply_RaisesTheRange_WithItsType()
    {
        var args = With(() => PersianView(), v =>
        {
            QuickRangeSelectedEventArgs? raised = null;
            v.QuickRangeApplied += (_, e) => raised = e;
            v.SelectPreset("month");
            Click(Part(v, "PART_ApplyButton"));
            return raised;
        });

        var today = Persian(DateTime.Today);
        Assert.NotNull(args);
        Assert.Equal("month", args!.RangeType);
        Assert.Equal((today.Year, today.Month, 1), Ymd(args.RangeStart));
        Assert.Equal(DateRangeSelectedEventArgs.DefaultStartTime, args.StartTime);
        Assert.Equal(DateRangeSelectedEventArgs.DefaultEndTime, args.EndTime);
    }

    [Fact]
    public void Apply_WithOnlyOneEnd_DoesNothing()
    {
        var applied = With(() => PersianView(new PersianDate(1405, 9, 1, 0)), v =>
        {
            int count = 0;
            v.QuickRangeApplied += (_, _) => count++;
            v.Apply();
            return count;
        });

        Assert.Equal(0, applied);
    }

    [Fact]
    public void Cancel_DropsTheDraft_AndRaisesCancelled()
    {
        var result = With(() => PersianView(new PersianDate(1405, 9, 1, 0), new PersianDate(1405, 9, 30, 0)), v =>
        {
            int cancelled = 0, applied = 0;
            v.QuickRangeCancelled += (_, _) => cancelled++;
            v.QuickRangeApplied += (_, _) => applied++;
            v.SelectPreset("today");
            Click(Part(v, "PART_CancelButton"));
            return (cancelled, applied, v.DraftStart, v.DraftEnd);
        });

        Assert.Equal(1, result.cancelled);
        Assert.Equal(0, result.applied);
        Assert.Equal((1405, 9, 1), Ymd(result.DraftStart));
        Assert.Equal((1405, 9, 30), Ymd(result.DraftEnd));
    }

    [Fact]
    public void Clear_EmptiesTheDraft_AndRaisesCleared()
    {
        var result = With(() => PersianView(new PersianDate(1405, 9, 1, 0), new PersianDate(1405, 9, 30, 0)), v =>
        {
            int cleared = 0;
            v.QuickRangeCleared += (_, _) => cleared++;
            Click(Part(v, "PART_ClearRangeButton"));
            return (cleared, v.DraftStart, v.DraftEnd, v.SummaryText);
        });

        Assert.Equal(1, result.cleared);
        Assert.Null(result.DraftStart);
        Assert.Null(result.DraftEnd);
        Assert.Equal(string.Empty, result.SummaryText);
    }

    // ──────────────────────────────────────────────────────
    // The period grid
    // ──────────────────────────────────────────────────────

    [Fact]
    public void TheGrid_OpensOnTheMonthTab_OfTheRangesYear()
    {
        var result = With(() => PersianView(new PersianDate(1403, 5, 1, 0), new PersianDate(1403, 5, 31, 0)), v =>
            (v.CurrentPeriodKind, v.DisplayYear, v.PeriodButtons.Count,
             selected: v.PeriodButtons.Select((b, i) => (b, i)).Where(t => Selected(t.b)).Select(t => t.i).ToList()));

        Assert.Equal(QuickRangeView.PeriodKind.Month, result.CurrentPeriodKind);
        Assert.Equal(1403, result.DisplayYear);
        Assert.Equal(12, result.Count);
        Assert.Equal(new[] { 4 }, result.selected);   // Mordad, the 5th month
    }

    [Fact]
    public void ASeasonRange_OpensTheSeasonTab()
    {
        var result = With(() => PersianView(new PersianDate(1404, 7, 1, 0), new PersianDate(1404, 9, 30, 0)), v =>
            (v.CurrentPeriodKind, v.PeriodButtons.Count, v.CurrentRangeType,
             selected: v.PeriodButtons.FindIndex(Selected)));

        Assert.Equal(QuickRangeView.PeriodKind.Season, result.CurrentPeriodKind);
        Assert.Equal(4, result.Count);
        Assert.Equal("season", result.CurrentRangeType);
        Assert.Equal(2, result.selected);   // autumn
    }

    [Fact]
    public void TheYearStepper_MovesTheGrid_AndAPeriodClickUsesThatYear()
    {
        var result = With(() => PersianView(), v =>
        {
            int year = v.DisplayYear;
            Click(Part(v, "PART_YearPreviousButton"));
            Click(Part(v, "PART_YearPreviousButton"));
            Click(Part(v, "PART_YearNextButton"));
            int shown = v.DisplayYear;
            Click(v.PeriodButtons[11]);   // Esfand
            return (year, shown, v.DraftStart, v.DraftEnd);
        });

        Assert.Equal(result.year - 1, result.shown);
        Assert.Equal((result.shown, 12, 1), Ymd(result.DraftStart));
        Assert.Equal((result.shown, 12, PersianCalendarHelper.GetDaysInMonth(result.shown, 12)), Ymd(result.DraftEnd));
    }

    [Fact]
    public void TheHalfYearTab_HasTwoPeriods_WithTheLocalizedNames()
    {
        var result = With(() => PersianView(), v =>
        {
            Click(Part(v, "PART_GroupHalfYearButton"));
            var texts = v.PeriodButtons.Select(b => b.Content as string).ToList();
            Click(v.PeriodButtons[1]);
            return (texts, v.DraftStart, v.DraftEnd, v.CurrentRangeType, tab: Selected(Part(v, "PART_GroupHalfYearButton")));
        });

        Assert.Equal(new FarsiLocalization().HalfYearNames, result.texts);
        Assert.Equal(7, result.DraftStart!.Value.Month);
        Assert.Equal((12, PersianCalendarHelper.GetDaysInMonth(result.DraftEnd!.Value.Year, 12)),
            (result.DraftEnd.Value.Month, result.DraftEnd.Value.Day));
        Assert.Equal("halfyear", result.CurrentRangeType);
        Assert.True(result.tab);
    }

    [Fact]
    public void FuturePeriods_AreMuted()
    {
        var result = With(() => PersianView(), v =>
        {
            Click(Part(v, "PART_YearNextButton"));
            var nextYear = v.PeriodButtons.All(b => b.Classes.Contains("future"));
            Click(Part(v, "PART_YearPreviousButton"));
            Click(Part(v, "PART_YearPreviousButton"));
            var lastYear = v.PeriodButtons.Any(b => b.Classes.Contains("future"));
            return (nextYear, lastYear);
        });

        Assert.True(result.nextYear);
        Assert.False(result.lastYear);
    }

    // ──────────────────────────────────────────────────────
    // Highlight and summary
    // ──────────────────────────────────────────────────────

    [Fact]
    public void TheCurrentMonth_HighlightsThePreset_AndTheGridCell_AndIsSummarised()
    {
        var today = Persian(DateTime.Today);
        var loc = new FarsiLocalization();

        var result = With(() => PersianView(), v =>
        {
            v.SelectPreset("month");
            return (presets: v.PresetButtons.Where(Selected).Select(b => (string)b.Tag!).ToList(),
                cell: v.PeriodButtons.FindIndex(Selected), v.SummaryText, v.CurrentRangeType);
        });

        Assert.Contains("month", result.presets);
        Assert.Equal(today.Month - 1, result.cell);
        int days = PersianCalendarHelper.GetDaysInMonth(today.Year, today.Month);
        Assert.Equal($"{loc.CurrentMonthLabel} · {loc.FormatDayCount(days)}", result.SummaryText);
        Assert.Equal("month", result.CurrentRangeType);
    }

    [Fact]
    public void ACustomRange_HighlightsNothing_AndIsCustom()
    {
        var result = With(() => PersianView(new PersianDate(1403, 2, 3, 0), new PersianDate(1403, 2, 9, 0)), v =>
            (presets: v.PresetButtons.Count(Selected), cells: v.PeriodButtons.Count(Selected), v.SummaryText, v.CurrentRangeType));

        Assert.Equal(0, result.presets);
        Assert.Equal(0, result.cells);
        Assert.Equal(new FarsiLocalization().FormatDayCount(7), result.SummaryText);
        Assert.Equal("custom", result.CurrentRangeType);
    }

    [Fact]
    public void ThePresetsShowTheirLength()
    {
        var counts = With(() => PersianView(), v => v.PresetButtons
            .Select(b => b.GetVisualDescendants().OfType<TextBlock>().First(t => t.Classes.Contains("qr-count")).Text)
            .ToList());

        var loc = new FarsiLocalization();
        Assert.Equal(loc.FormatDayCount(1), counts[0]);
        Assert.Equal(loc.FormatDayCount(7), counts[3]);
        Assert.Equal(loc.FormatDayCount(30), counts[6]);
    }

    // ──────────────────────────────────────────────────────
    // Inside a CalendarView
    // ──────────────────────────────────────────────────────

    [Fact]
    public void InACalendarView_ClearEmptiesTheRange_AndCancelGoesBackToTheCalendar()
    {
        var result = _session.Ui(() =>
        {
            var calendar = new CalendarView
            {
                CalendarType = CalendarType.Persian,
                IsRangeMode = true,
                RangeStart = new PersianDate(1405, 9, 1, 0),
                RangeEnd = new PersianDate(1405, 9, 30, 0),
            };
            var window = new Window { Width = 700, Height = 600, Content = calendar };
            window.Show();
            Dispatcher.UIThread.RunJobs();
            try
            {
                var open = calendar.GetVisualDescendants().OfType<Button>().First(b => b.Name == "PART_QuickRangeButton");
                var panel = calendar.GetVisualDescendants().OfType<QuickRangeView>().First();
                int cleared = 0;
                calendar.QuickRangeCleared += (_, _) => cleared++;

                Click(open);
                var shown = panel.IsVisible;
                panel.Cancel();
                Dispatcher.UIThread.RunJobs();
                var afterCancel = (panel.IsVisible, calendar.RangeStart);

                Click(open);
                panel.ClearDraft();
                Dispatcher.UIThread.RunJobs();
                return (shown, afterCancel, cleared, calendar.RangeStart, calendar.RangeEnd, panelVisible: panel.IsVisible);
            }
            finally
            {
                window.Close();
            }
        });

        Assert.True(result.shown);
        Assert.False(result.afterCancel.IsVisible);
        Assert.Equal((1405, 9, 1), Ymd(result.afterCancel.RangeStart));
        Assert.Equal(1, result.cleared);
        Assert.Null(result.RangeStart);
        Assert.Null(result.RangeEnd);
        Assert.False(result.panelVisible);
    }
}

internal static class ListExtensions
{
    public static int FindIndex(this IReadOnlyList<Button> list, Func<Button, bool> match)
    {
        for (int i = 0; i < list.Count; i++)
            if (match(list[i]))
                return i;
        return -1;
    }
}
