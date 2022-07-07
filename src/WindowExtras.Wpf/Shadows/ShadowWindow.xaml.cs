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

    private Window? _owner;
    private HWND _hwnd;
    private HWND _ownerHwnd;
    private HwndTarget? _hwndTarget;
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
        if (!IsVisible && _owner?.IsLoaded == true)
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

        if (_owner != window)
        {
            _owner = window;
            _owner.Loaded += OnOwnerLoaded;
            _owner.LocationChanged += OnOwnerLocationChanged;
            _owner.SizeChanged += OnOwnerSizeChanged;
            _owner.Activated += OnOwnerActivated;
            _owner.StateChanged += OnOwnerStateChanged;
            _owner.Closed += OnOwnerClosed;
        }
    }

    internal void Detach()
    {
        if (_owner != null)
        {
            _owner.Loaded -= OnOwnerLoaded;
            _owner.LocationChanged -= OnOwnerLocationChanged;
            _owner.SizeChanged -= OnOwnerSizeChanged;
            _owner.Activated -= OnOwnerActivated;
            _owner.StateChanged -= OnOwnerStateChanged;
            _owner.Closed -= OnOwnerClosed;
            _owner = null;
        }
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        var ownerInterop = new WindowInteropHelper(_owner ?? throw new InvalidOperationException());
        _ownerHwnd = (HWND)ownerInterop.Handle;

        var windowInterop = new WindowInteropHelper(this);
        _hwnd = (HWND)windowInterop.Handle;

        var hwndSource = HwndSource.FromHwnd(windowInterop.Handle)!;
        hwndSource.AddHook(WindowHook);

        _hwndTarget = hwndSource.CompositionTarget;

        SetTransparent();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        Detach();
    }

    private void OnOwnerLoaded(object sender, RoutedEventArgs e)
    {
        UpdatePosition(true, true);
        TryShow();
    }

    private void OnOwnerLocationChanged(object? sender, EventArgs e)
    {
        UpdatePosition(true, false);
    }

    private void OnOwnerSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdatePosition(false, true);
    }

    private void OnOwnerActivated(object? sender, EventArgs e)
    {
        UpdatePosition(true, true);
    }

    private void OnOwnerStateChanged(object? sender, EventArgs e)
    {
        switch (_owner?.WindowState)
        {
            case WindowState.Normal:
                Visibility = Visibility.Visible;
                break;

            case WindowState.Minimized:
            case WindowState.Maximized:
                Visibility = Visibility.Collapsed;
                break;
        }
    }

    private void OnOwnerClosed(object? sender, EventArgs e)
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

    private void UpdatePosition(bool move, bool resize)
    {
        if (_owner == null)
        {
            return;
        }

        var margin = Root.Margin;
        var flags = SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE |
                    SET_WINDOW_POS_FLAGS.SWP_NOSENDCHANGING;

        var location = new Point(
            _owner.Left - margin.Left,
            _owner.Top - margin.Top);

        var size = new Point(
            _owner.Width + margin.Left + margin.Right,
            _owner.Height + margin.Top + margin.Bottom);

        if (!move)
        {
            flags |= SET_WINDOW_POS_FLAGS.SWP_NOMOVE;
        }

        if (!resize)
        {
            flags |= SET_WINDOW_POS_FLAGS.SWP_NOSIZE;
        }

        if (_hwndTarget != null)
        {
            if (move)
            {
                var point = _hwndTarget.TransformToDevice.Transform(location);
                _left = DpiHelper.Round(point.X);
                _top = DpiHelper.Round(point.Y);
            }

            if (resize)
            {
                var point = _hwndTarget.TransformToDevice.Transform(size);
                _width = DpiHelper.Round(point.X);
                _height = DpiHelper.Round(point.Y);
            }

            SetWindowPos(_hwnd, _ownerHwnd, _left, _top, _width, _height, flags);
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

    private unsafe IntPtr WindowHook(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
    {
        if (msg == WM_WINDOWPOSCHANGING)
        {
            var position = (WINDOWPOS*)lparam;
            position->hwndInsertAfter = _ownerHwnd;
            position->x = _left;
            position->y = _top;
            position->cx = _width;
            position->cy = _height;
            handled = true;
        }

        return IntPtr.Zero;
    }
}
