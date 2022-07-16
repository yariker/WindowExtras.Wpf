using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using WindowExtras.Wpf.Helpers;
using static Windows.Win32.PInvoke;

namespace WindowExtras.Wpf.Menu;

/// <summary>
/// Represents the system menu of a <see cref="Window"/>.
/// </summary>
[ContentProperty(nameof(Items))]
public class SystemMenu : Animatable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SystemMenu"/> class.
    /// </summary>
    public SystemMenu()
    {
        Items = new MenuItemCollection();
    }

    #region MinimizeBox

    /// <summary>
    /// Identifies the <see cref="MinimizeBox"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty MinimizeBoxProperty = DependencyProperty.Register(
        nameof(MinimizeBox), typeof(bool?), typeof(SystemMenu));

    /// <summary>
    /// Gets or sets a value indicating whether the Minimize button is displayed in the caption bar of the <see cref="Window"/>.
    /// </summary>
    /// <value>
    /// <c>true</c> to display the Minimize button in the caption bar; <c>false</c> to hide it.
    /// The default value is <c>null</c>, which let's the <see cref="Window"/> handle its Minimize button
    /// visibility based on the <see cref="Window.ResizeMode"/>.
    /// </value>
    [Category("Common")]
    public bool? MinimizeBox
    {
        get => (bool?)GetValue(MinimizeBoxProperty);
        set => SetValue(MinimizeBoxProperty, value);
    }

    #endregion

    #region MaximizeBox

    /// <summary>
    /// Identifies the <see cref="MaximizeBox"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty MaximizeBoxProperty = DependencyProperty.Register(
        nameof(MaximizeBox), typeof(bool?), typeof(SystemMenu));

    /// <summary>
    /// Gets or sets a value indicating whether the Maximize button is displayed in the caption bar of the <see cref="Window"/>.
    /// </summary>
    /// <value>
    /// <c>true</c> to display the Maximize button in the caption bar; <c>false</c> to hide it.
    /// The default value is <c>null</c>, which let's the <see cref="Window"/> handle its Maximize button
    /// visibility based on the <see cref="Window.ResizeMode"/>.
    /// </value>
    [Category("Common")]
    public bool? MaximizeBox
    {
        get => (bool?)GetValue(MaximizeBoxProperty);
        set => SetValue(MaximizeBoxProperty, value);
    }

    #endregion

    #region ControlBox

    /// <summary>
    /// Identifies the <see cref="ControlBox"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ControlBoxProperty = DependencyProperty.Register(
        nameof(ControlBox), typeof(bool), typeof(SystemMenu), new PropertyMetadata(true));

    /// <summary>
    /// Gets or sets a value indicating whether a control box is displayed in the caption bar of the <see cref="Window"/>.
    /// </summary>
    /// <value>
    /// <c>true</c> to display the control box in the caption bar; <c>false</c> to hide it.
    /// The default value is <c>true</c>.
    /// </value>
    [Category("Common")]
    public bool ControlBox
    {
        get => (bool)GetValue(ControlBoxProperty);
        set => SetValue(ControlBoxProperty, value);
    }

    #endregion

    #region ShowIcon

    /// <summary>
    /// Identifies the <see cref="ShowIcon"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ShowIconProperty = DependencyProperty.Register(
        "ShowIcon", typeof(bool), typeof(SystemMenu), new PropertyMetadata(true));

    /// <summary>
    /// Gets or sets a value indicating whether an icon is displayed in the caption bar of the <see cref="Window"/>.
    /// </summary>
    /// <value>
    /// <c>true</c> to display the icon in the caption bar; <c>false</c> to hide it.
    /// The default value is <c>true</c>.
    /// </value>
    [Category("Common")]
    public bool ShowIcon
    {
        get => (bool)GetValue(ShowIconProperty);
        set => SetValue(ShowIconProperty, value);
    }

    #endregion

    #region Items

    /// <summary>
    /// Identifies the <see cref="Items"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
        "Items", typeof(MenuItemCollection), typeof(SystemMenu));

    /// <summary>
    /// Gets or sets the collection of menu items appended to the <see cref="Window"/>
    /// system menu.
    /// </summary>
    /// <value>
    /// A collection of the <see cref="MenuItem"/> objects.
    /// </value>
    [Category("Common")]
    public MenuItemCollection? Items
    {
        get => (MenuItemCollection?)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    #endregion

    /// <inheritdoc />
    protected override Freezable CreateInstanceCore() => new SystemMenu();

    internal static void RefreshIcon(Window window, SystemMenu? systemMenu)
    {
        var hwnd = (HWND)window.GetHwnd();
        var style = (WINDOW_EX_STYLE)GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);

        if (systemMenu == null || systemMenu.ShowIcon)
        {
            style &= ~WINDOW_EX_STYLE.WS_EX_DLGMODALFRAME;
            SetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, (int)style);

            window.InvalidateFrame();
            window.RefreshIcon();
        }
        else
        {
            style |= WINDOW_EX_STYLE.WS_EX_DLGMODALFRAME;
            SetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, (int)style);

            window.InvalidateFrame();
            window.RefreshIcon();

            SendMessage(hwnd, WM_SETICON, new WPARAM(ICON_SMALL), 0);
            SendMessage(hwnd, WM_SETICON, new WPARAM(ICON_BIG), 0);
        }
    }

    internal static void RefreshControlBox(Window window, SystemMenu? systemMenu)
    {
        var hwnd = (HWND)window.GetHwnd();

        // Reset to default.
        GetSystemMenu(hwnd, true);

        var menu = GetSystemMenu_SafeHandle(hwnd, false);
        var oldStyle = (WINDOW_STYLE)GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE);
        var newStyle = oldStyle;

        switch (systemMenu?.MinimizeBox)
        {
            case null:
                switch (window.ResizeMode)
                {
                    case ResizeMode.NoResize:
                        newStyle &= ~WINDOW_STYLE.WS_MINIMIZEBOX;
                        break;
                    default:
                        newStyle |= WINDOW_STYLE.WS_MINIMIZEBOX;
                        break;
                }

                break;
            case true:
                newStyle |= WINDOW_STYLE.WS_MINIMIZEBOX;
                break;
            case false:
                newStyle &= ~WINDOW_STYLE.WS_MINIMIZEBOX;
                break;
        }

        switch (systemMenu?.MaximizeBox)
        {
            case null:
                switch (window.ResizeMode)
                {
                    case ResizeMode.CanResize:
                    case ResizeMode.CanResizeWithGrip:
                        newStyle |= WINDOW_STYLE.WS_MAXIMIZEBOX;
                        break;
                    default:
                        newStyle &= ~WINDOW_STYLE.WS_MAXIMIZEBOX;
                        break;
                }

                break;
            case true:
                newStyle |= WINDOW_STYLE.WS_MAXIMIZEBOX;
                break;
            case false:
                newStyle &= ~WINDOW_STYLE.WS_MAXIMIZEBOX;
                break;
        }

        switch (systemMenu?.ControlBox)
        {
            case null:
            case true:
                newStyle |= WINDOW_STYLE.WS_SYSMENU;
                break;
            case false:
                newStyle &= ~WINDOW_STYLE.WS_SYSMENU;
                break;
        }

        EnableSystemMenu(menu, newStyle);

        if (newStyle != oldStyle)
        {
            SetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE, (int)newStyle);
            window.InvalidateFrame();
        }

        if (systemMenu != null)
        {
            InsertMenuItems(menu, systemMenu);
        }

        // Don't destroy the system menu handle.
        menu.SetHandleAsInvalid();
    }

    private static void EnableSystemMenu(SafeHandle menu, WINDOW_STYLE style)
    {
        // Restore.
        if (style.HasFlag(WINDOW_STYLE.WS_MAXIMIZE))
        {
            EnableMenuItem(menu, SC_RESTORE, MENU_ITEM_FLAGS.MF_BYCOMMAND | MENU_ITEM_FLAGS.MF_ENABLED);
        }
        else
        {
            EnableMenuItem(menu, SC_RESTORE, MENU_ITEM_FLAGS.MF_BYCOMMAND | MENU_ITEM_FLAGS.MF_GRAYED);
        }

        // Size.
        if (style.HasFlag(WINDOW_STYLE.WS_THICKFRAME))
        {
            EnableMenuItem(menu, SC_SIZE, MENU_ITEM_FLAGS.MF_BYCOMMAND | MENU_ITEM_FLAGS.MF_ENABLED);
        }
        else
        {
            EnableMenuItem(menu, SC_SIZE, MENU_ITEM_FLAGS.MF_BYCOMMAND | MENU_ITEM_FLAGS.MF_GRAYED);
        }

        // Minimize.
        if (style.HasFlag(WINDOW_STYLE.WS_MINIMIZEBOX))
        {
            EnableMenuItem(menu, SC_MINIMIZE, MENU_ITEM_FLAGS.MF_BYCOMMAND | MENU_ITEM_FLAGS.MF_ENABLED);
        }
        else
        {
            EnableMenuItem(menu, SC_MINIMIZE, MENU_ITEM_FLAGS.MF_BYCOMMAND | MENU_ITEM_FLAGS.MF_GRAYED);
        }

        // Maximize.
        if (style.HasFlag(WINDOW_STYLE.WS_MAXIMIZEBOX))
        {
            EnableMenuItem(menu, SC_MAXIMIZE, MENU_ITEM_FLAGS.MF_BYCOMMAND | MENU_ITEM_FLAGS.MF_ENABLED);
        }
        else
        {
            EnableMenuItem(menu, SC_MAXIMIZE, MENU_ITEM_FLAGS.MF_BYCOMMAND | MENU_ITEM_FLAGS.MF_GRAYED);
        }

        // Close.
        if (style.HasFlag(WINDOW_STYLE.WS_SYSMENU))
        {
            EnableMenuItem(menu, SC_CLOSE, MENU_ITEM_FLAGS.MF_BYCOMMAND | MENU_ITEM_FLAGS.MF_ENABLED);
        }
        else
        {
            EnableMenuItem(menu, SC_CLOSE, MENU_ITEM_FLAGS.MF_BYCOMMAND | MENU_ITEM_FLAGS.MF_GRAYED);
        }
    }

    private static void InsertMenuItems(SafeHandle menu, SystemMenu systemMenu)
    {
        if (systemMenu.Items?.Count > 0)
        {
            for (int i = 0; i < systemMenu.Items.Count; i++)
            {
                var menuItem = systemMenu.Items[i];
                var flags = MENU_ITEM_FLAGS.MF_BYCOMMAND;

                if (menuItem.Kind == MenuItemKind.Separator)
                {
                    flags |= MENU_ITEM_FLAGS.MF_SEPARATOR;
                }

                if (!menuItem.IsEnabled)
                {
                    flags |= MENU_ITEM_FLAGS.MF_DISABLED;
                }

                InsertMenu(menu, SC_CLOSE, flags, (nuint)i, menuItem.Text);
            }

            InsertMenu(menu, SC_CLOSE, MENU_ITEM_FLAGS.MF_BYCOMMAND | MENU_ITEM_FLAGS.MF_SEPARATOR, 0, null);
        }
    }
}
