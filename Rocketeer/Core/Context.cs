using System;
using System.Collections.Generic;

namespace Rocketeer
{
    public static class Context
    {
        public static int reportIdCounter = 0;
        public static readonly HashSet<string> patchedMethods = new HashSet<string>();

        public static readonly Dictionary<int, RocketeerReport> reports = new Dictionary<int, RocketeerReport>();
        public static readonly Dictionary<string, RocketeerReport> reportsByMethodPath = new Dictionary<string, RocketeerReport>();

        public static object reportDictLocker = new object();
        public static object reportLocker = new object();
    }
}
