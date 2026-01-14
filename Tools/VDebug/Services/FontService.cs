using BepInEx;
using System.IO;
using TMPro;
using UnityEngine;

namespace VDebug.Services;

public static class FontService
{
    private static TMP_FontAsset _customFont;
    private static bool _triedLoading;

    public static TMP_FontAsset GetFont()
    {
        if (_customFont != null) return _customFont;
        if (_triedLoading) return null; // Fallback to default if already tried

        _triedLoading = true;
        
        try
        {
            // 1. Try to load from "vdebug.bundle" in plugin folder (Best for custom/distributed fonts)
            // This bypasses OS font stripping issues by using Unity's native AssetBundle format
            string bundlePath = Path.Combine(Paths.PluginPath, "vdebug.bundle");
            if (!File.Exists(bundlePath))
                bundlePath = Path.Combine(Paths.PluginPath, "VDebug", "vdebug.bundle");

            if (File.Exists(bundlePath))
            {
                 try 
                 {
                     VDebugLog.Log.LogInfo($"[VDebug] Loading AssetBundle from: {bundlePath}");
                     AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
                     if (bundle != null)
                     {
                         // Try loading TMP_FontAsset first (pre-cooked)
                         var assets = bundle.LoadAllAssets<TMP_FontAsset>();
                         if (assets != null && assets.Length > 0)
                         {
                             _customFont = assets[0];
                             VDebugLog.Log.LogInfo($"[VDebug] Loaded FontAsset from bundle: {_customFont.name}");
                             return _customFont;
                         }

                         // Try loading raw Font and converting
                         var fonts = bundle.LoadAllAssets<Font>();
                         if (fonts != null && fonts.Length > 0)
                         {
                             _customFont = TMP_FontAsset.CreateFontAsset(fonts[0]);
                             VDebugLog.Log.LogInfo($"[VDebug] Created FontAsset from bundle font: {fonts[0].name}");
                             return _customFont;
                         }
                         
                         bundle.Unload(false); // Unload metadata but keep assets? Actually better keep loaded or manage lifecycle.
                         // For a singleton service, keeping it loaded is fine.
                     }
                 }
                 catch (Exception ex)
                 {
                     VDebugLog.Log.LogWarning($"[VDebug] Failed to load bundle: {ex.Message}");
                 }
            }

            // 2. Try to find the configured font among globally loaded Game Assets (Resources)
            // This is the robust method since OS font loading is stripped.
            
            // Default preference if user hasn't configued anything: NotoSansMono (cleaner for debug)
            string targetName = Plugin.CustomFontName?.Value;
            if (string.IsNullOrEmpty(targetName)) targetName = "NotoSansMono-Regular"; // Good default if available

            VDebugLog.Log.LogInfo($"[VDebug] Searching in-game assets for font: '{targetName}'...");

            // Search TMP Assets first
            var allTmpAssets = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            
            foreach (var tmp in allTmpAssets)
            {
                if (tmp.name.Contains(targetName, StringComparison.OrdinalIgnoreCase))
                {
                    VDebugLog.Log.LogInfo($"[VDebug] Found match in TMP assets: {tmp.name}");
                    _customFont = tmp;
                    return _customFont;
                }
            }

            // Search Raw Fonts second
            var allRawFonts = Resources.FindObjectsOfTypeAll<Font>();
            
            foreach (var f in allRawFonts)
            {
                if (f.name.Contains(targetName, StringComparison.OrdinalIgnoreCase))
                {
                    VDebugLog.Log.LogInfo($"[VDebug] Found match in raw fonts: {f.name}");
                    _customFont = TMP_FontAsset.CreateFontAsset(f);
                    return _customFont;
                }
            }
            
            // --- DEBUG LOGGING REMOVED ---
            // If debug is needed, uncomment or add a debug config flag
            // VDebugLog.Log.LogInfo($"[VDebug] Found {allTmpAssets.Length} TMP assets and {allRawFonts.Length} raw fonts.");

            // 3. Fallback to Unity built-in Arial (last resort)
            Font builtin = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (builtin != null)
            {
                VDebugLog.Log.LogInfo("[VDebug] Using built-in Unity Arial font.");
                _customFont = TMP_FontAsset.CreateFontAsset(builtin);
                return _customFont;
            }


        }
        catch (System.Exception ex)
        {
            VDebugLog.Log.LogWarning($"[VDebug] Failed to load custom font: {ex.Message}");
        }

        return null;
    }
}
