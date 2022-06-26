using System;
using System.Windows;
using System.Windows.Media;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using static Windows.Win32.PInvoke;

namespace WindowExtras.Wpf.Helpers;

internal static class DpiHelper
{
    internal static int Round(double value) => (int)Math.Round(value, MidpointRounding.AwayFromZero);

    internal static Matrix GetTransformMatrix(TransformDirection direction) => GetTransformMatrix(0, direction);

    internal static Matrix GetTransformMatrix(Visual visual, TransformDirection direction)
    {
        var target = PresentationSource.FromVisual(visual)?.CompositionTarget;
        if (target != null)
        {
            return direction switch
            {
                TransformDirection.ToDevice => target.TransformToDevice,
                TransformDirection.FromDevice => target.TransformFromDevice,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
            };
        }

        return GetTransformMatrix(0, direction);
    }

    internal static Matrix GetTransformMatrix(nint hwnd, TransformDirection direction)
    {
        var deviceContext = GetDC((HWND)hwnd);

        try
        {
            var dpiX = GetDeviceCaps(deviceContext, GET_DEVICE_CAPS_INDEX.LOGPIXELSX);
            var dpiY = GetDeviceCaps(deviceContext, GET_DEVICE_CAPS_INDEX.LOGPIXELSY);

            var matrix = new Matrix();

            switch (direction)
            {
                case TransformDirection.ToDevice:
                    matrix.Scale(dpiX / 96.0, dpiY / 96.0);
                    break;
                case TransformDirection.FromDevice:
                    matrix.Scale(96.0 / dpiX, 96.0 / dpiY);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            return matrix;
        }
        finally
        {
            ReleaseDC((HWND)hwnd, deviceContext);
        }
    }
}
