using System;

namespace VDebug.Services;

internal static class AnsiColors
{
    // ANSI escape codes. These are best-effort and can be disabled via config.
    const string Reset = "\u001b[0m";

    // 256-color foreground: \x1b[38;5;{n}m
    static string Fg256(int n) => $"\u001b[38;5;{n}m";

    // Sources (distinct colors)
    static readonly string ClientTagColor = Fg256(39);   // vivid blue
    static readonly string ServerTagColor = Fg256(208);  // orange
    static readonly string DefaultTagColor = Fg256(250); // light gray

    // Levels (severity colors)
    static readonly string WarnColor = Fg256(220);  // yellow
    static readonly string ErrorColor = Fg256(196); // red

    public enum VLevel
    {
        Info,
        Warning,
        Error
    }

    public static string FormatPrefix(string source)
    {
        string tag = ResolveTag(source, out string tagColor);
        return $"{tagColor}{tag}{Reset}";
    }

    public static string FormatMessage(VLevel level, string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return message;
        }

        return level switch
        {
            VLevel.Warning => $"{WarnColor}{message}{Reset}",
            VLevel.Error => $"{ErrorColor}{message}{Reset}",
            _ => message
        };
    }

    static string ResolveTag(string source, out string color)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            color = DefaultTagColor;
            return "[VDebug]";
        }

        // These are the conventions used by Bloodcraft/Eclipse bridges.
        if (source.Contains("Client", StringComparison.OrdinalIgnoreCase))
        {
            color = ClientTagColor;
            return "[Client]";
        }

        if (source.Contains("Server", StringComparison.OrdinalIgnoreCase))
        {
            color = ServerTagColor;
            return "[Server]";
        }

        color = DefaultTagColor;
        return "[VDebug]";
    }
}

