using System;
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

        public static RocketeerReport GetReport(int reportId)
        {
            RocketeerReport report;
            lock (Context.reportDictLocker)
                report = Context.reports[reportId];
            return report;
        }
    }
}
