using System;
using System.Windows;
using System.Windows.Input;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using WindowExtras.Wpf.Helpers;
using static Windows.Win32.PInvoke;

namespace WindowExtras.Wpf;

public static partial class WindowEx
{
    private static readonly CommandBinding[] SystemCommandBindings =
    {
        new(SystemCommands.CloseWindowCommand, OnCloseWindowExecute, OnCloseWindowCanExecute),
        new(SystemCommands.MaximizeWindowCommand, OnMaximizeWindowExecute, OnMaximizeWindowCanExecute),
        new(SystemCommands.MinimizeWindowCommand, OnMinimizeWindowExecute, OnMinimizeWindowCanExecute),
        new(SystemCommands.RestoreWindowCommand, OnRestoreWindowExecute, OnRestoreWindowCanExecute),
        new(SystemCommands.ShowSystemMenuCommand, OnShowSystemMenuExecute),
    };

    /// <summary>
    /// Identifies the EnableSystemCommands attached property.
    /// </summary>
    public static readonly DependencyProperty EnableSystemCommandsProperty = DependencyProperty.RegisterAttached(
        "EnableSystemCommands", typeof(bool), typeof(WindowEx), new PropertyMetadata(false, OnEnableSystemCommandsChanged));

    /// <summary>
    /// Sets the value indicating whether the binding of the <see cref="SystemCommands"/> of the <see cref="Window"/> is enabled.
    /// </summary>
    /// <param name="window">The dependency object on which to set the value of the <see cref="EnableSystemCommandsProperty"/>.</param>
    /// <param name="value">The new value to set the property to.</param>
    public static void SetEnableSystemCommands(DependencyObject window, bool value)
    {
        if (window == null)
        {
            throw new ArgumentNullException(nameof(window));
        }

        window.SetValue(EnableSystemCommandsProperty, value);
    }

    /// <summary>
    /// Gets the value indicating whether the binding of the <see cref="SystemCommands"/> of the <see cref="Window"/> is enabled.
    /// </summary>
    /// <param name="window">The dependency object from which to retrieve the value of the <see cref="EnableSystemCommandsProperty"/>.</param>
    /// <returns>The current value of the <see cref="EnableSystemCommandsProperty"/> on the specified dependency object.</returns>
    [AttachedPropertyBrowsableForType(typeof(Window))]
    public static bool GetBindSystemCommands(DependencyObject window)
    {
        if (window == null)
        {
            throw new ArgumentNullException(nameof(window));
        }

        return (bool)window.GetValue(EnableSystemCommandsProperty);
    }

    private static void OnEnableSystemCommandsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not Window window || DesignerHelper.IsInDesignMode)
        {
            return;
        }

        switch (e.NewValue)
        {
            case true:
                foreach (CommandBinding binding in SystemCommandBindings)
                {
                    window.CommandBindings.Add(binding);
                }

                window.StateChanged += OnWindowStateChanged;
                break;

            case false:
                foreach (CommandBinding binding in SystemCommandBindings)
                {
                    window.CommandBindings.Remove(binding);
                }

                window.StateChanged -= OnWindowStateChanged;
                break;
        }
    }

    private static void OnWindowStateChanged(object sender, EventArgs e)
    {
        CommandManager.InvalidateRequerySuggested();
    }

    private static void OnCloseWindowExecute(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is Window window)
        {
            SystemCommands.CloseWindow(window);
            e.Handled = true;
        }
    }

    private static void OnCloseWindowCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is Window window)
        {
            e.CanExecute = IsWindowStyleSet(window, WINDOW_STYLE.WS_SYSMENU);
            e.Handled = true;
        }
    }

    private static void OnMaximizeWindowExecute(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is Window window)
        {
            SystemCommands.MaximizeWindow(window);
            e.Handled = true;
        }
    }

    private static void OnMaximizeWindowCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is Window window)
        {
            e.CanExecute = window.WindowState != WindowState.Maximized &&
                           IsWindowStyleSet(window, WINDOW_STYLE.WS_MAXIMIZEBOX);

            e.Handled = true;
        }
    }

    private static void OnMinimizeWindowExecute(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is Window window)
        {
            SystemCommands.MinimizeWindow(window);
            e.Handled = true;
        }
    }

    private static void OnMinimizeWindowCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is Window window)
        {
            e.CanExecute = window.WindowState != WindowState.Minimized &&
                           IsWindowStyleSet(window, WINDOW_STYLE.WS_MINIMIZEBOX);

            e.Handled = true;
        }
    }

    private static void OnRestoreWindowExecute(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is Window window)
        {
            SystemCommands.RestoreWindow(window);
            e.Handled = true;
        }
    }

    private static void OnRestoreWindowCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is Window window)
        {
            e.CanExecute = window.WindowState is WindowState.Maximized or WindowState.Minimized;
            e.Handled = true;
        }
    }

    private static void OnShowSystemMenuExecute(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is Window window)
        {
            var position = e.Parameter is Point p ? p : GetMouseScreenPosition(window);
            SystemCommands.ShowSystemMenu(window, position);
            e.Handled = true;
        }
    }

    private static Point GetMouseScreenPosition(Window window)
    {
        GetCursorPos(out POINT p);
        var transformMatrix = DpiHelper.GetTransformMatrix(window, TransformDirection.FromDevice);
        return transformMatrix.Transform(new Point(p.x, p.y));
    }

    private static bool IsWindowStyleSet(Window window, WINDOW_STYLE flag)
    {
        var hwnd = window.GetHwnd();
        var style = (WINDOW_STYLE)GetWindowLong((HWND)hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE);
        return style.HasFlag(flag);
    }
}
