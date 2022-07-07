using System.Windows;

namespace WindowExtras.Wpf.Menu;

/// <summary>
/// Represents a collection of <see cref="MenuItem"/> objects.
/// </summary>
public class MenuItemCollection : FreezableCollection<MenuItem>
{
    /// <inheritdoc />
    protected override Freezable CreateInstanceCore() => new MenuItemCollection();
}
