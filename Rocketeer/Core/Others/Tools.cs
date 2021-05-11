using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RocketMan;
using Verse;

namespace Rocketeer
{
    public static class Tools
    {
        public static bool IsPatched(this MethodBase method)
        {
            return Context.patchedMethods.Contains(method.GetUniqueMethodIdentifier());
        }

        public static bool TryGetRocketeerMethodTracker(this MethodBase method, out RocketeerMethodTracker report)
        {
            string id = method.GetUniqueMethodIdentifier();
            return Context.trackerByUniqueIdentifier.TryGetValue(id, out report);
        }

        public static string GetDeclaredTypeMethodPath(this MethodBase method)
        {
            var type = method.DeclaringType;
            return $"{type.Namespace}.{type.Name}:{method.Name}";
        }

        public static string GetReflectedTypeMethodPath(this MethodBase method)
        {
            var type = method.ReflectedType;
            return $"{type.Namespace}.{type.Name}:{method.Name}";
        }

        public static string GetUniqueMethodIdentifier(this MethodBase method)
        {
            return $"{method.GetDeclaredTypeMethodPath()}&{method.GetReflectedTypeMethodPath()}";
        }

        public static bool IsValidMethodPath(this string methodPath)
        {
            methodPath = methodPath.Trim();
            if (methodPath.Count(c => c == ' ' || c == '\t' || c == '\n') > 0)
                return false;
            try
            {
                if (AccessTools.Method(methodPath) is MethodBase method && method != null && method.IsValidTarget())
                    return true;
            }
            catch { }
            return false;
        }

        public static string TrimMethodName(this string name)
        {
            if (name.StartsWith("get_")) name.Substring("get_".Length);
            return name;
        }

        public static string[] GetStackTraceAsString(this Exception exception)
        {
            StackTrace trace = new StackTrace(exception);
            StackFrame frame;
            string[] frames = new string[trace.FrameCount];
            for (int i = 0; i < trace.FrameCount; i++)
            {
                frame = trace.GetFrame(i);
                frames[i] = $"method:{frame.GetMethod().GetDeclaredTypeMethodPath()}\t" +
                    $"file:{frame.GetFileName()}\t" +
                    $"line:{frame.GetFileLineNumber()}\t" +
                    $"offset:{frame.GetILOffset()}\t";
            }
            return frames;
        }

        private static readonly Dictionary<MethodBase, string> _packageIdCache = new Dictionary<MethodBase, string>();

        public static string GetModPackageId(this MethodBase method)
        {
            if (!(method?.IsValidTarget() ?? false))
                return null;
            if (_packageIdCache.TryGetValue(method, out string value))
                return value;
            Assembly assembly = method.DeclaringType.Assembly;
            return _packageIdCache[method] = LoadedModManager.RunningMods.First(m => m.assemblies?.loadedAssemblies?.Contains(assembly) ?? false)?.PackageId ?? null;
        }
    }
}
