using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Xunit;

namespace ModernPersianDatePicker.Tests.Headless;

/// <summary>A Fluent application with the picker's theme, like the demo's App.axaml.</summary>
public sealed class TestApp : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
        Styles.Add(new StyleInclude(new Uri("avares://ModernPersianDatePicker.Tests.Headless"))
        {
            Source = new Uri("avares://ModernPersianDatePicker/Themes/ModernPersianDatePickerTheme.xaml"),
        });
        RequestedThemeVariant = ThemeVariant.Light;
    }
}

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<TestApp>().UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

/// <summary>
/// ONE session for the whole assembly (a process cannot host two). Every test runs
/// its UI work through <see cref="Ui{T}(Func{T})"/>, on the session's thread.
/// </summary>
public sealed class HeadlessSession : IDisposable
{
    private readonly HeadlessUnitTestSession _session = HeadlessUnitTestSession.StartNew(typeof(TestAppBuilder));

    public T Ui<T>(Func<T> work) => _session.Dispatch(work, CancellationToken.None).GetAwaiter().GetResult();

    public void Ui(Action work) => _session.Dispatch(work, CancellationToken.None).GetAwaiter().GetResult();

    public void Dispose() => _session.Dispose();
}

[CollectionDefinition(Name)]
public sealed class HeadlessCollection : ICollectionFixture<HeadlessSession>
{
    public const string Name = "Headless";
}
