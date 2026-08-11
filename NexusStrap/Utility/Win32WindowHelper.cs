using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NexusStrap.Utility;

internal static class Win32WindowHelper
{
    private const uint WM_SETICON = 0x0080;
    private const int ICON_SMALL = 0;
    private const int ICON_BIG = 1;
    private const int GCLP_HICON = -14;

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowText(IntPtr hWnd, string lpString);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string? lpszClass, string? lpszWindow);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, nuint wParam, nint lParam);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr SetClassLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    public static void ApplyWindowTitleAndIcon(int processId, string title)
    {
        try
        {
            nint hIcon = GetNexusIcon();

            IntPtr hWnd = IntPtr.Zero;

            while (true)
            {
                hWnd = FindWindowEx(IntPtr.Zero, hWnd, null, null);

                if (hWnd == IntPtr.Zero)
                    break;

                GetWindowThreadProcessId(hWnd, out uint pid);

                if (pid == (uint)processId)
                {
                    SetWindowText(hWnd, title);

                    if (hIcon != IntPtr.Zero)
                    {
                        SetClassLongPtr(hWnd, GCLP_HICON, hIcon);
                        SendMessage(hWnd, WM_SETICON, (nuint)ICON_SMALL, hIcon);
                        SendMessage(hWnd, WM_SETICON, (nuint)ICON_BIG, hIcon);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to set window title/icon: {ex.Message}");
        }
    }

    private static nint GetNexusIcon()
    {
        try
        {
            string? exePath = Environment.ProcessPath;
            if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
            {
                var icon = Icon.ExtractAssociatedIcon(exePath);
                return icon?.Handle ?? IntPtr.Zero;
            }
            return IntPtr.Zero;
        }
        catch
        {
            return IntPtr.Zero;
        }
    }
}
