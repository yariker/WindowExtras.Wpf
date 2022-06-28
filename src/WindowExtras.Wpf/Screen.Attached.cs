using System;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Win32;
using WindowExtras.Wpf.Helpers;

namespace WindowExtras.Wpf;

public partial record Screen
{
    private static readonly DependencyPropertyKey CurrentPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        "Current", typeof(Screen), typeof(Screen), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

    /// <summary>
    /// Identifies the Current attached property.
    /// </summary>
    private static readonly DependencyProperty CurrentProperty = CurrentPropertyKey.DependencyProperty;

    static Screen()
    {
        if (DesignerHelper.IsInDesignMode)
        {
            return;
        }

        EventManager.RegisterClassHandler(
            typeof(Window),
            FrameworkElement.LoadedEvent,
            new RoutedEventHandler(OnWindowLoaded),
            true);

        SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
    }

    /// <summary>
    /// Gets the value of the Current attached property from the specified element.
    /// </summary>
    /// <returns>
    /// A <see cref="Screen"/> instance that represents the display on which the specified element is displayed.
    /// </returns>
    [AttachedPropertyBrowsableForType(typeof(Window))]
    public static Screen GetCurrent(DependencyObject element)
    {
        if (element == null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        return (Screen)element.GetValue(CurrentProperty);
    }

    private static void SetCurrent(DependencyObject element, Screen screen)
    {
        if (element == null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        element.SetValue(CurrentPropertyKey, screen);
    }

    private static void OnWindowLoaded(object? sender, RoutedEventArgs e)
    {
        var window = (Window)sender!;
        window.LocationChanged += OnWindowLocationChanged;
        window.Unloaded += OnWindowUnloaded;
        Refresh(window, null);
    }

    private static void OnWindowUnloaded(object? sender, RoutedEventArgs e)
    {
        var window = (Window)sender!;
        window.LocationChanged -= OnWindowLocationChanged;
        window.Unloaded -= OnWindowLocationChanged;
    }

    private static void OnWindowLocationChanged(object? sender, EventArgs e)
    {
        var window = (Window)sender!;
        var screen = GetCurrent(window);

        var windowBounds = new Rect(window.Left, window.Top, window.Width, window.Height);
        var screenBounds = screen.Bounds;

        screenBounds.Intersect(windowBounds);

        if (screenBounds.Width <= windowBounds.Width / 2 ||
            screenBounds.Height <= windowBounds.Height / 2)
        {
            Refresh(window, screen);
        }
    }

    private static void OnDisplaySettingsChanged(object? sender, EventArgs e)
    {
        foreach (Window window in Application.Current.Windows)
        {
            Refresh(window, GetCurrent(window));
        }
    }

    private static void Refresh(Window window, Screen? current)
    {
        var hwndSource = (HwndSource?)PresentationSource.FromVisual(window);
        if (hwndSource == null)
        {
            return;
        }

        var screen = FromHwnd(hwndSource.Handle);
        if (current != screen)
        {
            SetCurrent(window, screen);
        }
    }
}
