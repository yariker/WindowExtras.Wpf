using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using WindowExtras.Wpf.Helpers;
using static Windows.Win32.PInvoke;

namespace WindowExtras.Wpf.Menu;

[ContentProperty(nameof(Text))]
public class MenuItem : Animatable, ICommandSource
{
    private bool _isEnabledCore = true;

    /// <summary>
    /// Occurs when this <see cref="MenuItem"/> is clicked.
    /// </summary>
    public event EventHandler? Click;

    #region Text

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text), typeof(string), typeof(MenuItem), new PropertyMetadata(string.Empty, null, OnCoerceText));

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

    #region IsSeparator

    public static readonly DependencyProperty IsSeparatorProperty = DependencyProperty.Register(
        nameof(IsSeparator), typeof(bool), typeof(MenuItem));

    [Category("Common")]
    public bool IsSeparator
    {
        get => (bool)GetValue(IsSeparatorProperty);
        set => SetValue(IsSeparatorProperty, value);
    }

    #endregion

    #region IsEnabled

    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(
        nameof(IsEnabled), typeof(bool), typeof(MenuItem), new PropertyMetadata(true, null, OnCoerceIsEnabled));

    [Category("Common")]
    public bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    private static object OnCoerceIsEnabled(DependencyObject sender, object baseValue)
    {
        if (sender is not MenuItem menuItem || DesignerHelper.IsInDesignMode)
        {
            return baseValue;
        }

        return (bool)baseValue && menuItem.IsEnabledCore;
    }

    #endregion

    #region Command

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        nameof(Command), typeof(ICommand), typeof(MenuItem), new PropertyMetadata(OnCommandChanged));

    /// <inheritdoc />
    [Category("Common")]
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    private bool IsEnabledCore
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

    private static void OnCommandChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not MenuItem menuItem || DesignerHelper.IsInDesignMode)
        {
            return;
        }

        if (e.OldValue is ICommand oldCommand)
        {
            CanExecuteChangedEventManager.RemoveHandler(oldCommand, menuItem.OnCanExecuteChanged);
        }

        if (e.NewValue is ICommand newCommand)
        {
            CanExecuteChangedEventManager.AddHandler(newCommand, menuItem.OnCanExecuteChanged);
        }

        menuItem.UpdateIsEnabledCore();
    }

    private void OnCanExecuteChanged(object sender, EventArgs e)
    {
        UpdateIsEnabledCore();
    }

    private void UpdateIsEnabledCore()
    {
        IsEnabledCore = Command == null || CommandHelper.CanExecute(this);
    }

    #endregion

    #region CommandParameter

    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
        nameof(CommandParameter), typeof(object), typeof(MenuItem));

    /// <inheritdoc />
    [Category("Common")]
    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    #endregion

    #region CommandTarget

    public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register(
        nameof(CommandTarget), typeof(IInputElement), typeof(MenuItem));

    /// <inheritdoc />
    [Category("Common")]
    public IInputElement CommandTarget
    {
        get => (IInputElement)GetValue(CommandTargetProperty);
        set => SetValue(CommandTargetProperty, value);
    }

    #endregion

    /// <inheritdoc />
    public override string ToString() => IsSeparator ? "(Separator)" : Text;

    /// <inheritdoc />
    protected override Freezable CreateInstanceCore() => new MenuItem();

    /// <summary>
    /// Invokes the <see cref="Click"/> event.
    /// </summary>
    protected internal virtual void OnClick()
    {
        Click?.Invoke(this, EventArgs.Empty);
        CommandHelper.Execute(this);
    }

    //protected internal virtual unsafe void OnInitializeMenuItem(nint menuItemInfo)
    //{
    //    var itemInfo = (MENUITEMINFOW*)menuItemInfo;
    //    itemInfo->cbSize = (uint)Marshal.SizeOf<MENUITEMINFOW>();
    //    itemInfo->dwTypeData = new PWSTR();
    //}
}
