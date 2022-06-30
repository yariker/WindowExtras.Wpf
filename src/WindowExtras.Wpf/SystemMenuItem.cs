using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace WindowExtras.Wpf;

public class SystemMenuItem : Animatable, ICommandSource
{
    public event EventHandler? Click;

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        "Header", typeof(string), typeof(SystemMenuItem), new PropertyMetadata(string.Empty, null, OnCoerceHeader));

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        "Command", typeof(ICommand), typeof(SystemMenuItem));

    /// <inheritdoc />
    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
        "CommandParameter", typeof(object), typeof(SystemMenuItem), new PropertyMetadata(default(object)));

    /// <inheritdoc />
    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register(
        "CommandTarget", typeof(IInputElement), typeof(SystemMenuItem), new PropertyMetadata(default(IInputElement)));

    /// <inheritdoc />
    public IInputElement CommandTarget
    {
        get => (IInputElement)GetValue(CommandTargetProperty);
        set => SetValue(CommandTargetProperty, value);
    }

    /// <inheritdoc />
    protected override Freezable CreateInstanceCore() => new SystemMenuItem();

    protected internal virtual void OnClick()
    {
        Click?.Invoke(this, EventArgs.Empty);
    }

    private static object OnCoerceHeader(DependencyObject d, object? baseValue)
    {
        return baseValue ?? string.Empty;
    }
}
