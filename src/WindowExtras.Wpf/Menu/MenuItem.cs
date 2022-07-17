using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using WindowExtras.Wpf.Helpers;

namespace WindowExtras.Wpf.Menu;

/// <summary>
/// Represents the menu item.
/// </summary>
public class MenuItem : Animatable, ICommandSource
{
    private bool _isEnabledCore = true;

    /// <summary>
    /// Occurs when a menu item is clicked.
    /// </summary>
    public event EventHandler? Click;

    #region IsEnabled

    /// <summary>
    /// Identifies the <see cref="IsEnabled"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(
        nameof(IsEnabled), typeof(bool), typeof(MenuItem), new PropertyMetadata(true, null, OnCoerceIsEnabled));

    /// <summary>
    /// Determines whether this <see cref="MenuItem"/> is enabled for user interactions.
    /// </summary>
    [Category("Common")]
    public bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    private static object OnCoerceIsEnabled(DependencyObject sender, object baseValue)
    {
        return sender is MenuItem menuItem
            ? (bool)baseValue && menuItem.IsEnabledCore
            : baseValue;
    }

    #endregion

    #region Kind

    /// <summary>
    /// Identifies the <see cref="Kind"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty KindProperty = DependencyProperty.Register(
        nameof(Kind), typeof(MenuItemKind), typeof(MenuItem), new PropertyMetadata(MenuItemKind.Text));

    /// <summary>
    /// Gets or sets the value determining the menu item kind.
    /// </summary>
    [Category("Common")]
    public MenuItemKind Kind
    {
        get => (MenuItemKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    #endregion

    #region Text

    /// <summary>
    /// Identifies the <see cref="Text"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text), typeof(string), typeof(MenuItem), new PropertyMetadata(string.Empty, null, OnCoerceText));

    /// <summary>
    /// Gets or sets the menu item text.
    /// </summary>
    [Category("Common")]
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    private static object OnCoerceText(DependencyObject sender, object? baseValue)
    {
        return baseValue ?? string.Empty;
    }

    #endregion

    #region Command

    /// <summary>
    /// Identifies the <see cref="Command"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        nameof(Command), typeof(ICommand), typeof(MenuItem), new PropertyMetadata(OnCommandChanged));

    /// <inheritdoc />
    [Category("Common")]
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    private static void OnCommandChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not MenuItem menuItem || DesignerHelper.IsInDesignMode)
        {
            return;
        }

        if (e.OldValue is ICommand oldCommand)
        {
            CanExecuteChangedEventManager.RemoveHandler(oldCommand, menuItem.OnCommandCanExecuteChanged);
        }

        if (e.NewValue is ICommand newCommand)
        {
            CanExecuteChangedEventManager.AddHandler(newCommand, menuItem.OnCommandCanExecuteChanged);
        }

        menuItem.UpdateIsEnabledCore();
    }

    #endregion

    #region CommandParameter

    /// <summary>
    /// Identifies the <see cref="CommandParameter"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
        nameof(CommandParameter), typeof(object), typeof(MenuItem));

    /// <inheritdoc />
    [Category("Common")]
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    #endregion

    #region CommandTarget

    /// <summary>
    /// Identifies the <see cref="CommandTarget"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register(
        nameof(CommandTarget), typeof(IInputElement), typeof(MenuItem));

    /// <inheritdoc />
    [Category("Common")]
    public IInputElement? CommandTarget
    {
        get => (IInputElement)GetValue(CommandTargetProperty);
        set => SetValue(CommandTargetProperty, value);
    }

    #endregion

    /// <summary>
    /// Allows inheritors to disable the <see cref="MenuItem"/> regardless of the <see cref="IsEnabled"/> base property value.
    /// </summary>
    protected bool IsEnabledCore
    {
        get => _isEnabledCore;
        set
        {
            if (_isEnabledCore != value)
            {
                _isEnabledCore = value;
                CoerceValue(IsEnabledProperty);
            }
        }
    }

    /// <summary>
    /// Invokes the <see cref="Click"/> event.
    /// </summary>
    protected internal virtual void OnClick()
    {
        Click?.Invoke(this, EventArgs.Empty);
        CommandHelper.Execute(this);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Kind switch
        {
            MenuItemKind.Separator => "(Separator)",
            _ => Text,
        };
    }

    /// <inheritdoc />
    protected override Freezable CreateInstanceCore() => new MenuItem();

    private void OnCommandCanExecuteChanged(object? sender, EventArgs e)
    {
        UpdateIsEnabledCore();
    }

    private void UpdateIsEnabledCore()
    {
        IsEnabledCore = Command == null || CommandHelper.CanExecute(this);
    }
}
