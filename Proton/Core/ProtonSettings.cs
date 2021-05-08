using System;
using System.Collections.Generic;
using RocketMan.Tabs;
using Verse;

namespace Proton
{
    public class ThingDefSettings : IExposable
    {
        public ThingDef thingDef;
        public string thingDefName;
        public bool isCritical = true;
        public bool isRecyclable = false;

        public bool IsJunk
        {
            get => !isCritical;
        }

        public ThingDefSettings()
        {
        }

        public ThingDefSettings(string thingDefName)
        {
            this.thingDefName = thingDefName;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref thingDefName, "thingDefName");
            Scribe_Values.Look(ref isCritical, "isCritical");
            Scribe_Values.Look(ref isRecyclable, "isRecyclable");
        }

        public void ResolveContent()
        {
            if (DefDatabase<ThingDef>.defsByName.TryGetValue(this.thingDefName, out var def))
                this.thingDef = def;
        }

        public void Cache()
        {
            if (this.thingDef == null) return;
            Context.thingSettingsByDef[thingDef] = this;
            Context.thingJunkByDef[thingDef.index] = !isCritical;
        }
    }

    public class ProtonSettings : IExposable
    {
        public List<ThingDefSettings> thingDefSettings = new List<ThingDefSettings>();

        public void ExposeData()
        {
            Scribe_Collections.Look(ref thingDefSettings, "thingDefSettings", LookMode.Deep);
            if (thingDefSettings == null) thingDefSettings = new List<ThingDefSettings>();
        }
    }

    [ProtonPatch(typeof(TabContent_Proton), nameof(TabContent_Proton.DoExtras))]
    public static class ProtonSettingsGUIUtility
    {
        public static void Postfix()
        {
        }
    }
}
