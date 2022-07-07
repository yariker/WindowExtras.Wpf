using System;
using System.Windows;
using System.Windows.Interop;
using WindowExtras.Wpf.Helpers;

namespace WindowExtras.Wpf;

public static partial class WindowEx
{
    private static readonly DependencyPropertyKey ScreenPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        "Screen", typeof(Screen), typeof(WindowEx), new PropertyMetadata(null));

    /// <summary>
    /// Identifies the Screen attached property.
    /// </summary>
    private static readonly DependencyProperty ScreenProperty = ScreenPropertyKey.DependencyProperty;

    /// <summary>
    /// Gets the value of the Screen attached property from the specified window.
    /// </summary>
    /// <returns>
    /// A <see cref="Screen"/> instance that represents the display on which the specified window is displayed.
    /// </returns>
    [AttachedPropertyBrowsableForType(typeof(Window))]
    public static Screen GetScreen(DependencyObject window)
    {
        if (window == null)
        {
            throw new ArgumentNullException(nameof(window));
        }

        return (Screen)window.GetValue(ScreenProperty);
    }

    private static void SetScreen(DependencyObject window, Screen screen)
    {
        if (window == null)
        {
            throw new ArgumentNullException(nameof(window));
        }

        window.SetValue(ScreenPropertyKey, screen);
    }

    private static void OnWindowLoaded(object? sender, RoutedEventArgs e)
    {
        var window = (Window)sender!;
        window.LocationChanged += OnWindowLocationChanged;
        RefreshScreen(window, null);
    }

    private static void OnWindowLocationChanged(object? sender, EventArgs e)
    {
        var window = (Window)sender!;
        var screen = GetScreen(window);

        var windowBounds = new Rect(window.Left, window.Top, window.Width, window.Height);
        var screenBounds = screen.Bounds;

        screenBounds.Intersect(windowBounds);

        if (screenBounds.Width <= windowBounds.Width / 2 ||
            screenBounds.Height <= windowBounds.Height / 2)
        {
            RefreshScreen(window, screen);
        }
    }

    private static void OnDisplaySettingsChanged(object? sender, EventArgs e)
    {
        foreach (Window window in Application.Current.Windows)
        {
            RefreshScreen(window, GetScreen(window));
        }
    }

    private static void RefreshScreen(Window window, Screen? current)
    {
        var hwndSource = (HwndSource?)PresentationSource.FromVisual(window);
        if (hwndSource == null)
        {
            return;
        }

        var screen = Screen.FromHwnd(hwndSource.Handle);
        if (current != screen)
        {
            SetScreen(window, screen);
        }
    }
}
