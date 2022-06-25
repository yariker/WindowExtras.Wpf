using System;
using System.ComponentModel;
using System.Windows;

namespace WindowExtras.Wpf;

/// <summary>
/// Provides attached properties for a <see cref="Window"/>.
/// </summary>
public static class WindowEx
{
    #region WindowShadow

    /// <summary>
    /// Identifies the WindowShadow dependency property.
    /// </summary>
    public static readonly DependencyProperty WindowShadowProperty = DependencyProperty.RegisterAttached(
        "WindowShadow", typeof(WindowShadow), typeof(WindowShadow), new PropertyMetadata(OnWindowShadowChanged));

    /// <summary>
    /// Gets the value of the WindowShadow attached property from the specified <see cref="Window"/>.
    /// </summary>
    public static void SetWindowShadow(DependencyObject window, WindowShadow? value)
    {
        if (window == null)
        {
            throw new ArgumentNullException(nameof(window));
        }

        window.SetValue(WindowShadowProperty, value);
    }

    /// <summary>
    /// Sets the value of the WindowShadow attached property on the specified <see cref="Window"/>.
    /// </summary>
    [AttachedPropertyBrowsableForType(typeof(Window))]
    public static WindowShadow? GetWindowShadow(DependencyObject window)
    {
        if (window == null)
        {
            throw new ArgumentNullException(nameof(window));
        }

        return (WindowShadow?)window.GetValue(WindowShadowProperty);
    }

    private static void OnWindowShadowChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not Window window || DesignerProperties.GetIsInDesignMode(window))
        {
            return;
        }

        var shadowWindow = ShadowWindow.GetShadowWindow(window);

        if (e.NewValue is null)
        {
            if (shadowWindow != null)
            {
                shadowWindow.Detach();
                shadowWindow.Close();

                window.ClearValue(ShadowWindow.ShadowWindowProperty);
            }

            return;
        }

        if (e.NewValue is WindowShadow shadow)
        {
            if (shadowWindow == null)
            {
                shadowWindow = new ShadowWindow();
                shadowWindow.Attach(window);

                ShadowWindow.SetShadowWindow(window, shadowWindow);
            }

            if (e.OldValue != e.NewValue)
            {
                shadowWindow.SetShadow(shadow);
            }

            shadowWindow.TryShow();
        }
    }

    #endregion
}
