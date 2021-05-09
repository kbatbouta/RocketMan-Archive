using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Rocketeer
{
    public static class Tools
    {
        public static string TrimMethodName(this string name)
        {
            if (name.StartsWith("get_")) name.Substring("get_".Length);
            return name;
        }

        public static RocketeerReport GetReportById(int reportId)
        {
            RocketeerReport report;
            lock (Context.reportDictLocker)
                report = Context.reports[reportId];
            return report;
        }

        public static string GetMethodPath(this MethodBase method)
        {
            var type = method.DeclaringType;
            var space = type.Namespace;
            return $"{space}.{type.Name}:{method.Name}";
        }
    }
}
