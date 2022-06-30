using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using WindowExtras.Wpf.Helpers;
using static Windows.Win32.PInvoke;

namespace WindowExtras.Wpf;

public partial class SystemMenu
{
    /// <summary>
    /// An empty icon (required for icon refresh).
    /// </summary>
    private static readonly BitmapSource EmptyBitmap = BitmapSource.Create(
        1, 1, 96, 96, PixelFormats.Bgra32, null, new byte[] { 0, 0, 0, 0 }, 4);

    /// <summary>
    /// Identifies the Menu attached property.
    /// </summary>
    public static readonly DependencyProperty MenuProperty = DependencyProperty.RegisterAttached(
        "Menu", typeof(SystemMenu), typeof(SystemMenu), new PropertyMetadata(OnMenuChanged));

    /// <summary>
    /// Gets the value of the <see cref="MenuProperty"/> from the specified <see cref="Window"/>.
    /// </summary>
    [AttachedPropertyBrowsableForType(typeof(Window))]
    public static SystemMenu? GetMenu(DependencyObject element)
    {
        if (element == null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        return (SystemMenu)element.GetValue(MenuProperty);
    }

    /// <summary>
    /// Sets the value of the <see cref="MenuProperty"/> on the specified <see cref="Window"/>.
    /// </summary>
    public static void SetMenu(DependencyObject element, SystemMenu? value)
    {
        if (element == null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        element.SetValue(MenuProperty, value);
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
            UpdateSystemMenu(window, systemMenu);
        }
        else if (e.OldValue == null)
        {
            window.SourceInitialized += OnWindowSourceInitialized;
        }
        else if (e.NewValue == null)
        {
            window.SourceInitialized -= OnWindowSourceInitialized;
        }
    }

    private static void OnWindowSourceInitialized(object? sender, EventArgs e)
    {
        var window = (Window)sender!;
        var systemMenu = GetMenu(window);
        UpdateSystemMenu(window, systemMenu);
    }

    private static void UpdateSystemMenu(Window window, SystemMenu? systemMenu)
    {
        UpdateControlBox(window, systemMenu);
        UpdateIcon(window, systemMenu);
    }

    private static void UpdateControlBox(Window window, SystemMenu? systemMenu)
    {
        var hwndSource = (HwndSource)PresentationSource.FromVisual(window)!;
        var hwnd = (HWND)hwndSource.Handle;

        GetSystemMenu(hwnd, true);

        using var menu = GetSystemMenu_SafeHandle(hwnd, false);
        var oldStyle = (WINDOW_STYLE)GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE);
        var newStyle = oldStyle;

        if (systemMenu == null || systemMenu.MinimizeBox)
        {
            EnableMenuItem(menu, SC_MINIMIZE, MENU_ITEM_FLAGS.MF_BYCOMMAND | MENU_ITEM_FLAGS.MF_ENABLED);
            newStyle |= WINDOW_STYLE.WS_MINIMIZEBOX;
        }
        else
        {
            EnableMenuItem(menu, SC_MINIMIZE, MENU_ITEM_FLAGS.MF_BYCOMMAND | MENU_ITEM_FLAGS.MF_GRAYED);
            newStyle &= ~WINDOW_STYLE.WS_MINIMIZEBOX;
        }

        if (systemMenu == null || systemMenu.MaximizeBox)
        {
            EnableMenuItem(menu, SC_MAXIMIZE, MENU_ITEM_FLAGS.MF_BYCOMMAND | MENU_ITEM_FLAGS.MF_ENABLED);
            newStyle |= WINDOW_STYLE.WS_MAXIMIZEBOX;
        }
        else
        {
            EnableMenuItem(menu, SC_MAXIMIZE, MENU_ITEM_FLAGS.MF_BYCOMMAND | MENU_ITEM_FLAGS.MF_GRAYED);
            newStyle &= ~WINDOW_STYLE.WS_MAXIMIZEBOX;
        }

        if (systemMenu == null || systemMenu.ControlBox)
        {
            EnableMenuItem(menu, SC_CLOSE, MENU_ITEM_FLAGS.MF_BYCOMMAND | MENU_ITEM_FLAGS.MF_ENABLED);
            newStyle |= WINDOW_STYLE.WS_SYSMENU;
        }
        else
        {
            EnableMenuItem(menu, SC_CLOSE, MENU_ITEM_FLAGS.MF_BYCOMMAND | MENU_ITEM_FLAGS.MF_GRAYED);
            newStyle &= ~WINDOW_STYLE.WS_SYSMENU;
        }

        if (newStyle != oldStyle)
        {
            SetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE, (int)newStyle);
            InvalidateFrame(hwnd);
        }

        if (systemMenu != null && systemMenu.Items.Count > 0)
        {
            for (int i = 0; i < systemMenu.Items.Count; i++)
            {
                InsertMenu(menu, SC_CLOSE, MENU_ITEM_FLAGS.MF_BYCOMMAND, (nuint)i, systemMenu.Items[i].Header);
            }

            InsertMenu(menu, SC_CLOSE, MENU_ITEM_FLAGS.MF_BYCOMMAND | MENU_ITEM_FLAGS.MF_SEPARATOR, 0, null);
        }
    }

    private static void UpdateIcon(Window window, SystemMenu? systemMenu)
    {
        var hwndSource = (HwndSource)PresentationSource.FromVisual(window)!;
        var hwnd = (HWND)hwndSource.Handle;

        var style = (WINDOW_EX_STYLE)GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);

        if (systemMenu == null || systemMenu.ShowIcon)
        {
            style &= ~WINDOW_EX_STYLE.WS_EX_DLGMODALFRAME;
            SetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, (int)style);

            InvalidateFrame(hwnd);
            RefreshIcon(window);
        }
        else
        {
            style |= WINDOW_EX_STYLE.WS_EX_DLGMODALFRAME;
            SetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, (int)style);

            InvalidateFrame(hwnd);
            RefreshIcon(window);

            SendMessage(hwnd, WM_SETICON, new WPARAM(ICON_SMALL), 0);
            SendMessage(hwnd, WM_SETICON, new WPARAM(ICON_BIG), 0);
        }
    }

    private static void RefreshIcon(Window window)
    {
        var icon = window.Icon;
        window.Icon = EmptyBitmap;
        window.Icon = icon;
    }

    private static void InvalidateFrame(HWND hwnd)
    {
        SetWindowPos(hwnd, HWND.HWND_TOP, 0, 0, 0, 0, SET_WINDOW_POS_FLAGS.SWP_NOSIZE |
                                                      SET_WINDOW_POS_FLAGS.SWP_NOZORDER |
                                                      SET_WINDOW_POS_FLAGS.SWP_NOMOVE |
                                                      SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE |
                                                      SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED);
    }
}
