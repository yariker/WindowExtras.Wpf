using System;
using System.Windows;
using WindowExtras.Wpf.Helpers;
using WindowExtras.Wpf.Shadows;

namespace WindowExtras.Wpf;

public static partial class WindowEx
{
    /// <summary>
    /// Identifies the Shadow attached property.
    /// </summary>
    public static readonly DependencyProperty WindowShadowProperty = DependencyProperty.RegisterAttached(
        "WindowShadow", typeof(WindowShadow), typeof(WindowEx), new PropertyMetadata(OnShadowChanged));

    /// <summary>
    /// Gets the value of the <see cref="WindowShadowProperty"/> from the specified <see cref="Window"/>.
    /// </summary>
    /// <returns>
    /// The instance of <see cref="WindowShadow"/> that is attached to the specified <see cref="Window"/>.
    /// </returns>
    [AttachedPropertyBrowsableForType(typeof(Window))]
    public static WindowShadow? GetWindowShadow(DependencyObject window)
    {
        if (window == null)
        {
            throw new ArgumentNullException(nameof(window));
        }

        return (WindowShadow?)window.GetValue(WindowShadowProperty);
    }

    /// <summary>
    /// Sets the value of the <see cref="WindowShadowProperty"/> on the specified <see cref="Window"/>.
    /// </summary>
    public static void SetWindowShadow(DependencyObject window, WindowShadow? value)
    {
        if (window == null)
        {
            throw new ArgumentNullException(nameof(window));
        }

        window.SetValue(WindowShadowProperty, value);
    }

    private static void OnShadowChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not Window window || DesignerHelper.IsInDesignMode)
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
}
