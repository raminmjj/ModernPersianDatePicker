using System;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace ModernPersianDatePicker.Tests.Headless;

/// <summary>
/// A loaded range picker: <see cref="ModernPersianDatePicker.NotifyCultureChanged"/> converts the
/// range and re-renders it while <see cref="ModernPersianDatePicker.RangeStartDateTime"/> /
/// <see cref="ModernPersianDatePicker.RangeEndDateTime"/> keep the same days, and text typed
/// into the box is parsed as a range (invalid text is kept and reported under Keep).
/// </summary>
[Collection(HeadlessCollection.Name)]
public sealed class RangeDateTimeHeadlessTests
{
    private static readonly DateTime AzarFirst = new PersianCalendar().ToDateTime(1405, 9, 1, 0, 0, 0, 0);
    private static readonly DateTime AzarLast = new PersianCalendar().ToDateTime(1405, 9, 30, 0, 0, 0, 0);

    private readonly HeadlessSession _session;

    public RangeDateTimeHeadlessTests(HeadlessSession session) => _session = session;

    private static TextBox EditableText(ModernPersianDatePicker picker)
        => picker.GetVisualDescendants().OfType<TextBox>().First(t => t.Name == "PART_EditableText");

    private static Window Show(Control content)
    {
        var window = new Window { Width = 400, Height = 200, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    [Fact]
    public void NotifyCultureChanged_ConvertsALoadedRange_AndKeepsTheDays()
    {
        var result = _session.Ui(() =>
        {
            var previous = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = new CultureInfo("fa-IR");
            var picker = new ModernPersianDatePicker
            {
                CalendarType = CalendarType.Auto,
                IsRangeMode = true,
                IsEditable = true,
                RangeStartDateTime = AzarFirst,
                RangeEndDateTime = AzarLast,
            };
            var window = Show(picker);
            try
            {
                var persianText = EditableText(picker).Text;

                CultureInfo.CurrentUICulture = new CultureInfo("en-US");
                ModernPersianDatePicker.NotifyCultureChanged();
                Dispatcher.UIThread.RunJobs();

                return (persianText, gregorianText: EditableText(picker).Text,
                    picker.RangeStart, picker.RangeStartDateTime, picker.RangeEndDateTime);
            }
            finally
            {
                window.Close();
                CultureInfo.CurrentUICulture = previous;
            }
        });

        Assert.Equal("1405/09/01 ~ 1405/09/30", result.persianText);
        Assert.Equal(
            AzarFirst.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture) + " ~ "
                + AzarLast.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture),
            result.gregorianText);
        Assert.Equal(AzarFirst.Year, result.RangeStart!.Value.Year);
        Assert.Equal(AzarFirst, result.RangeStartDateTime);
        Assert.Equal(AzarLast, result.RangeEndDateTime);
    }

    [Fact]
    public void TextTypedIntoTheBox_IsParsedAsARange_AndInvalidTextIsKept()
    {
        var result = _session.Ui(() =>
        {
            var picker = new ModernPersianDatePicker
            {
                CalendarType = CalendarType.Persian,
                IsRangeMode = true,
                IsEditable = true,
                InvalidValueAction = InvalidValueAction.Keep,
            };
            var window = Show(picker);
            try
            {
                var box = EditableText(picker);
                box.Text = "1405/09/01 ~ 1405/09/30";
                picker.CommitText();
                var start = picker.RangeStartDateTime;
                var end = picker.RangeEndDateTime;

                box.Text = "1405/09/01 ~";
                picker.CommitText();

                return (start, end, invalid: picker.HasInvalidInput, kept: box.Text,
                    stillStart: picker.RangeStartDateTime);
            }
            finally
            {
                window.Close();
            }
        });

        Assert.Equal(AzarFirst, result.start);
        Assert.Equal(AzarLast, result.end);
        Assert.True(result.invalid);
        Assert.Equal("1405/09/01 ~", result.kept);
        Assert.Equal(AzarFirst, result.stillStart);
    }
}
