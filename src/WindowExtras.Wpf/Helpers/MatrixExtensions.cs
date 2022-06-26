using System;
using System.Windows;
using System.Windows.Media;

namespace WindowExtras.Wpf.Helpers;

internal static class MatrixExtensions
{
    internal static void Transform(this Matrix matrix, Span<Point> points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = matrix.Transform(points[i]);
        }
    }
}
