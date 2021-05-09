using System;
using System.Reflection;
using HarmonyLib;

namespace RocketLite
{
    public static class PatchingUtility
    {
        public static bool IsMethodEmpty(this MethodBase method)
        {
            return method.GetMethodBody()?.GetILAsByteArray()?.Length <= 1;
        }

        public static bool IsValidTarget(this MethodBase method)
        {
            return method != null && method.HasMethodBody() && !method.IsMethodEmpty() && !method.IsAbstract;
        }

        public static string GetMethodPath(this MethodBase method)
        {
            return string.Format("{0}.{1}:{2}", method.DeclaringType.Namespace, method.ReflectedType.Name, method.Name);
        }
    }
}
