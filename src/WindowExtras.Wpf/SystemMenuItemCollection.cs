using System.Windows;

namespace WindowExtras.Wpf;

public class SystemMenuItemCollection : FreezableCollection<SystemMenuItem>
{
    /// <inheritdoc />
    protected override Freezable CreateInstanceCore() => new SystemMenuItemCollection();
}
