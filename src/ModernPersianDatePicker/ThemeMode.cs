namespace ModernPersianDatePicker;

/// <summary>
/// Determines how a <see cref="ModernPersianDatePicker"/> resolves its Light/Dark theme.
/// </summary>
public enum ThemeMode
{
    /// <summary>
    /// Follow the operating system's current Light/Dark setting, updating automatically when it changes.
    /// </summary>
    System,

    /// <summary>
    /// Always use the Light theme regardless of the system/application setting.
    /// </summary>
    Light,

    /// <summary>
    /// Always use the Dark theme regardless of the system/application setting.
    /// </summary>
    Dark
}
