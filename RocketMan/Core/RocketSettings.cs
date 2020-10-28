using System;
using System.Collections.Generic;
using System.Linq;
using HugsLib;
using RimWorld;
using UnityEngine;
using UnityEngine.XR;
using Verse;

namespace RocketMan
{
    public partial class RocketMod : Mod
    {
        public static RocketModSettings settings;

        public static int defaultValue = 0;

        public static bool resetInitiated = false;

        public static string defaultValueString = "5";

        public static List<StatSettings> statsSettings = new List<StatSettings>();

        public static string searchString = "";

        public static Vector2 scroll = Vector2.zero;
        public static Rect view = Rect.zero;

        public static RocketMod instance;

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

        public RocketMod(ModContentPack content) : base(content)
        {
            RocketMod.instance = this;
            RocketMod.settings = GetSettings<RocketModSettings>();
            UpdateExceptions();
        }

        public override string SettingsCategory()
        {
            return "RocketMan";
        }

        public override void DoSettingsWindowContents(global::UnityEngine.Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            DoSettings(inRect);
        }

        public override void WriteSettings()
        {
            UpdateStats();
            UpdateExceptions();
            base.WriteSettings();
        }

        public static void DoSettings(Rect inRect, bool doStats = true, Action<Listing_Standard> extras = null)
        {
            ReadStats();

            var listRect = new Rect(inRect.x, inRect.y + 10f, inRect.width, inRect.height - 50f);
            var contentRect = new Rect(0f, 0f, inRect.width - 20f, 50f * DefDatabase<StatDef>.AllDefs.Count() + 200);
            if (!doStats)
            {
                contentRect = new Rect(0f, 0f, inRect.width - 20f, 500);
            }

            Widgets.BeginScrollView(listRect, ref scroll, contentRect, true);

            var listing = new Listing_Standard();

            listing.Begin(contentRect);
            var font = Text.Font;
            Text.Font = GameFont.Medium;
            listing.Label("RocketMan Settings:");
            Text.Font = font;
            listing.GapLine();
            listing.CheckboxLabeled("Enabled", ref Finder.enabled);
            listing.GapLine();

            if (Finder.enabled)
            {
                Text.Font = GameFont.Tiny;
                listing.CheckboxLabeled("Enable debuging", ref Finder.debug);

                if (Finder.debug)
                {
                    listing.CheckboxLabeled("Enable Stat Logging (will destroy performance!)", ref Finder.statLogging);
                }

                listing.GapLine();

                listing.CheckboxLabeled("Enable thoughts checks caching", ref Finder.thoughtsCaching);

                listing.CheckboxLabeled("Enable adaptive mod", ref Finder.learning);

                listing.GapLine();

                listing.Label(string.Format("Clear all stored data in an interval of {0} ticks", Finder.universalCacheAge));
                Finder.universalCacheAge = (int)listing.Slider(Finder.universalCacheAge, 500, Finder.debug ? 10000 : 2000);

                listing.GapLine();

                if (doStats)
                {
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
                        RocketMod.resetInitiated = false;
                        Reset();
                    }

                    listing.GapLine();
                    DoStatSettings(listing);

                    listing.GapLine();
                }
            }

            if (extras != null)
            {
                extras.Invoke(listing);
            }

            Text.Font = font;

            listing.End();
            Widgets.EndScrollView();
        }

        public static void DoStatSettings(Listing_Standard listing)
        {
            UpdateStats();

            var counter = 0;
            var font = Text.Font;

            Text.Font = GameFont.Small;

            listing.Gap();
            listing.Label("Search:");

            searchString = listing.TextEntry(searchString, 1).ToLower();

            listing.Gap();

            Text.Font = GameFont.Tiny;

            foreach (StatSettings settings in statsSettings)
            {
                if (false
                    || searchString.Trim() == ""
                    || settings.stat.ToLower().Contains(searchString))
                {
                    listing.Label(string.Format("{0}. {1} set to expire in {2} ticks", counter++, settings.stat, settings.expireAfter));
                    settings.expireAfter = (int)listing.Slider(settings.expireAfter, 0, 255);
                }
            }

            Text.Font = font;

            foreach (StatSettings setting in statsSettings)
            {
                Finder.statExpiry[DefDatabase<StatDef>.defsByName[setting.stat].index] = (byte)setting.expireAfter;
            }

            RocketMod.instance.WriteSettings();
            RocketMod.UpdateExceptions();
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

            UpdateExceptions();
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

            UpdateExceptions();
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
                Scribe_Values.Look<bool>(ref Finder.thoughtsCaching, "thoughtsCaching", true);

                Scribe_Values.Look<int>(ref Finder.ageOfGetValueUnfinalizedCache, "ageOfGetValueUnfinalizedCache", 0);
                Scribe_Values.Look<int>(ref Finder.universalCacheAge, "universalCacheAge", 0);

                Scribe_Collections.Look(ref statsSettings, "statsSettings", LookMode.Deep);

                UpdateExceptions();
            }
        }
    }
}
