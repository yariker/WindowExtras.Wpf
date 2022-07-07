using System;
using System.Windows;
using System.Windows.Interop;
using WindowExtras.Wpf.Helpers;
using WindowExtras.Wpf.Menu;
using static Windows.Win32.PInvoke;

namespace WindowExtras.Wpf;

public static partial class WindowEx
{
    /// <summary>
    /// Identifies the SystemMenu attached property.
    /// </summary>
    public static readonly DependencyProperty SystemMenuProperty = DependencyProperty.RegisterAttached(
        "SystemMenu", typeof(SystemMenu), typeof(WindowEx), new PropertyMetadata(OnMenuChanged));

    /// <summary>
    /// Gets the value of the <see cref="SystemMenuProperty"/> from the specified <see cref="Window"/>.
    /// </summary>
    [AttachedPropertyBrowsableForType(typeof(Window))]
    public static SystemMenu? GetSystemMenu(DependencyObject element)
    {
        if (element == null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        return (SystemMenu)element.GetValue(SystemMenuProperty);
    }

    /// <summary>
    /// Sets the value of the <see cref="SystemMenuProperty"/> on the specified <see cref="Window"/>.
    /// </summary>
    public static void SetSystemMenu(DependencyObject element, SystemMenu? value)
    {
        if (element == null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        element.SetValue(SystemMenuProperty, value);
    }

    private static void OnMenuChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not Window window || DesignerHelper.IsInDesignMode)
        {
            return;
        }

        var hwndSource = (HwndSource?)PresentationSource.FromVisual(window);
        if (hwndSource != null)
        {
            var systemMenu = (SystemMenu?)e.NewValue;

            SystemMenu.RefreshControlBox(window, systemMenu);
            SystemMenu.RefreshIcon(window, systemMenu);

            hwndSource.AddHook(WindowHook);
        }
        else
        {
            window.SourceInitialized -= OnWindowSourceInitialized;
            window.SourceInitialized += OnWindowSourceInitialized;
        }
    }

    private static void OnWindowSourceInitialized(object? sender, EventArgs e)
    {
        var window = (Window)sender!;
        var systemMenu = GetSystemMenu(window);
        var hwndSource = (HwndSource)PresentationSource.FromVisual(window)!;

        SystemMenu.RefreshControlBox(window, systemMenu);
        SystemMenu.RefreshIcon(window, systemMenu);

        hwndSource.AddHook(WindowHook);
    }

    private static nint WindowHook(nint hwnd, int msg, nint wparam, nint lparam, ref bool handled)
    {
        if (msg == WM_SYSCOMMAND)
        {
            if (HwndSource.FromHwnd(hwnd)?.RootVisual is Window window &&
                GetSystemMenu(window) is { Items.Count: > 0 } menu)
            {
                if (wparam < menu.Items.Count)
                {
                    menu.Items[(int)wparam].OnClick();
                    handled = true;
                }
            }
        }

        return 0;
    }
}
