using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;

namespace WindowExtras.Wpf.Helpers;

internal static class WindowHelper
{
    /// <summary>
    /// An empty icon (required for icon refresh).
    /// </summary>
    private static readonly BitmapSource EmptyBitmap = BitmapSource.Create(
        1, 1, 96, 96, PixelFormats.Bgra32, null, new byte[] { 0, 0, 0, 0 }, 4);

    internal static nint GetHwnd(this Window window)
    {
        if (window == null)
        {
            throw new ArgumentNullException(nameof(window));
        }

        var hwndSource = (HwndSource?)PresentationSource.FromVisual(window);
        return hwndSource != null ? hwndSource.Handle : 0;
    }

    internal static void RefreshIcon(this Window window)
    {
        if (window == null)
        {
            throw new ArgumentNullException(nameof(window));
        }

        var icon = window.Icon;
        window.Icon = EmptyBitmap;
        window.Icon = icon;
    }

    internal static void InvalidateFrame(this Window window)
    {
        var hwnd = window.GetHwnd();
        SetWindowPos((HWND)hwnd, default, 0, 0, 0, 0, SET_WINDOW_POS_FLAGS.SWP_NOSIZE |
                                                      SET_WINDOW_POS_FLAGS.SWP_NOZORDER |
                                                      SET_WINDOW_POS_FLAGS.SWP_NOMOVE |
                                                      SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE |
                                                      SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED);
    }
}
