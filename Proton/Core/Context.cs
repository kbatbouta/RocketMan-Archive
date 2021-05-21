using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Proton
{
    public static class Context
    {
        public static ProtonSettings settings;
        public static Dictionary<string, AlertSettings> typeIdToSettings = new Dictionary<string, AlertSettings>();
        public static AlertsReadout readoutInstance;
        public static AlertSettings[] alertsSettings;
        public static Alert[] alerts;
    }
}
