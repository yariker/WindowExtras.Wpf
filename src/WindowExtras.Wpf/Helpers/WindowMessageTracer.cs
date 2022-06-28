using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Windows.Interop;
using Windows.Win32;

namespace WindowExtras.Wpf.Helpers;

internal sealed class WindowMessageTracer : IDisposable
{
    private static readonly Type MessageType = typeof(uint);
    private static readonly Dictionary<int, string> KnownMessages = new();

    private static readonly HashSet<int> IgnoreMessages = new()
    {
        (int)PInvoke.WM_MOUSEMOVE,
        (int)PInvoke.WM_MOUSELEAVE,
        (int)PInvoke.WM_NCMOUSEMOVE,
        (int)PInvoke.WM_NCMOUSELEAVE,
        (int)PInvoke.WM_NCHITTEST,
        (int)PInvoke.WM_CT_BOTTOM_FIELD_FIRST,
        (int)PInvoke.WM_GETTEXT,
    };

    private readonly StringBuilder _stringBuilder = new();
    private readonly HwndSource _hwndSource;

    static WindowMessageTracer()
    {
        var type = typeof(PInvoke);
        var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Static);

        foreach (var field in fields)
        {
            if (field.Name.StartsWith("WM_") && field.IsLiteral && field.FieldType == MessageType)
            {
                KnownMessages[Convert.ToInt32(field.GetRawConstantValue())] = field.Name;
            }
        }
    }

    public WindowMessageTracer(nint hwnd)
    {
        if (HwndSource.FromHwnd(hwnd) is not HwndSource hwndSource)
        {
            throw new ArgumentException("Invalid window handle.", nameof(hwnd));
        }

        _hwndSource = hwndSource;
        _hwndSource.AddHook(WindowHook);
    }

    private nint WindowHook(nint hwnd, int msg, nint wparam, nint lparam, ref bool handled)
    {
        if (!IgnoreMessages.Contains(msg))
        {
            _stringBuilder.Clear();
            _stringBuilder.AppendFormat("{0:hh:mm:ss.fff}: ", DateTime.Now);

            if (KnownMessages.TryGetValue(msg, out string? message))
            {
                _stringBuilder.Append(message);
            }
            else
            {
                _stringBuilder.AppendFormat("{0:X4}", msg);
            }

            Trace.WriteLine(_stringBuilder.ToString());
        }

        return 0;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _hwndSource.RemoveHook(WindowHook);
    }
}
