using ModernPersianDatePicker.Localizations;
using Xunit;

namespace ModernPersianDatePicker.Tests;

public class LocalizationTests
{
    [Fact]
    public void FarsiLocalization_HasCorrectArrayLengths()
    {
        var loc = new FarsiLocalization();
        Assert.Equal(13, loc.MonthNames.Length);
        Assert.Equal(7, loc.DayNames.Length);
        Assert.Equal(7, loc.ShortDayNames.Length);
        Assert.False(string.IsNullOrEmpty(loc.TodayLabel));
    }

    [Fact]
    public void EnglishLocalization_HasCorrectArrayLengths()
    {
        var loc = new EnglishLocalization();
        Assert.Equal(13, loc.MonthNames.Length);
        Assert.Equal(7, loc.DayNames.Length);
        Assert.Equal(7, loc.ShortDayNames.Length);
        Assert.Equal("Today", loc.TodayLabel);
    }

    [Fact]
    public void ArabicLocalization_HasCorrectArrayLengths()
    {
        var loc = new ArabicLocalization();
        Assert.Equal(13, loc.MonthNames.Length);
        Assert.Equal(7, loc.DayNames.Length);
        Assert.Equal(7, loc.ShortDayNames.Length);
        Assert.False(string.IsNullOrEmpty(loc.TodayLabel));
    }

    [Fact]
    public void KurdishLocalization_HasCorrectArrayLengths()
    {
        var loc = new KurdishLocalization();
        Assert.Equal(13, loc.MonthNames.Length);
        Assert.Equal(7, loc.DayNames.Length);
        Assert.Equal(7, loc.ShortDayNames.Length);
        Assert.False(string.IsNullOrEmpty(loc.TodayLabel));
    }

    [Fact]
    public void Language_DefaultIsFarsi()
    {
        var picker = new ModernPersianDatePicker();
        Assert.Equal(CalendarLanguage.Farsi, picker.Language);
    }

    [Fact]
    public void LocalizationProvider_DefaultIsNull()
    {
        var picker = new ModernPersianDatePicker();
        Assert.Null(picker.LocalizationProvider);
    }

    [Fact]
    public void Language_CanBeChanged()
    {
        var picker = new ModernPersianDatePicker { Language = CalendarLanguage.English };
        Assert.Equal(CalendarLanguage.English, picker.Language);
    }

    [Fact]
    public void LocalizationProvider_OverridesLanguage()
    {
        var custom = new EnglishLocalization();
        var picker = new ModernPersianDatePicker
        {
            Language = CalendarLanguage.Farsi,
            LocalizationProvider = custom
        };
        Assert.Same(custom, picker.LocalizationProvider);
    }

    [Fact]
    public void CalendarView_LocalizationProvider_CanBeSet()
    {
        var view = new CalendarView();
        var loc = new EnglishLocalization();
        view.LocalizationProvider = loc;
        Assert.Same(loc, view.LocalizationProvider);
    }

    [Fact]
    public void EnglishMonthNames_AreCorrect()
    {
        var loc = new EnglishLocalization();
        Assert.Equal("Farvardin", loc.MonthNames[1]);
        Assert.Equal("Esfand", loc.MonthNames[12]);
    }

    [Fact]
    public void EnglishDayNames_AreCorrect()
    {
        var loc = new EnglishLocalization();
        Assert.Equal("Shanbeh", loc.DayNames[0]);
        Assert.Equal("Jomeh", loc.DayNames[6]);
    }

    [Fact]
    public void AllImplementations_ImplementInterface()
    {
        ICalendarLocalization farsi = new FarsiLocalization();
        ICalendarLocalization english = new EnglishLocalization();
        ICalendarLocalization arabic = new ArabicLocalization();
        ICalendarLocalization kurdish = new KurdishLocalization();

        Assert.NotNull(farsi.MonthNames);
        Assert.NotNull(english.MonthNames);
        Assert.NotNull(arabic.MonthNames);
        Assert.NotNull(kurdish.MonthNames);
    }
}
