using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace WindowExtras.Wpf;

/// <summary>
/// Represents an object that describes the customizations to the drop shadow of a <see cref="Window"/>.
/// </summary>
public partial class WindowShadow : Animatable
{
    #region CornerRadius

    /// <summary>
    /// Identifies the <see cref="CornerRadius"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CornerRadiusProperty =
        Border.CornerRadiusProperty.AddOwner(typeof(WindowShadow));

    /// <summary>
    /// Gets or sets a value that indicates the amount that the corners of a shadow are rounded.
    /// </summary>
    [Category("Appearance")]
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    #endregion

    #region Radius

    /// <summary>
    /// Identifies the <see cref="Radius"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty RadiusProperty =
        BlurEffect.RadiusProperty.AddOwner(typeof(WindowShadow), new PropertyMetadata(20.0, null, OnCoerceRadius));

    /// <summary>
    /// Gets or sets a value that indicates the radius of the shadow's blur effect.
    /// </summary>
    /// <value>
    /// The radius of the shadow's blur curve. The default is <c>20.0</c>.
    /// </value>
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

    /// <summary>
    /// Identifies the <see cref="Opacity"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty OpacityProperty =
        UIElement.OpacityProperty.AddOwner(typeof(WindowShadow));

    /// <summary>
    /// Gets or sets the opacity of the drop shadow.
    /// </summary>
    /// <value>
    /// The opacity factor. The default value is <c>1.0</c>.
    /// </value>
    [Category("Appearance")]
    public double Opacity
    {
        get => (double)GetValue(OpacityProperty);
        set => SetValue(OpacityProperty, value);
    }

    #endregion

    #region OffsetX

    /// <summary>
    /// Identifies the <see cref="OffsetX"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty OffsetXProperty = DependencyProperty.Register(
        nameof(OffsetX), typeof(double), typeof(WindowShadow));

    /// <summary>
    /// Gets or sets the offset of the drop shadow along the x-axis.
    /// </summary>
    [Category("Appearance")]
    public double OffsetX
    {
        get => (double)GetValue(OffsetXProperty);
        set => SetValue(OffsetXProperty, value);
    }

    #endregion

    #region OffsetY

    /// <summary>
    /// Identifies the <see cref="OffsetY"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty OffsetYProperty = DependencyProperty.Register(
        nameof(OffsetY), typeof(double), typeof(WindowShadow));

    /// <summary>
    /// Gets or sets the offset of the drop shadow along the y-axis.
    /// </summary>
    [Category("Appearance")]
    public double OffsetY
    {
        get => (double)GetValue(OffsetYProperty);
        set => SetValue(OffsetYProperty, value);
    }

    #endregion

    #region ShadowBrush

    /// <summary>
    /// Identifies the <see cref="ShadowBrush"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ShadowBrushProperty = DependencyProperty.Register(
        nameof(ShadowBrush), typeof(Brush), typeof(WindowShadow), new PropertyMetadata(Brushes.Black));

    /// <summary>
    /// Gets or sets the <see cref="Brush"/> that defines the color of the drop shadow.
    /// </summary>
    /// <value>
    /// A <see cref="Brush"/> that defines the color of the drop shadow. The default is value <see cref="Brushes.Black"/>.
    /// </value>
    public Brush? ShadowBrush
    {
        get => (Brush?)GetValue(ShadowBrushProperty);
        set => SetValue(ShadowBrushProperty, value);
    }

    #endregion

    #region BackdropBrush

    /// <summary>
    /// Identifies the <see cref="BackdropBrush"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty BackdropBrushProperty = DependencyProperty.Register(
        nameof(BackdropBrush), typeof(Brush), typeof(WindowShadow));

    /// <summary>
    /// Gets or sets the <see cref="Brush"/> to render on top of the drop shadow, behind the host <see cref="Window"/>.
    /// </summary>
    /// <value>
    /// A <see cref="Brush"/> instance to render on top of the drop shadow, behind the host <see cref="Window"/>.
    /// The default is value <c>null</c> (no brush).
    /// </value>
    public Brush? BackdropBrush
    {
        get => (Brush?)GetValue(BackdropBrushProperty);
        set => SetValue(BackdropBrushProperty, value);
    }

    #endregion

    #region RenderingBias

    /// <summary>
    /// Identifies the <see cref="RenderingBias"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty RenderingBiasProperty =
        BlurEffect.RenderingBiasProperty.AddOwner(typeof(WindowShadow), new PropertyMetadata(RenderingBias.Quality));

    /// <summary>
    /// Gets or sets a value that indicates whether the system renders the drop shadow with emphasis on speed or quality.
    /// </summary>
    /// <value>
    /// A <see cref="RenderingBias"/> value that indicates the rendering quality.
    /// The default is <see cref="System.Windows.Media.Effects.RenderingBias.Quality"/>.
    /// </value>
    public RenderingBias RenderingBias
    {
        get => (RenderingBias)GetValue(RenderingBiasProperty);
        set => SetValue(RenderingBiasProperty, value);
    }

    #endregion

    /// <inheritdoc />
    protected override Freezable CreateInstanceCore() => new WindowShadow();
}
