using System;

namespace WindowExtras.Wpf;

internal static class MathUtils
{
    internal static int ToInt32(this double value)
    {
        // Matches WPF algorithm of conversion from double to int.
        return (int)Math.Round(value, MidpointRounding.AwayFromZero);
    }
}