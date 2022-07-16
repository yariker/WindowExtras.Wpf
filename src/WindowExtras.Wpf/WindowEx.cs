using System.Windows;
using Microsoft.Win32;
using WindowExtras.Wpf.Helpers;

namespace WindowExtras.Wpf;

/// <summary>
/// Provides attached properties that extend the functionality of the <see cref="Window"/>.
/// </summary>
public static partial class WindowEx
{
    static WindowEx()
    {
        if (DesignerHelper.IsInDesignMode)
        {
            return;
        }

        EventManager.RegisterClassHandler(
            typeof(Window), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnWindowLoaded), true);

        SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
    }
}
