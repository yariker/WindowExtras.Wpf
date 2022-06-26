using System.ComponentModel;

namespace WindowExtras.Wpf.Helpers;

internal class DesignerHelper
{
    internal static bool IsInDesignMode { get; } =
        DesignerProperties.IsInDesignModeProperty.DefaultMetadata.DefaultValue is true ||
        LicenseManager.UsageMode == LicenseUsageMode.Designtime;
}
