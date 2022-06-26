# WindowExtras for WPF

This .NET library makes very it easy to attach a shadow to a standard WPF window without having to override the window template.
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

    <winex:WindowShadow.Shadow>
        <winex:WindowShadow OffsetY="15" Opacity="0.3" Radius="30" />
    </winex:WindowShadow.Shadow>

    <Button Width="100" Height="32" Click="ButtonClicked" Content="Close">
        <x:Code><![CDATA[void ButtonClicked(object sender, RoutedEventArgs e) => Close();]]></x:Code>
    </Button>

</Window>
```

# Gallery

<p float='middle'>
  <img src='doc/Demo1.png' width='32%' />
  <img src='doc/Demo2.png' width='32%' />
  <img src='doc/Demo3.png' width='32%' />
</p>
