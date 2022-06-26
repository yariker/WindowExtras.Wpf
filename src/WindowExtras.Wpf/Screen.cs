﻿using System;
using System.Runtime.InteropServices;
using System.Windows;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using WindowExtras.Wpf.Helpers;
using static Windows.Win32.PInvoke;

namespace WindowExtras.Wpf;

/// <summary>
/// Represents a display device.
/// </summary>
public partial record Screen
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="bounds"></param>
    /// <param name="workingArea"></param>
    /// <param name="isPrimary"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public Screen(string name, Rect bounds, Rect workingArea, bool isPrimary)
    {
        DeviceName = name ?? throw new ArgumentNullException(nameof(name));
        Bounds = bounds;
        WorkingArea = workingArea;
        IsPrimary = isPrimary;
    }

    /// <summary>
    /// Gets the device name of this screen.
    /// </summary>
    public string DeviceName { get; }

    /// <summary>
    /// Gets the bounds of the screen, in device-independent units (1/96th of an inch).
    /// </summary>
    public Rect Bounds { get; }

    /// <summary>
    /// Gets the desktop area of this screen, in device-independent units (1/96th of an inch).
    /// </summary>
    public Rect WorkingArea { get; }

    /// <summary>
    /// Gets the value indicating whether this screen is the primary device.
    /// </summary>
    public bool IsPrimary { get; }

    /// <summary>
    /// Gets the size of a maximized top-level window on this screen, in device-independent units (1/96th of an inch).
    /// </summary>
    public Size MaximizedWindowSize
    {
        get
        {
            // https://devblogs.microsoft.com/oldnewthing/20150304-00/?p=44543
            var resizeBorder = SystemParameters.WindowResizeBorderThickness;

            return new Size(
                WorkingArea.Width + resizeBorder.Left + resizeBorder.Right,
                WorkingArea.Height + resizeBorder.Top + resizeBorder.Bottom);
        }
    }

    /// <summary>
    /// Gets the primary screen.
    /// </summary>
    public static Screen GetPrimary() => FromHwnd(0);

    /// <summary>
    /// Retrieves the <see cref="Screen"/> that contains the largest portion of the specified window handle.
    /// </summary>
    public static unsafe Screen FromHwnd(nint hwnd)
    {
        var monitor = hwnd == 0
            ? MonitorFromPoint(new POINT(), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY)
            : MonitorFromWindow((HWND)hwnd, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);

        var monitorInfo = new MONITORINFOEXW { monitorInfo = { cbSize = (uint)Marshal.SizeOf<MONITORINFOEXW>() } };

        GetMonitorInfo(monitor, (MONITORINFO*)&monitorInfo);

        Span<Point> bounds = stackalloc[]
        {
            new Point(monitorInfo.monitorInfo.rcMonitor.left, monitorInfo.monitorInfo.rcMonitor.top),
            new Point(monitorInfo.monitorInfo.rcMonitor.right, monitorInfo.monitorInfo.rcMonitor.bottom),
        };

        Span<Point> workingArea = stackalloc[]
        {
            new Point(monitorInfo.monitorInfo.rcWork.left, monitorInfo.monitorInfo.rcWork.top),
            new Point(monitorInfo.monitorInfo.rcWork.right, monitorInfo.monitorInfo.rcWork.bottom),
        };

        var transform = DpiHelper.GetTransformMatrix(hwnd, TransformDirection.FromDevice);
        transform.Transform(bounds);
        transform.Transform(workingArea);

        var isPrimary = (monitorInfo.monitorInfo.dwFlags & MONITORINFOF_PRIMARY) == MONITORINFOF_PRIMARY;

        return new Screen(
            monitorInfo.szDevice.ToString(),
            new Rect(bounds[0], bounds[1]),
            new Rect(workingArea[0], workingArea[1]),
            isPrimary);
    }
}