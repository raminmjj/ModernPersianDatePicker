using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace ModernPersianDatePicker.Tests.Headless;

/// <summary>
/// <see cref="ModernPersianDatePicker.ShowClearButton"/>: the x button shows only while the
/// field holds a date, a range or typed text, and clicking it (or Ctrl+Delete) clears the
/// field as <see cref="ModernPersianDatePicker.Clear"/> does.
/// </summary>
[Collection(HeadlessCollection.Name)]
public sealed class ClearButtonTests
{
    private readonly HeadlessSession _session;

    public ClearButtonTests(HeadlessSession session) => _session = session;

    private static Button ClearButton(ModernPersianDatePicker picker)
        => picker.GetVisualDescendants().OfType<Button>().First(b => b.Name == "PART_ClearButton");

    private static Control Separator(ModernPersianDatePicker picker)
        => picker.GetVisualDescendants().OfType<Control>().First(c => c.Name == "PART_ClearSeparator");

    private static TextBox EditableText(ModernPersianDatePicker picker)
        => picker.GetVisualDescendants().OfType<TextBox>().First(t => t.Name == "PART_EditableText");

    private static Window Show(Control content)
    {
        var window = new Window { Width = 400, Height = 200, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static void Click(Button button)
    {
        button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();
    }

    [Fact]
    public void Off_TheButtonNeverShows()
    {
        var visible = _session.Ui(() =>
        {
            var picker = new ModernPersianDatePicker { CalendarType = CalendarType.Persian };
            var window = Show(picker);
            try
            {
                picker.SelectedDate = new PersianDate(1405, 9, 1, 0);
                Dispatcher.UIThread.RunJobs();
                return ClearButton(picker).IsVisible || Separator(picker).IsVisible;
            }
            finally
            {
                window.Close();
            }
        });

        Assert.False(visible);
    }

    [Fact]
    public void On_TheButtonShowsOnlyWithAValue_AndClickingItClears()
    {
        var result = _session.Ui(() =>
        {
            var picker = new ModernPersianDatePicker { CalendarType = CalendarType.Persian, ShowClearButton = true };
            var window = Show(picker);
            try
            {
                var empty = ClearButton(picker).IsVisible;
                picker.SelectedDateTime = new DateTime(2026, 11, 22);
                Dispatcher.UIThread.RunJobs();
                var withValue = ClearButton(picker).IsVisible && Separator(picker).IsVisible;

                Click(ClearButton(picker));

                return (empty, withValue, picker.SelectedDateTime, afterClear: ClearButton(picker).IsVisible);
            }
            finally
            {
                window.Close();
            }
        });

        Assert.False(result.empty);
        Assert.True(result.withValue);
        Assert.Null(result.SelectedDateTime);
        Assert.False(result.afterClear);
    }

    [Fact]
    public void RangeMode_ClickingIt_ClearsBothEnds()
    {
        var result = _session.Ui(() =>
        {
            var picker = new ModernPersianDatePicker { CalendarType = CalendarType.Persian, IsRangeMode = true, ShowClearButton = true };
            var window = Show(picker);
            try
            {
                picker.RangeStartDateTime = new DateTime(2026, 11, 22);
                picker.RangeEndDateTime = new DateTime(2026, 12, 21);
                Dispatcher.UIThread.RunJobs();
                var visible = ClearButton(picker).IsVisible;

                Click(ClearButton(picker));

                return (visible, picker.RangeStart, picker.RangeEnd, picker.RangeStartDateTime, picker.RangeEndDateTime);
            }
            finally
            {
                window.Close();
            }
        });

        Assert.True(result.visible);
        Assert.Null(result.RangeStart);
        Assert.Null(result.RangeEnd);
        Assert.Null(result.RangeStartDateTime);
        Assert.Null(result.RangeEndDateTime);
    }

    [Fact]
    public void InvalidTextKept_ShowsTheButton_AndClickingItEmptiesTheBox()
    {
        var result = _session.Ui(() =>
        {
            var picker = new ModernPersianDatePicker
            {
                CalendarType = CalendarType.Persian,
                IsEditable = true,
                InvalidValueAction = InvalidValueAction.Keep,
                ShowClearButton = true,
            };
            var window = Show(picker);
            try
            {
                EditableText(picker).Text = "1405/13/01";
                picker.CommitText();
                Dispatcher.UIThread.RunJobs();
                var visible = ClearButton(picker).IsVisible;
                var invalid = picker.HasInvalidInput;

                Click(ClearButton(picker));

                return (visible, invalid, text: EditableText(picker).Text, stillInvalid: picker.HasInvalidInput,
                    afterClear: ClearButton(picker).IsVisible);
            }
            finally
            {
                window.Close();
            }
        });

        Assert.True(result.invalid);
        Assert.True(result.visible);
        Assert.True(string.IsNullOrEmpty(result.text));
        Assert.False(result.stillInvalid);
        Assert.False(result.afterClear);
    }

    [Fact]
    public void TypingIntoAnEmptyBox_ShowsTheButton()
    {
        var visible = _session.Ui(() =>
        {
            var picker = new ModernPersianDatePicker { IsEditable = true, ShowClearButton = true };
            var window = Show(picker);
            try
            {
                EditableText(picker).Text = "1405/0";
                Dispatcher.UIThread.RunJobs();
                return ClearButton(picker).IsVisible;
            }
            finally
            {
                window.Close();
            }
        });

        Assert.True(visible);
    }

    [Fact]
    public void CtrlDelete_InTheBox_Clears()
    {
        var result = _session.Ui(() =>
        {
            var picker = new ModernPersianDatePicker
            {
                CalendarType = CalendarType.Persian,
                IsEditable = true,
                ShowClearButton = true,
                SelectedDate = new PersianDate(1405, 9, 1, 0),
            };
            var window = Show(picker);
            try
            {
                EditableText(picker).RaiseEvent(new KeyEventArgs
                {
                    RoutedEvent = InputElement.KeyDownEvent,
                    Key = Key.Delete,
                    KeyModifiers = KeyModifiers.Control,
                });
                Dispatcher.UIThread.RunJobs();
                return picker.SelectedDate;
            }
            finally
            {
                window.Close();
            }
        });

        Assert.Null(result);
    }

    [Fact]
    public void ADisabledPicker_ShowsNoButton()
    {
        var visible = _session.Ui(() =>
        {
            var picker = new ModernPersianDatePicker
            {
                CalendarType = CalendarType.Persian,
                ShowClearButton = true,
                SelectedDate = new PersianDate(1405, 9, 1, 0),
            };
            var window = Show(picker);
            try
            {
                picker.IsEnabled = false;
                Dispatcher.UIThread.RunJobs();
                return ClearButton(picker).IsVisible;
            }
            finally
            {
                window.Close();
            }
        });

        Assert.False(visible);
    }

    [Fact]
    public void TheButton_IsNotAFocusStop()
    {
        var focusable = _session.Ui(() =>
        {
            var picker = new ModernPersianDatePicker { ShowClearButton = true, SelectedDate = new PersianDate(1405, 9, 1, 0) };
            var window = Show(picker);
            try
            {
                return ClearButton(picker).Focusable;
            }
            finally
            {
                window.Close();
            }
        });

        Assert.False(focusable);
    }
}
