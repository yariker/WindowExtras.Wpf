using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace WindowExtras.Wpf;

public class WindowShadow : Animatable
{
    #region CornerRadius

    public static readonly DependencyProperty CornerRadiusProperty =
        Border.CornerRadiusProperty.AddOwner(typeof(WindowShadow));

    [Bindable(true)]
    [Category("Appearance")]
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    #endregion

    #region Radius

    public static readonly DependencyProperty RadiusProperty =
        BlurEffect.RadiusProperty.AddOwner(typeof(WindowShadow), new PropertyMetadata(20.0, null, OnCoerceRadius));

    [Bindable(true)]
    [Category("Appearance")]
    public double Radius
    {
        get => (double)GetValue(RadiusProperty);
        set => SetValue(RadiusProperty, value);
    }

    private static object OnCoerceRadius(DependencyObject sender, object value)
    {
        // MAX_RADIUS = 100 according to BlurEffect.h.
        return Math.Max(0, Math.Min(100, (double)value));
    }

    #endregion

    #region Opacity

    public static readonly DependencyProperty OpacityProperty =
        UIElement.OpacityProperty.AddOwner(typeof(WindowShadow));

    [Bindable(true)]
    [Category("Appearance")]
    public double Opacity
    {
        get => (double)GetValue(OpacityProperty);
        set => SetValue(OpacityProperty, value);
    }

    #endregion

    #region OffsetX

    public static readonly DependencyProperty OffsetXProperty = DependencyProperty.Register(
        "OffsetX", typeof(double), typeof(WindowShadow));

    [Bindable(true)]
    [Category("Appearance")]
    public double OffsetX
    {
        get => (double)GetValue(OffsetXProperty);
        set => SetValue(OffsetXProperty, value);
    }

    #endregion

    #region OffsetY

    public static readonly DependencyProperty OffsetYProperty = DependencyProperty.Register(
        "OffsetY", typeof(double), typeof(WindowShadow));

    [Bindable(true)]
    [Category("Appearance")]
    public double OffsetY
    {
        get => (double)GetValue(OffsetYProperty);
        set => SetValue(OffsetYProperty, value);
    }

    #endregion

    #region ShadowBrush

    public static readonly DependencyProperty ShadowBrushProperty = DependencyProperty.Register(
        "ShadowBrush", typeof(Brush), typeof(WindowShadow), new PropertyMetadata(Brushes.Black));

    [Bindable(true)]
    public Brush ShadowBrush
    {
        get => (Brush)GetValue(ShadowBrushProperty);
        set => SetValue(ShadowBrushProperty, value);
    }

    #endregion

    #region BackdropBrush

    public static readonly DependencyProperty BackdropBrushProperty = DependencyProperty.Register(
        "BackdropBrush", typeof(Brush), typeof(WindowShadow));

    [Bindable(true)]
    public Brush BackdropBrush
    {
        get => (Brush)GetValue(BackdropBrushProperty);
        set => SetValue(BackdropBrushProperty, value);
    }

    #endregion

    /// <inheritdoc />
    protected override Freezable CreateInstanceCore() => new WindowShadow();
}