using System;
using System.Runtime.InteropServices;

namespace VDebug.Services;

/// <summary>
/// Best-effort enabling of ANSI escape processing in Windows console hosts (ConHost/Windows Terminal).
/// Without this, ANSI sequences will print literally (e.g. "[38;5;39m").
/// </summary>
internal static class ConsoleAnsiSupport
{
    const int STD_OUTPUT_HANDLE = -11;
    const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
    const uint ENABLE_PROCESSED_OUTPUT = 0x0001;

    static bool _attempted;
    static bool _enabled;

    public static bool IsEnabled => _enabled;

    public static void TryEnable()
    {
        if (_attempted)
        {
            return;
        }

        _attempted = true;

        try
        {
            IntPtr handle = GetStdHandle(STD_OUTPUT_HANDLE);
            if (handle == IntPtr.Zero || handle == new IntPtr(-1))
            {
                return;
            }

            if (!GetConsoleMode(handle, out uint mode))
            {
                return;
            }

            uint newMode = mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING | ENABLE_PROCESSED_OUTPUT;
            if (!SetConsoleMode(handle, newMode))
            {
                return;
            }

            _enabled = true;
        }
        catch
        {
            // Keep VDebug fail-safe.
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
}

