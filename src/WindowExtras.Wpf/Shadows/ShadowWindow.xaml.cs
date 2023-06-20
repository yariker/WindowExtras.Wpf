using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using WindowExtras.Wpf.Helpers;
using static Windows.Win32.PInvoke;

namespace WindowExtras.Wpf.Shadows;

/// <summary>
/// Interaction logic for ChildWindow.xaml
/// </summary>
internal partial class ShadowWindow : Window
{
    #region ShadowWindow

    internal static readonly DependencyProperty ShadowWindowProperty = DependencyProperty.RegisterAttached(
        "ShadowWindow", typeof(ShadowWindow), typeof(ShadowWindow));

    internal static void SetShadowWindow(Window element, ShadowWindow? value)
    {
        if (element == null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        element.SetValue(ShadowWindowProperty, value);
    }

    [AttachedPropertyBrowsableForType(typeof(Window))]
    internal static ShadowWindow? GetShadowWindow(Window element)
    {
        if (element == null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        return (ShadowWindow?)element.GetValue(ShadowWindowProperty);
    }

    #endregion

    private HWND _hwnd;
    private HwndTarget? _hwndTarget;

    private Window? _hostWindow;
    private HwndSource? _hostSource;
    private HWND _hostHwnd;

    private HwndSource? _ownerSource;

    private int _left;
    private int _top;
    private int _width;
    private int _height;

    internal ShadowWindow()
    {
        InitializeComponent();
    }

    internal void TryShow()
    {
        if (!IsVisible && _hostWindow?.IsLoaded == true)
        {
            Show();
        }
    }

    internal void SetShadow(WindowShadow shadow)
    {
        if (shadow == null)
        {
            throw new ArgumentNullException(nameof(shadow));
        }

        BindingOperations.SetBinding(
            Blur,
            BlurEffect.RadiusProperty,
            new Binding(nameof(WindowShadow.Radius)) { Source = shadow });

        BindingOperations.SetBinding(
            Translate,
            TranslateTransform.XProperty,
            new Binding(nameof(WindowShadow.OffsetX)) { Source = shadow });

        BindingOperations.SetBinding(
            Translate,
            TranslateTransform.YProperty,
            new Binding(nameof(WindowShadow.OffsetY)) { Source = shadow });

        BindingOperations.SetBinding(
            Shadow,
            OpacityProperty,
            new Binding(nameof(WindowShadow.Opacity)) { Source = shadow });

        BindingOperations.SetBinding(
            Shadow,
            Border.BackgroundProperty,
            new Binding(nameof(WindowShadow.ShadowBrush)) { Source = shadow });

        BindingOperations.SetBinding(
            Shadow,
            Border.CornerRadiusProperty,
            new Binding(nameof(WindowShadow.CornerRadius)) { Source = shadow });

        BindingOperations.SetBinding(
            Backdrop,
            Border.BackgroundProperty,
            new Binding(nameof(WindowShadow.BackdropBrush)) { Source = shadow });

        BindingOperations.SetBinding(
            Blur,
            BlurEffect.RenderingBiasProperty,
            new Binding(nameof(WindowShadow.RenderingBias)) { Source = shadow });
    }

    internal void Attach(Window window)
    {
        if (window == null)
        {
            throw new ArgumentNullException(nameof(window));
        }

        if (_hostWindow != window)
        {
            _hostWindow = window;
            _hostWindow.Loaded += OnHostWindowLoaded;
            _hostWindow.Closed += OnHostWindowClosed;
        }
    }

    internal void Detach()
    {
        if (_hostWindow != null)
        {
            _hostWindow.Loaded -= OnHostWindowLoaded;
            _hostWindow.Closed -= OnHostWindowClosed;
            _hostWindow = null;
        }

        if (_hostSource != null)
        {
            _hostSource.RemoveHook(HostWindowHook);
            _hostSource = null;
        }

        if (_ownerSource != null)
        {
            _ownerSource.RemoveHook(HostOwnerHook);
            _ownerSource = null;
        }
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        if (_hostWindow == null)
        {
            throw new InvalidOperationException();
        }

        // Host window.
        _hostSource = (HwndSource)PresentationSource.FromVisual(_hostWindow)!;
        _hostHwnd = (HWND)_hostSource.Handle;
        _hostSource.AddHook(HostWindowHook);

        // Owner's owner window.
        if (_hostWindow.Owner is Window hostOwner)
        {
            _ownerSource = (HwndSource)PresentationSource.FromVisual(hostOwner)!;
            _ownerSource.AddHook(HostOwnerHook);
        }

        // Shadow window.
        var hwndSource = (HwndSource)PresentationSource.FromVisual(this)!;
        _hwnd = (HWND)hwndSource.Handle;
        _hwndTarget = hwndSource.CompositionTarget;
        hwndSource.AddHook(ShadowWindowHook);

        SetTransparent();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        Detach();
    }

    private void OnHostWindowLoaded(object sender, RoutedEventArgs e)
    {
        UpdatePosition(true, true);
        TryShow();
    }

    private void OnHostWindowClosed(object? sender, EventArgs e)
    {
        Close();
    }

    private void OnMarginUpdated(object? sender, DataTransferEventArgs e)
    {
        UpdatePosition(true, true);
    }

    private void SetTransparent()
    {
        var style = (WINDOW_EX_STYLE)GetWindowLong(_hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
        style |= WINDOW_EX_STYLE.WS_EX_TRANSPARENT | WINDOW_EX_STYLE.WS_EX_NOACTIVATE;
        SetWindowLong(_hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, (int)style);
    }

    private unsafe void UpdatePosition(WINDOWPOS* hostPosition)
    {
        if (_hostWindow == null || _hwndTarget == null)
        {
            return;
        }

        var margin = Root.Margin;

        var location = new Point(-margin.Left, -margin.Top);
        location = _hwndTarget.TransformToDevice.Transform(location);
        location.Offset(hostPosition->x, hostPosition->y);

        var size = new Point(margin.Left + margin.Right, margin.Top + margin.Bottom);
        size = _hwndTarget.TransformToDevice.Transform(size);
        size.Offset(hostPosition->cx, hostPosition->cy);

        UpdatePosition(
            DpiHelper.Round(location.X), DpiHelper.Round(location.Y),
            DpiHelper.Round(size.X), DpiHelper.Round(size.Y),
            !hostPosition->flags.HasFlag(SET_WINDOW_POS_FLAGS.SWP_NOMOVE),
            !hostPosition->flags.HasFlag(SET_WINDOW_POS_FLAGS.SWP_NOSIZE));
    }

    private void UpdatePosition(bool move, bool resize)
    {
        if (_hostWindow == null)
        {
            return;
        }

        var margin = Root.Margin;

        var location = new Point(
            _hostWindow.Left - margin.Left,
            _hostWindow.Top - margin.Top);

        var size = new Point(
            _hostWindow.Width + margin.Left + margin.Right,
            _hostWindow.Height + margin.Top + margin.Bottom);

        UpdatePosition(location, size, move, resize);
    }

    private void UpdatePosition(Point location, Point size, bool move, bool resize)
    {
        if (_hwndTarget != null)
        {
            location = _hwndTarget.TransformToDevice.Transform(location);
            size = _hwndTarget.TransformToDevice.Transform(size);

            UpdatePosition(
                DpiHelper.Round(location.X), DpiHelper.Round(location.Y),
                DpiHelper.Round(size.X), DpiHelper.Round(size.Y),
                move, resize);
        }
        else
        {
            if (move)
            {
                Left = location.X;
                Top = location.Y;
            }

            if (resize)
            {
                Width = size.X;
                Height = size.Y;
            }
        }
    }

    private void UpdatePosition(int left, int top, int width, int height, bool move, bool resize)
    {
        if (_hostWindow == null)
        {
            return;
        }

        var flags = SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE |
                    SET_WINDOW_POS_FLAGS.SWP_NOSENDCHANGING;

        if (!move)
        {
            flags |= SET_WINDOW_POS_FLAGS.SWP_NOMOVE;
        }

        if (!resize)
        {
            flags |= SET_WINDOW_POS_FLAGS.SWP_NOSIZE;
        }

        if (move)
        {
            _left = left;
            _top = top;
        }

        if (resize)
        {
            _width = width;
            _height = height;
        }

        SetWindowPos(_hwnd, _hostHwnd, _left, _top, _width, _height, flags);
    }

    private unsafe nint HostWindowHook(nint hwnd, int msg, nint wparam, nint lparam, ref bool handled)
    {
        if (msg == WM_WINDOWPOSCHANGED)
        {
            var position = (WINDOWPOS*)lparam;
            UpdatePosition(position);
        }
        else if (msg == WM_SIZE)
        {
            if (wparam == SIZE_MAXIMIZED || wparam == SIZE_MINIMIZED)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }
        else if (msg == WM_SYSCOMMAND)
        {
            var command = wparam & 0xFFF0;
            if (command == SC_MAXIMIZE || command == SC_MINIMIZE)
            {
                Hide();
            }
        }

        return 0;
    }

    private unsafe nint ShadowWindowHook(nint hwnd, int msg, nint wparam, nint lparam, ref bool handled)
    {
        if (msg == WM_WINDOWPOSCHANGING)
        {
            // Override shadow window position.
            var position = (WINDOWPOS*)lparam;
            position->hwndInsertAfter = _hostHwnd;
            position->x = _left;
            position->y = _top;
            position->cx = _width;
            position->cy = _height;
            handled = true;
        }

        return 0;
    }

    private unsafe nint HostOwnerHook(nint hwnd, int msg, nint wparam, nint lparam, ref bool handled)
    {
        if (msg == WM_WINDOWPOSCHANGED)
        {
            var position = (WINDOWPOS*)lparam;
            if (position->hwndInsertAfter != _hwnd)
            {
                // Update shadow window z-order.
                UpdatePosition(false, false);
            }
        }

        return 0;
    }
}
