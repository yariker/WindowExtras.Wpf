using System;
using System.ComponentModel;
using System.Windows;

namespace WindowExtras.Wpf;

public static class WindowEx
{
    #region WindowShadow

    public static readonly DependencyProperty WindowShadowProperty = DependencyProperty.RegisterAttached(
        "WindowShadow", typeof(WindowShadow), typeof(WindowShadow), new PropertyMetadata(OnWindowShadowChanged));

    public static void SetWindowShadow(Window element, WindowShadow value)
    {
        if (element == null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        element.SetValue(WindowShadowProperty, value);
    }

    [AttachedPropertyBrowsableForType(typeof(Window))]
    public static WindowShadow GetWindowShadow(Window element)
    {
        if (element == null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        return (WindowShadow)element.GetValue(WindowShadowProperty);
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