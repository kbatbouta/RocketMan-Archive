using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Rocketeer
{
    public static class Tools
    {
        public static bool HasRocketeerPatch(this MethodBase method)
        {
            return Context.patchedMethods.Contains(method.GetMethodPath());
        }

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

        public static string GetReflectedTypeMethodPath(this MethodBase method)
        {
            var type = method.ReflectedType;
            return $"{type.Namespace}.{type.Name}:{method.Name}";
        }

        public static string GetUniqueMethodIdentifier(this MethodBase method)
        {
            var type = method.ReflectedType;
            return $"{method.GetMethodPath()}&{method.GetReflectedTypeMethodPath()}";
        }

        public static string[] GetStackTraceAsString(this Exception exception)
        {
            StackTrace trace = new StackTrace(exception);
            StackFrame frame;
            string[] frames = new string[trace.FrameCount];
            for (int i = 0; i < trace.FrameCount; i++)
            {
                frame = trace.GetFrame(i);
                frames[i] = $"method:{frame.GetMethod().GetMethodPath()}\t" +
                    $"file:{frame.GetFileName()}\t" +
                    $"line:{frame.GetFileLineNumber()}\t" +
                    $"offset:{frame.GetILOffset()}\t";
            }
            return frames;
        }
    }
}
