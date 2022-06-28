using System;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;
using WindowExtras.Wpf.Helpers;
using static Windows.Win32.PInvoke;

namespace WindowExtras.Wpf;

public partial class SystemMenu
{
    public static readonly DependencyProperty MenuProperty = DependencyProperty.RegisterAttached(
        "Menu", typeof(SystemMenu), typeof(SystemMenu), new PropertyMetadata(OnMenuChanged));

    public static void SetMenu(DependencyObject element, SystemMenu value)
    {
        element.SetValue(MenuProperty, value);
    }

    public static SystemMenu GetMenu(DependencyObject element)
    {
        return (SystemMenu)element.GetValue(MenuProperty);
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

    private static void OnWindowSourceInitialized(object sender, EventArgs e)
    {
        var window = (Window)sender;
        var systemMenu = GetMenu(window);
        UpdateSystemMenu(window, systemMenu);
    }

    private static void UpdateSystemMenu(Window window, SystemMenu? systemMenu)
    {
        //UpdateControlBox(window, systemMenu);
        UpdateIcon(window, systemMenu);
        RedrawFrame(window);
    }

    private static void UpdateControlBox(Window window, SystemMenu? systemMenu)
    {
        var hwndSource = (HwndSource)PresentationSource.FromVisual(window)!;
        var hwnd = (HWND)hwndSource.Handle;

        var menu = GetSystemMenu(hwnd, false);
        var style = (WINDOW_STYLE)GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE);

        style |= WINDOW_STYLE.WS_THICKFRAME;

        if (systemMenu is { MinimizeBox: true })
        {
            EnableMenuItem(menu, SC_MINIMIZE, MENU_ITEM_FLAGS.MF_BYCOMMAND | MENU_ITEM_FLAGS.MF_ENABLED);
            style |= WINDOW_STYLE.WS_MINIMIZEBOX;
        }
        else
        {
            EnableMenuItem(menu, SC_MINIMIZE, MENU_ITEM_FLAGS.MF_BYCOMMAND | MENU_ITEM_FLAGS.MF_GRAYED);
            style &= ~WINDOW_STYLE.WS_MINIMIZEBOX;
        }

        if (systemMenu is { MaximizeBox: true })
        {
            EnableMenuItem(menu, SC_MAXIMIZE, MENU_ITEM_FLAGS.MF_BYCOMMAND | MENU_ITEM_FLAGS.MF_ENABLED);
            style |= WINDOW_STYLE.WS_MAXIMIZEBOX;
        }
        else
        {
            EnableMenuItem(menu, SC_MAXIMIZE, MENU_ITEM_FLAGS.MF_BYCOMMAND | MENU_ITEM_FLAGS.MF_GRAYED);
            style &= ~WINDOW_STYLE.WS_MAXIMIZEBOX;
        }

        if (systemMenu is { ControlBox: true })
        {
            EnableMenuItem(menu, SC_CLOSE, MENU_ITEM_FLAGS.MF_BYCOMMAND | MENU_ITEM_FLAGS.MF_ENABLED);
            style |= WINDOW_STYLE.WS_SYSMENU;
        }
        else
        {
            EnableMenuItem(menu, SC_CLOSE, MENU_ITEM_FLAGS.MF_BYCOMMAND | MENU_ITEM_FLAGS.MF_GRAYED);
            style &= ~WINDOW_STYLE.WS_SYSMENU;
        }

        SetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE, (int)style);
    }

    private static void UpdateIcon(Window window, SystemMenu? systemMenu)
    {
        var hwndSource = (HwndSource)PresentationSource.FromVisual(window)!;
        var hwnd = (HWND)hwndSource.Handle;

        var style = (WINDOW_EX_STYLE)GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);

        if (systemMenu is { ShowIcon: true })
        {
            style &= ~WINDOW_EX_STYLE.WS_EX_DLGMODALFRAME;
            //typeof(Window).GetMethod("UpdateIcon", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(window, null);
        }
        else
        {
            style |= WINDOW_EX_STYLE.WS_EX_DLGMODALFRAME;
            
        }

        SetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, (int)style);
        
        SendMessage(hwnd, WM_SETICON, new WPARAM(ICON_SMALL), 0);
            SendMessage(hwnd, WM_SETICON, new WPARAM(ICON_BIG), 0);
    }

    private static void RedrawFrame(Window window)
    {
        var hwndSource = (HwndSource)PresentationSource.FromVisual(window)!;
        var hwnd = (HWND)hwndSource.Handle;

        //RedrawWindow(hwnd, null, null, REDRAW_WINDOW_FLAGS.RDW_INVALIDATE | REDRAW_WINDOW_FLAGS.RDW_FRAME);

        SetWindowPos(hwnd, HWND.HWND_TOP, 0, 0, 0, 0, SET_WINDOW_POS_FLAGS.SWP_NOSIZE |
                                                      SET_WINDOW_POS_FLAGS.SWP_NOZORDER |
                                                      SET_WINDOW_POS_FLAGS.SWP_NOMOVE |
                                                      SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE |
                                                      SET_WINDOW_POS_FLAGS.SWP_DRAWFRAME);

        RedrawWindow(
            hwnd,
            null,
            null,
            REDRAW_WINDOW_FLAGS.RDW_INVALIDATE | REDRAW_WINDOW_FLAGS.RDW_ERASE | REDRAW_WINDOW_FLAGS.RDW_ALLCHILDREN);
    }
}
