using System.Windows;
using System.Windows.Media;

namespace WindowExtras.Wpf.Helpers;

internal static class MatrixExtensions
{
    internal static unsafe void Transform(this Matrix matrix, Point* points, int count)
    {
        for (int i = 0; i < count; i++)
        {
            points[i] = matrix.Transform(points[i]);
        }
    }
}
