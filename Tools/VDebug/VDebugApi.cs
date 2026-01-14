using System;
using VDebug.Services;

namespace VDebug;

/// <summary>
/// Public static API surface that other plugins can call via reflection.
/// Keep this type name stable.
/// </summary>
public static class VDebugApi
{
    public const int ApiVersion = 2;

    #region Logging

    public static void LogInfo(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        VDebugLog.Log.LogInfo(message);
    }

    public static void LogWarning(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        VDebugLog.Log.LogWarning(message);
    }

    public static void LogError(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        VDebugLog.Log.LogError(message);
    }

    #endregion

    #region Asset Dumping

    public static void DumpMenuAssets()
    {
        try
        {
            AssetDumpService.DumpMenuAssets();
        }
        catch (Exception ex)
        {
            VDebugLog.Log.LogWarning($"[VDebugApi] DumpMenuAssets failed: {ex}");
        }
    }

    public static void DumpCharacterMenu()
    {
        try
        {
            AssetDumpService.DumpCharacterMenu();
        }
        catch (Exception ex)
        {
            VDebugLog.Log.LogWarning($"[VDebugApi] DumpCharacterMenu failed: {ex}");
        }
    }

    public static void DumpHudMenu()
    {
        try
        {
            AssetDumpService.DumpHudMenu();
        }
        catch (Exception ex)
        {
            VDebugLog.Log.LogWarning($"[VDebugApi] DumpHudMenu failed: {ex}");
        }
    }

    public static void DumpMainMenu()
    {
        try
        {
            AssetDumpService.DumpMainMenu();
        }
        catch (Exception ex)
        {
            VDebugLog.Log.LogWarning($"[VDebugApi] DumpMainMenu failed: {ex}");
        }
    }

    #endregion

    #region Debug Panel Control

    /// <summary>
    /// Show the debug panel. Initializes it if not already created.
    /// </summary>
    public static void ShowDebugPanel()
    {
        try
        {
            DebugPanelService.Initialize();
            DebugPanelService.ShowPanel();
        }
        catch (Exception ex)
        {
            VDebugLog.Log.LogWarning($"[VDebugApi] ShowDebugPanel failed: {ex}");
        }
    }

    /// <summary>
    /// Hide the debug panel.
    /// </summary>
    public static void HideDebugPanel()
    {
        try
        {
            DebugPanelService.HidePanel();
        }
        catch (Exception ex)
        {
            VDebugLog.Log.LogWarning($"[VDebugApi] HideDebugPanel failed: {ex}");
        }
    }

    /// <summary>
    /// Toggle debug panel visibility.
    /// </summary>
    public static void ToggleDebugPanel()
    {
        try
        {
            DebugPanelService.Initialize();
            DebugPanelService.TogglePanel();
        }
        catch (Exception ex)
        {
            VDebugLog.Log.LogWarning($"[VDebugApi] ToggleDebugPanel failed: {ex}");
        }
    }

    #endregion
}
