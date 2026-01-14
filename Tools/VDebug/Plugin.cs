using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using UnityEngine;

namespace VDebug;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
internal class Plugin : BasePlugin
{
    internal static Plugin Instance { get; private set; }
    internal static ManualLogSource LogInstance => Instance.Log;

    public override void Load()
    {
        Instance = this;
        VDebugLog.SetLog(Log);

        if (Application.productName == "VRisingServer")
        {
            Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME}[{MyPluginInfo.PLUGIN_VERSION}] is a client debug plugin; skipping on {Application.productName}.");
            return;
        }

        // Initialize the debug panel (will be created when API is called or automatically)
        // Panel initialization is deferred until a canvas is available
        Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME}[{MyPluginInfo.PLUGIN_VERSION}] loaded. Call VDebugApi.ShowDebugPanel() to activate.");
    }
}
