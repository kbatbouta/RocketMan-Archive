using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RimWorld;
using RocketMan;
using RocketMan.Tabs;
using Verse;

namespace Proton
{
    public class ProtonSettings : IExposable
    {
        public float executionTimeLimit = 25f;

        public float minInterval = 25f;

        public void ExposeData()
        {
            List<AlertSettings> alertsSettings = Context.alertSettingsByIndex?.ToList() ?? new List<AlertSettings>();
            Scribe_Collections.Look(ref alertsSettings, "settings", LookMode.Deep);
            Scribe_Values.Look(ref executionTimeLimit, "executionTimeLimit", 25f);
            Scribe_Values.Look(ref minInterval, "minInterval", 2f);
            if (Scribe.mode != LoadSaveMode.Saving && alertsSettings != null)
            {
                foreach (var s in alertsSettings)
                {
                    Context.typeIdToSettings[s.typeId] = s;
                }
            }
        }
    }
}
