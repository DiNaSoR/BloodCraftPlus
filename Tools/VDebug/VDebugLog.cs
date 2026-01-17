using BepInEx.Logging;
using System.Collections.Concurrent;

namespace VDebug;

internal static class VDebugLog
{
    static ManualLogSource _log;
    static readonly ConcurrentDictionary<string, ManualLogSource> _namedLogs = new();

    public static ManualLogSource Log => _log ??= Logger.CreateLogSource("VDebug");

    public static ManualLogSource GetLog(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Log;
        }

        return _namedLogs.GetOrAdd(name, static n => Logger.CreateLogSource(n));
    }

    public static void SetLog(ManualLogSource logSource)
    {
        _log = logSource;
    }
}

