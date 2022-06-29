using System.Windows;
using System.Windows.Media.Animation;

namespace WindowExtras.Wpf;

/// <summary>
/// Represents the system menu of a <see cref="Window"/>.
/// </summary>
public partial class SystemMenu : Animatable
{
    public static readonly DependencyProperty MinimizeBoxProperty = DependencyProperty.Register(
        nameof(MinimizeBox), typeof(bool), typeof(SystemMenu), new PropertyMetadata(true));

    public bool MinimizeBox
    {
        get => (bool)GetValue(MinimizeBoxProperty);
        set => SetValue(MinimizeBoxProperty, value);
    }

    public static readonly DependencyProperty MaximizeBoxProperty = DependencyProperty.Register(
        nameof(MaximizeBox), typeof(bool), typeof(SystemMenu), new PropertyMetadata(true));

    public bool MaximizeBox
    {
        get => (bool)GetValue(MaximizeBoxProperty);
        set => SetValue(MaximizeBoxProperty, value);
    }

    public static readonly DependencyProperty ControlBoxProperty = DependencyProperty.Register(
        nameof(ControlBox), typeof(bool), typeof(SystemMenu), new PropertyMetadata(true));

    public bool ControlBox
    {
        get => (bool)GetValue(ControlBoxProperty);
        set => SetValue(ControlBoxProperty, value);
    }

    public static readonly DependencyProperty ShowIconProperty = DependencyProperty.Register(
        "ShowIcon", typeof(bool), typeof(SystemMenu), new PropertyMetadata(true));

    public bool ShowIcon
    {
        get { return (bool)GetValue(ShowIconProperty); }
        set { SetValue(ShowIconProperty, value); }
    }

    /// <inheritdoc />
    protected override Freezable CreateInstanceCore() => new SystemMenu();
}
