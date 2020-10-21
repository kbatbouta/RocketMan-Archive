using System;
using System.Collections.Generic;
using System.Linq;
using HugsLib;
using RimWorld;
using UnityEngine;
using UnityEngine.XR;
using Verse;

namespace RocketMan.src
{
    public class RocketMod : Mod
    {
        public static RocketModSettings settings;

        public static int defaultValue = 0;

        public static bool resetInitiated = false;

        public static string defaultValueString = "5";

        public static List<StatSettings> statsSettings = new List<StatSettings>();

        public string searchString = "";

        static Vector2 scroll = Vector2.zero;
        static Rect view = Rect.zero;

        public class StatSettings : IExposable
        {
            public string stat;

            public int expireAfter = 0;

            public void ExposeData()
            {
                Scribe_Values.Look(ref stat, "statDef");
                Scribe_Values.Look(ref expireAfter, "expiryTime", 5);
            }
        }

        public override string SettingsCategory()
        {
            return "RocketMan";
        }

        public RocketMod(ModContentPack contentPack) : base(contentPack)
        {
            settings = GetSettings<RocketModSettings>();
        }

        public override void DoSettingsWindowContents(global::UnityEngine.Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);

            ReadStats();

            var listRect = new Rect(inRect.x, inRect.y + 10f, inRect.width, inRect.height - 50f);
            var contentRect = new Rect(0f, 0f, inRect.width - 20f, 50f * DefDatabase<StatDef>.AllDefs.Count() + 200);

            Widgets.BeginScrollView(listRect, ref scroll, contentRect, true);

            var listing = new Listing_Standard();

            listing.Begin(contentRect);

            listing.CheckboxLabeled("Enabled", ref Finder.enabled);
            listing.GapLine();

            if (Finder.enabled)
            {

                listing.CheckboxLabeled("Enable debuging", ref Finder.debug);
                listing.GapLine();

                listing.CheckboxLabeled("Enable adaptive mod", ref Finder.learning);
                listing.GapLine();

                listing.Label(string.Format("Clear all stored data in an interval of {0} tikcs", Finder.universalCacheAge));
                Finder.universalCacheAge = (int)listing.Slider(Finder.universalCacheAge, 500, Finder.debug ? 10000 : 2000);

                listing.GapLine();

                listing.Label("Custom value");

                listing.TextFieldNumeric(ref defaultValue, ref defaultValueString, 0f, 15f);
                if (!resetInitiated && listing.ButtonText("Apply Custom value to all!"))
                {
                    resetInitiated = true;
                }

                if (resetInitiated && listing.ButtonText("Cancel"))
                {
                    resetInitiated = false;
                }

                if (resetInitiated && listing.ButtonText("Are you sure? YES!"))
                {
                    Reset();
                }

                listing.GapLine();

                DrawStatDefs(listing);

                listing.GapLine();
            }

            listing.End();
            Widgets.EndScrollView();
        }

        public void DrawStatDefs(Listing_Standard listing)
        {
            UpdateStats();

            var counter = 0;

            listing.Gap();
            listing.Label("Search:");

            searchString = listing.TextEntry(searchString, 1).ToLower();

            listing.Gap();

            foreach (StatSettings settings in statsSettings)
            {
                if (false
                    || searchString.Trim() == ""
                    || settings.stat.ToLower().Contains(searchString))
                {
                    listing.Label(string.Format("{0}. {1} set to expire in \t {2} ticks", counter++, settings.stat, settings.expireAfter));
                    settings.expireAfter = (int)listing.Slider(settings.expireAfter, 0, 255);
                }
            }

            foreach (StatSettings setting in statsSettings)
            {
                Finder.statExpiry[DefDatabase<StatDef>.defsByName[setting.stat].index] = (byte)setting.expireAfter;
            }

            WriteSettings();
        }

        public override void WriteSettings()
        {
            UpdateStats();
            base.WriteSettings();
        }

        public static void ReadStats()
        {
            if (statsSettings == null || statsSettings.Count == 0)
            {
                return;
            }

            foreach (StatSettings setting in statsSettings)
            {
                setting.expireAfter = Finder.statExpiry[DefDatabase<StatDef>.defsByName[setting.stat].index];
            }
        }

        public static void Reset()
        {
            var defs = DefDatabase<StatDef>.AllDefs;

            statsSettings.Clear();
            foreach (StatDef def in defs)
            {
                statsSettings.Add(new StatSettings() { stat = def.defName, expireAfter = def.defName.PredictValueFromString() + defaultValue });
            }

            var failed = false;
            foreach (StatSettings setting in statsSettings)
            {
                if (setting?.stat == null)
                {
                    failed = true;
                    break;
                }

                Finder.statExpiry[DefDatabase<StatDef>.defsByName[setting.stat].index] = (byte)setting.expireAfter;
            }

            if (failed)
            {
                Log.Warning("Failed to reindex the statDef database");
                statsSettings.Clear();

                UpdateStats();
            }
        }

        public static void UpdateStats()
        {
            if (statsSettings == null)
            {
                statsSettings = new List<StatSettings>();
            }

            var defs = DefDatabase<StatDef>.AllDefs;

            if (statsSettings.Count != defs.Count())
            {
                statsSettings.Clear();
                foreach (StatDef def in defs)
                {
                    statsSettings.Add(new StatSettings() { stat = def.defName, expireAfter = def.defName.PredictValueFromString() + defaultValue });

                }
            }

            var failed = false;
            foreach (StatSettings setting in statsSettings)
            {
                if (setting?.stat == null)
                {
                    failed = true;
                    break;
                }

                Finder.statExpiry[DefDatabase<StatDef>.defsByName[setting.stat].index] = (byte)setting.expireAfter;
            }

            if (failed)
            {
                Log.Warning("Failed to reindex the statDef database");
                statsSettings.Clear();

                UpdateStats();
            }
        }

        public class RocketModSettings : ModSettings
        {
            public override void ExposeData()
            {
                base.ExposeData();

                if (Scribe.mode == LoadSaveMode.LoadingVars)
                {
                    ReadStats();
                }

                Scribe_Values.Look<bool>(ref Finder.enabled, "enabled", true);
                Scribe_Values.Look<bool>(ref Finder.learning, "learning", false);
                Scribe_Values.Look<bool>(ref Finder.debug, "debug", false);

                Scribe_Values.Look<int>(ref Finder.ageOfGetValueUnfinalizedCache, "ageOfGetValueUnfinalizedCache", 0);
                Scribe_Values.Look<int>(ref Finder.universalCacheAge, "universalCacheAge", 0);

                Scribe_Collections.Look(ref statsSettings, "statsSettings", LookMode.Deep);
            }
        }
    }
}
