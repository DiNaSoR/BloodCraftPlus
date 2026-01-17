using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Bloodcraft.Services;

/// <summary>
/// Optional integration point for the external VDebug plugin.
/// If VDebug is not installed, calls fall back to Core.Log.
/// </summary>
internal static class DebugToolsBridge
{
    const string DebugToolsAssemblyName = "VDebug";
    const string DebugToolsApiTypeName = "VDebug.VDebugApi";
    const string ServerLogSource = "VDebug - Server";

    static MethodInfo _logInfo;
    static MethodInfo _logWarning;
    static MethodInfo _logError;

    public static void TryLogInfo(string message) => TryLog("LogInfo", message, ref _logInfo, fallback: () => Core.Log.LogInfo(message));
    public static void TryLogWarning(string message) => TryLog("LogWarning", message, ref _logWarning, fallback: () => Core.Log.LogWarning(message));
    public static void TryLogError(string message) => TryLog("LogError", message, ref _logError, fallback: () => Core.Log.LogError(message));

    static void TryLog(string methodName, string message, ref MethodInfo cache, Action fallback)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        // Prefer newer API: LogX(string source, string message). Fall back to legacy LogX(string message).
        if (!TryResolveStaticMethod(methodName, new[] { typeof(string), typeof(string) }, ref cache, out MethodInfo method))
        {
            MethodInfo legacyCache = null;
            if (!TryResolveStaticMethod(methodName, new[] { typeof(string) }, ref legacyCache, out MethodInfo legacyMethod))
            {
                fallback?.Invoke();
                return;
            }

            try
            {
                legacyMethod.Invoke(null, new object[] { message });
            }
            catch
            {
                fallback?.Invoke();
            }

            return;
        }

        try
        {
            method.Invoke(null, new object[] { ServerLogSource, message });
        }
        catch
        {
            fallback?.Invoke();
        }
    }

    static bool TryResolveStaticMethod(string methodName, Type[] parameterTypes, ref MethodInfo cache, out MethodInfo method)
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
            return false;
        }

        method = apiType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, null, parameterTypes, null);
        if (method == null)
        {
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
}

