using BepInEx.Logging;

namespace VDebug;

internal static class VDebugLog
{
    static ManualLogSource _log;

    public static ManualLogSource Log => _log ??= Logger.CreateLogSource("VDebug");

    public static void SetLog(ManualLogSource logSource)
    {
        _log = logSource;
    }
}

