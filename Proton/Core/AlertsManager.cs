using System;
using RimWorld;

namespace Proton
{
    public static class AlertsManager
    {
        public static AlertsReadout readoutInstance;
        public static Alert[] alerts;

        public static void Initialize(AlertsReadout readoutInstance)
        {
            AlertsManager.readoutInstance = readoutInstance;
        }
    }
}
