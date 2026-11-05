using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace ModernPersianDatePicker.Tests.Headless;

/// <summary>
/// The picker in a running application: its ControlThemes, the layout an application
/// can change, one focus stop, the forced Light / Dark modes, the popup, and the time
/// section that is always left-to-right.
/// </summary>
[Collection(HeadlessCollection.Name)]
public sealed class TemplateAndThemeTests
{
    private readonly HeadlessSession _session;

    public TemplateAndThemeTests(HeadlessSession session) => _session = session;

    /// <summary>An application's own date field: a plain subclass, nothing overridden.</summary>
    private sealed class AppDateField : ModernPersianDatePicker
    {
    }

    private static Window Show(Control content, FlowDirection direction = FlowDirection.LeftToRight)
    {
        var window = new Window { Width = 500, Height = 600, FlowDirection = direction, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static T Part<T>(Visual root, string name) where T : Control
        => root.GetVisualDescendants().OfType<T>().First(c => c.Name == name);

    private static void OpenPopup(ModernPersianDatePicker picker)
    {
        Part<ToggleButton>(picker, "PART_ToggleButton").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();
    }

    private static Popup PopupOf(ModernPersianDatePicker picker)
        => picker.GetVisualDescendants().OfType<Popup>().First();

    private static Color BrushColor(StyledElement element, string key)
        => element.TryFindResource(key, element.ActualThemeVariant, out var value) && value is ISolidColorBrush brush
            ? brush.Color
            : default;

    // ── ControlThemes ──────────────────────────────────────────────────────

    [Fact]
    public void TheLook_IsAControlTheme_KeyedByType()
        => Assert.True(_session.Ui(() =>
            Application.Current!.TryFindResource(typeof(ModernPersianDatePicker), out var theme) && theme is ControlTheme));

    [Fact]
    public void ASubclass_GetsThePickersTemplate_WithoutOverridingAnything()
    {
        var hasTemplate = _session.Ui(() =>
        {
            var field = new AppDateField { IsEditable = true };
            var window = Show(field);
            try
            {
                return field.GetVisualDescendants().OfType<TextBox>().Any(t => t.Name == "PART_EditableText");
            }
            finally
            {
                window.Close();
            }
        });

        Assert.True(hasTemplate);
    }

    /// <summary>
    /// The text box's layout comes from the picker (TemplateBinding): an application's
    /// style sets the picker's Padding and the text moves. It used to be fixed IN the
    /// template, which outranks every style - and the picker's own Padding did nothing.
    /// </summary>
    [Fact]
    public void AnApplicationStyle_SetsThePadding_OfTheTextBox()
    {
        var (editable, display) = _session.Ui(() =>
        {
            var editablePicker = new ModernPersianDatePicker { IsEditable = true };
            var displayPicker = new ModernPersianDatePicker();
            var panel = new StackPanel { Children = { editablePicker, displayPicker } };
            panel.Styles.Add(new Style(x => x.OfType<ModernPersianDatePicker>())
            {
                Setters = { new Setter(TemplatedControl.PaddingProperty, new Thickness(20, 10)) },
            });
            var window = Show(panel);
            try
            {
                return (Part<TextBox>(editablePicker, "PART_EditableText").Padding,
                        Part<TextBlock>(displayPicker, "PART_DisplayText").Margin);
            }
            finally
            {
                window.Close();
            }
        });

        Assert.Equal(new Thickness(20, 10), editable);
        Assert.Equal(new Thickness(20, 10), display);
    }

    [Fact]
    public void ThereIsNoBuiltInMinimumWidth()
        => Assert.Equal(0, _session.Ui(() =>
        {
            var picker = new ModernPersianDatePicker();
            var window = Show(picker);
            try
            {
                return picker.MinWidth;
            }
            finally
            {
                window.Close();
            }
        }));

    // ── Focus ──────────────────────────────────────────────────────────────

    /// <summary>
    /// An editable picker is ONE focus stop - its text box. The control itself used to
    /// be focusable next to it: Tab / Enter stopped twice per field.
    /// </summary>
    [Fact]
    public void AnEditablePicker_IsOneFocusStop_ItsTextBox()
    {
        var (focusableWhenEditable, focusableWhenNot, focused, borderThickness) = _session.Ui(() =>
        {
            var picker = new ModernPersianDatePicker { IsEditable = true };
            var window = Show(picker);
            try
            {
                var editable = picker.Focusable;
                picker.FocusEditableText();
                Dispatcher.UIThread.RunJobs();
                var focusedElement = window.FocusManager?.GetFocusedElement();
                var thickness = picker.BorderThickness;

                picker.IsEditable = false;
                return (editable, picker.Focusable, focusedElement == Part<TextBox>(picker, "PART_EditableText"), thickness);
            }
            finally
            {
                window.Close();
            }
        });

        Assert.False(focusableWhenEditable);
        Assert.True(focusableWhenNot);
        Assert.True(focused);
        // Focus colours the border; it no longer thickens it (the box jumped).
        Assert.Equal(new Thickness(1), borderThickness);
    }

    // ── Theme modes ────────────────────────────────────────────────────────

    /// <summary>
    /// A forced Light picker in a Dark application is LIGHT - box and popup. "Light"
    /// used to only remove the Dark overrides, so it followed the (dark) application.
    /// </summary>
    [Fact]
    public void ForcedLightAndDark_WinOverTheApplicationsVariant_InTheBoxAndThePopup()
    {
        var colours = _session.Ui(() =>
        {
            var app = Application.Current!;
            var previous = app.RequestedThemeVariant;
            app.RequestedThemeVariant = ThemeVariant.Dark;
            var light = new ModernPersianDatePicker { Theme = ThemeMode.Light };
            var system = new ModernPersianDatePicker();
            var window = Show(new StackPanel { Children = { light, system } });
            try
            {
                OpenPopup(light);
                var calendar = PopupOf(light).Child!.GetLogicalDescendants().OfType<CalendarView>().First();
                var result = (
                    LightBox: BrushColor(light, "PersianDatePickerBackgroundBrush"),
                    LightPopup: BrushColor(calendar, "PersianDatePickerPopupBackgroundBrush"),
                    SystemBox: BrushColor(system, "PersianDatePickerBackgroundBrush"));

                light.Theme = ThemeMode.Dark;
                return (result.LightBox, result.LightPopup, result.SystemBox,
                        DarkBox: BrushColor(light, "PersianDatePickerBackgroundBrush"));
            }
            finally
            {
                window.Close();
                app.RequestedThemeVariant = previous;
            }
        });

        Assert.Equal(Color.Parse("#FFFFFFFF"), colours.LightBox);
        Assert.Equal(Color.Parse("#FFFFFFFF"), colours.LightPopup);
        Assert.Equal(Color.Parse("#FF2D2D2D"), colours.SystemBox);   // System follows the app (Dark)
        Assert.Equal(Color.Parse("#FF2D2D2D"), colours.DarkBox);
    }

    // ── Popup ──────────────────────────────────────────────────────────────

    [Fact]
    public void ThePopup_IsAtLeastAsWideAsTheField()
    {
        var minWidth = _session.Ui(() =>
        {
            var picker = new ModernPersianDatePicker { Width = 420 };
            var window = Show(picker);
            try
            {
                OpenPopup(picker);
                return ((Control)PopupOf(picker).Child!).MinWidth;
            }
            finally
            {
                window.Close();
            }
        });

        Assert.Equal(420, minWidth);
    }

    [Fact]
    public void TheCalendarButton_HasItsOwnTheme_NoFluentFill()
    {
        var (hasTheme, background) = _session.Ui(() =>
        {
            var picker = new ModernPersianDatePicker();
            var window = Show(picker);
            try
            {
                var toggle = Part<ToggleButton>(picker, "PART_ToggleButton");
                return (Application.Current!.TryFindResource("PersianDatePickerToggleButtonTheme", out var theme)
                        && ReferenceEquals(toggle.Theme, theme),
                        toggle.Background);
            }
            finally
            {
                window.Close();
            }
        });

        Assert.True(hasTheme);
        Assert.Equal(Brushes.Transparent, background);
    }

    // ── The time section ───────────────────────────────────────────────────

    /// <summary>
    /// The time reads hour : minute : second left to right in every language. In an
    /// RTL window it used to inherit the direction and mirror the spinners. Neither
    /// the window's direction, a later change of it, nor a local value may flip it.
    /// </summary>
    [Fact]
    public void TheTimeSection_IsAlwaysLeftToRight()
    {
        var observed = _session.Ui(() =>
        {
            var picker = new ModernPersianDatePicker { IsTimePickerEnabled = true };
            var window = Show(picker, FlowDirection.RightToLeft);
            try
            {
                OpenPopup(picker);
                var time = PopupOf(picker).Child!.GetLogicalDescendants().OfType<TimePickerView>().First();
                Dispatcher.UIThread.RunJobs();
                var hour = Part<TextBox>(time, "PART_HourBox");
                var second = Part<TextBox>(time, "PART_SecondBox");
                var hourLeftOfSecond = hour.TranslatePoint(default, time)!.Value.X
                                       < second.TranslatePoint(default, time)!.Value.X;
                var inRtl = time.FlowDirection;

                window.FlowDirection = FlowDirection.LeftToRight;
                window.FlowDirection = FlowDirection.RightToLeft;
                var afterParentChanges = time.FlowDirection;

                time.FlowDirection = FlowDirection.RightToLeft;
                return (inRtl, afterParentChanges, Local: time.FlowDirection, hourLeftOfSecond,
                        PickerDirection: picker.FlowDirection);
            }
            finally
            {
                window.Close();
            }
        });

        Assert.Equal(FlowDirection.RightToLeft, observed.PickerDirection);   // the picker itself stays RTL
        Assert.Equal(FlowDirection.LeftToRight, observed.inRtl);
        Assert.Equal(FlowDirection.LeftToRight, observed.afterParentChanges);
        Assert.Equal(FlowDirection.LeftToRight, observed.Local);
        Assert.True(observed.hourLeftOfSecond, "The hour box is not left of the second box.");
    }
}
