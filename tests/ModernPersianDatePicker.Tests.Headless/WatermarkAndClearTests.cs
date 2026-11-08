using System.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace ModernPersianDatePicker.Tests.Headless;

/// <summary>
/// An editable picker shows the application's own <see cref="ModernPersianDatePicker.Watermark"/>
/// (it used to show the localized label always), and <see cref="ModernPersianDatePicker.Clear"/>
/// empties a field holding text that is not a date (it used to keep the text and the flag).
/// </summary>
[Collection(HeadlessCollection.Name)]
public sealed class WatermarkAndClearTests
{
    private readonly HeadlessSession _session;

    public WatermarkAndClearTests(HeadlessSession session) => _session = session;

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
    public void AnEditablePicker_ShowsTheApplicationsWatermark()
    {
        var (custom, changed, localized) = _session.Ui(() =>
        {
            var picker = new ModernPersianDatePicker { IsEditable = true, Watermark = "yyyy/MM/dd" };
            var plain = new ModernPersianDatePicker { IsEditable = true, Language = CalendarLanguage.Farsi };
            var window = Show(new StackPanel { Children = { picker, plain } });
            try
            {
                var first = EditableText(picker).PlaceholderText;
                picker.Watermark = "روز/ماه/سال";
                Dispatcher.UIThread.RunJobs();
                return (first, EditableText(picker).PlaceholderText, EditableText(plain).PlaceholderText);
            }
            finally
            {
                window.Close();
            }
        });

        Assert.Equal("yyyy/MM/dd", custom);
        Assert.Equal("روز/ماه/سال", changed);
        // No watermark of its own: the localized label of the field's language.
        Assert.False(string.IsNullOrEmpty(localized));
        Assert.NotEqual("Select date...", localized);
    }

    [Fact]
    public void AWatermark_SurvivesALanguageChange()
        => Assert.Equal("yyyy/MM/dd", _session.Ui(() =>
        {
            var picker = new ModernPersianDatePicker { IsEditable = true, Watermark = "yyyy/MM/dd" };
            var window = Show(picker);
            try
            {
                picker.Language = CalendarLanguage.Farsi;
                picker.Language = CalendarLanguage.English;
                Dispatcher.UIThread.RunJobs();
                return EditableText(picker).PlaceholderText;
            }
            finally
            {
                window.Close();
            }
        }));

    [Fact]
    public void Clear_EmptiesAFieldHoldingInvalidText_WithNoDate()
    {
        var (invalidBefore, text, invalidAfter, date) = _session.Ui(() =>
        {
            var picker = new ModernPersianDatePicker { IsEditable = true, InvalidValueAction = InvalidValueAction.Keep };
            var window = Show(picker);
            try
            {
                EditableText(picker).Text = "not-a-date";
                picker.CommitText();
                var before = picker.HasInvalidInput;
                picker.Clear();
                Dispatcher.UIThread.RunJobs();
                return (before, EditableText(picker).Text, picker.HasInvalidInput, picker.SelectedDate);
            }
            finally
            {
                window.Close();
            }
        });

        Assert.True(invalidBefore);
        Assert.True(string.IsNullOrEmpty(text));
        Assert.False(invalidAfter);
        Assert.Null(date);
    }
}
