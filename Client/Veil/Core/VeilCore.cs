using Veil.Services;

namespace Veil.Core;

/// <summary>
/// Core initialization and lifecycle management for Veil.
/// </summary>
public static class VeilCore
{
    private static bool _initialized;

    /// <summary>
    /// Whether Veil has been initialized and is ready.
    /// </summary>
    public static bool IsInitialized => _initialized;

    /// <summary>
    /// Initialize all Veil systems.
    /// </summary>
    public static void Initialize()
    {
        Log.Trace("VeilCore.Initialize");

        if (_initialized)
        {
            Log.Warning("VeilCore", "Already initialized, skipping.");
            return;
        }

        Log.Info("VeilCore", "Initializing Veil Core...");

        // Initialize layout service (load saved positions)
        Log.Debug("VeilCore", "Initializing LayoutService...");
        Persistence.LayoutService.Initialize();

        // Initialize asset loader
        Log.Debug("VeilCore", "Initializing AssetLoader...");
        AssetLoader.Initialize();

        // Initialize chat sender
        Log.Debug("VeilCore", "Initializing ChatSender...");
        Patches.ChatSender.Initialize();

        // Initialize adapter manager (discovers and loads adapters)
        Log.Debug("VeilCore", "Initializing AdapterManager...");
        AdapterManager.Initialize();

        // Initialize UI orchestrator
        Log.Debug("VeilCore", "Initializing UIOrchestrator...");
        UIOrchestrator.Initialize();

        // Initialize our own independent canvas (not hooked to game UI)
        Log.Debug("VeilCore", "Initializing CanvasManager (independent canvas)...");
        CanvasManager.Initialize();

        // Initialize input patch (F1 toggle - using Harmony for IL2CPP compatibility)
        Log.Debug("VeilCore", "Initializing InputPatch...");
        Patches.InputPatch.Initialize();

        _initialized = true;
        Log.Info("VeilCore", "Veil Core initialized. Press F1 to toggle UI.");
        Log.TraceExit("VeilCore.Initialize");
    }

    /// <summary>
    /// Shutdown all Veil systems.
    /// </summary>
    public static void Shutdown()
    {
        Log.Trace("VeilCore.Shutdown");

        if (!_initialized)
        {
            Log.Debug("VeilCore", "Not initialized, nothing to shutdown.");
            return;
        }

        Log.Info("VeilCore", "Shutting down Veil...");

        CanvasManager.Shutdown();
        UIOrchestrator.Shutdown();
        AdapterManager.Shutdown();
        AssetLoader.Shutdown();

        _initialized = false;
        Log.Info("VeilCore", "Veil shutdown complete.");
        Log.TraceExit("VeilCore.Shutdown");
    }
}
