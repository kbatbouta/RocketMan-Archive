using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Verse;

namespace RocketMan
{
    public static class PatchingUtility
    {
        public static bool IsValidTarget(this MethodBase method)
        {
            return method != null && !method.IsAbstract && method.DeclaringType == method.ReflectedType && method.HasMethodBody() && method.GetMethodBody()?.GetILAsByteArray()?.Length > 1;
        }

        public static string GetMethodPath(this MethodBase method)
        {
            return string.Format("{0}.{1}:{2}", method.DeclaringType.Namespace, method.ReflectedType.Name, method.Name);
        }
    }
}
