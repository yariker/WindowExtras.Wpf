# <img src='src/WindowExtras.Wpf/Icon.png' alt='Logo' width='42' height='42' align='top' /> WindowExtras for WPF
<a href='https://www.nuget.org/packages/WindowExtras.Wpf' target='_blank'><img alt='NuGet' src='https://img.shields.io/nuget/v/WindowExtras.Wpf' /></a>

This package contains useful extensions for the WPF Window for .NET Framework and .NET 5.0+.

Extensions are provided as attached properties, accessible as `winex:WindowEx.*` via XAML:

| Property | Description |
| -------- | ----------- |
| [WindowShadow](#windowshadow) | Attaches a drop shadow to a window. |
| SystemMenu | Allows to adjust the window control box (mimimize, maximize and close buttons), icon (show/hide), and the system menu items. |
| Screen | Provides information about the display the window is currently on. Similar to the [SystemParameters.PrimaryScreen*](https://docs.microsoft.com/en-us/dotnet/api/system.windows.systemparameters#properties) properties, but with multi-display support. |

You can download the [Demo app](https://github.com/yariker/WindowExtras.Wpf/releases) to see these properties in action.

# WindowShadow

You can attach a drop shadow to a window without affecting the window template or conent.

Before using this property, Be sure to remove the default window shadow by setting `WindowStyle` to `None`,
as well as `ResizeMode` to `NoResize` or `AllowsTransparency` to `True`.

Here's a minimal example:

```XAML
<Window x:Class="MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:winex="https://github.com/yariker/WindowExtras.Wpf"
        Width="300" Height="200"
        ResizeMode="NoResize"
        WindowStartupLocation="CenterScreen"
        WindowStyle="None">
    
    <!-- Notice the new attached property below. -->
    <winex:WindowEx.WindowShadow>
        <winex:WindowShadow OffsetY="15" Opacity="0.3" Radius="30" />
    </winex:WindowEx.WindowShadow>

    <Button Width="100" Height="32" Click="CloseButtonClick" Content="Close">
        <x:Code><![CDATA[void CloseButtonClick(object sender, EventArgs e) => Close();]]></x:Code>
    </Button>

</Window>
```

This extension is:
* Completely customizable (color, amount of blur, opacity, offset, corner radius, etc.)
* Transparent to mouse clicks (just like any standard window shadow)
* Does not affect the actual size of the host window
* Respects the "Show shadows under windows" option in Performance Options in Windows settings
* Style/ResourceDictionary-friendly and animatable

## Gallery

<p float='middle'>
  <img src='doc/Demo1.png' width='32%' />
  <img src='doc/Demo2.png' width='32%' />
  <img src='doc/Demo3.png' width='32%' />
</p>
