using System;
using System.Collections.Generic;

namespace Rocketeer
{
    public static class Context
    {
        public static int reportIdCounter = 0;
        public static Dictionary<int, RocketeerReport> reports = new Dictionary<int, RocketeerReport>();
        public static Dictionary<string, RocketeerReport> reportsByMethodPath = new Dictionary<string, RocketeerReport>();

        public static object reportDictLocker = new object();
        public static object reportLocker = new object();
    }
}
