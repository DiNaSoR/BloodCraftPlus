using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Eclipse.Services;

/// <summary>
/// Optional integration point for the external VDebug plugin.
/// If the debug plugin is not installed, calls are no-ops (and do not error).
/// </summary>
internal static class DebugToolsBridge
{
    const string DebugToolsAssemblyName = "VDebug";
    const string DebugToolsApiTypeName = "VDebug.VDebugApi";

    static bool _loggedMissing;
    static MethodInfo _dumpMenuAssets;

    public static void TryDumpMenuAssets()
    {
        if (!TryResolveStaticMethod(nameof(TryDumpMenuAssets), "DumpMenuAssets", ref _dumpMenuAssets, out MethodInfo method))
        {
            LogMissingOnce();
            return;
        }

        try
        {
            method.Invoke(null, null);
        }
        catch (Exception ex)
        {
            Core.Log.LogWarning($"[VDebug] Failed invoking DumpMenuAssets: {ex.Message}");
        }
    }

    static bool TryResolveStaticMethod(string callSite, string methodName, ref MethodInfo cache, out MethodInfo method)
    {
        if (cache != null)
        {
            method = cache;
            return true;
        }

        method = null;

        Assembly apiAssembly = FindDebugToolsAssembly();
        if (apiAssembly == null)
        {
            return false;
        }

        Type apiType = apiAssembly.GetType(DebugToolsApiTypeName, throwOnError: false);
        if (apiType == null)
        {
            Core.Log.LogWarning($"[VDebug] Found assembly '{DebugToolsAssemblyName}', but '{DebugToolsApiTypeName}' was not found (callsite: {callSite}).");
            return false;
        }

        method = apiType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
        if (method == null)
        {
            Core.Log.LogWarning($"[VDebug] Found '{DebugToolsApiTypeName}', but method '{methodName}' was not found (callsite: {callSite}).");
            return false;
        }

        cache = method;
        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static Assembly FindDebugToolsAssembly()
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        for (int i = 0; i < assemblies.Length; i++)
        {
            Assembly assembly = assemblies[i];
            if (assembly == null)
            {
                continue;
            }

            AssemblyName name;
            try
            {
                name = assembly.GetName();
            }
            catch
            {
                continue;
            }

            if (name != null && string.Equals(name.Name, DebugToolsAssemblyName, StringComparison.Ordinal))
            {
                return assembly;
            }
        }

        for (int i = 0; i < assemblies.Length; i++)
        {
            Assembly assembly = assemblies[i];
            if (assembly == null)
            {
                continue;
            }

            try
            {
                if (assembly.GetType(DebugToolsApiTypeName, throwOnError: false) != null)
                {
                    return assembly;
                }
            }
            catch
            {
                continue;
            }
        }

        return null;
    }

    static void LogMissingOnce()
    {
        if (_loggedMissing)
        {
            return;
        }

        _loggedMissing = true;
        Core.Log.LogInfo("[VDebug] Debug plugin not installed; skipping.");
    }
}
