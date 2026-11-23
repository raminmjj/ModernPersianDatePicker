using ModernPersianDatePicker.Localizations;
using Xunit;

namespace ModernPersianDatePicker.Tests;

/// <summary>
/// The labels added for the redesigned Quick Range panel (2.6.12) are default interface
/// members: an implementation written before them still compiles and gets English, and
/// every built-in language translates them.
/// </summary>
public class QuickRangeLocalizationTests
{
    /// <summary>A custom localization written against 2.5 - none of the new members.</summary>
    private sealed class OldStyleLocalization : ICalendarLocalization
    {
        public string[] MonthNames { get; } = new string[13];
        public string[] DayNames { get; } = new string[7];
        public string[] ShortDayNames { get; } = new string[7];
        public string TodayLabel => "T";
        public string QuickRangeLabel => "Q";
        public string ApplyLabel => "A";
        public string CurrentWeekLabel => "W";
        public string CurrentMonthLabel => "M";
        public string CurrentYearLabel => "Y";
        public string MonthLabel => "m";
        public string SeasonLabel => "s";
        public string HalfYearLabel => "h";
        public string BackLabel => "b";
        public string FromLabel => "f";
        public string ToLabel => "t";
        public string SelectDateLabel => "d";
        public string GoToTodayLabel => "g";
        public string[] SeasonNames { get; } = { "1", "2", "3", "4" };
    }

    [Fact]
    public void AnOldImplementation_GetsEnglishDefaults()
    {
        ICalendarLocalization loc = new OldStyleLocalization();

        Assert.Equal("Yesterday", loc.YesterdayLabel);
        Assert.Equal("Last 7 Days", loc.Last7DaysLabel);
        Assert.Equal("Last Month", loc.LastMonthLabel);
        Assert.Equal("Last 30 Days", loc.Last30DaysLabel);
        Assert.Equal("Presets", loc.PresetsLabel);
        Assert.Equal("Cancel", loc.CancelLabel);
        Assert.Equal("Clear", loc.ClearLabel);
        Assert.Equal(new[] { "First Half", "Second Half" }, loc.HalfYearNames);
        Assert.Equal("1 day", loc.FormatDayCount(1));
        Assert.Equal("30 days", loc.FormatDayCount(30));
    }

    public static TheoryData<ICalendarLocalization> Translated => new()
    {
        new FarsiLocalization(),
        new ArabicLocalization(),
        new KurdishLocalization(),
    };

    [Theory]
    [MemberData(nameof(Translated))]
    public void EveryBuiltInLanguage_TranslatesTheNewLabels(ICalendarLocalization loc)
    {
        ICalendarLocalization english = new EnglishLocalization();

        Assert.NotEqual(english.YesterdayLabel, loc.YesterdayLabel);
        Assert.NotEqual(english.Last7DaysLabel, loc.Last7DaysLabel);
        Assert.NotEqual(english.LastMonthLabel, loc.LastMonthLabel);
        Assert.NotEqual(english.Last30DaysLabel, loc.Last30DaysLabel);
        Assert.NotEqual(english.PresetsLabel, loc.PresetsLabel);
        Assert.NotEqual(english.CancelLabel, loc.CancelLabel);
        Assert.NotEqual(english.ClearLabel, loc.ClearLabel);
        Assert.Equal(2, loc.HalfYearNames.Length);
        Assert.NotEqual(english.HalfYearNames[0], loc.HalfYearNames[0]);
        Assert.NotEqual(english.FormatDayCount(30), loc.FormatDayCount(30));
    }

    [Fact]
    public void Farsi_DayCount()
        => Assert.Equal("30 روز", new FarsiLocalization().FormatDayCount(30));

    [Theory]
    [InlineData(1, "يوم واحد")]
    [InlineData(2, "يومان")]
    [InlineData(7, "7 أيام")]
    [InlineData(30, "30 يومًا")]
    public void Arabic_DayCount_UsesTheRightPluralForm(int days, string expected)
        => Assert.Equal(expected, ((ICalendarLocalization)new ArabicLocalization()).FormatDayCount(days));
}
