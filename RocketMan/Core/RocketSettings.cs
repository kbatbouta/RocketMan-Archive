using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RocketMan
{
    public partial class RocketMod : Mod
    {
        private static RocketModSettings settings;
        private static readonly Listing_Standard standard = new Listing_Standard();
        private static List<StatSettings> statsSettings = new List<StatSettings>();
        private static string searchString = "";
        private static Vector2 scroll = Vector2.zero;
        private static Rect view = Rect.zero;

        public static RocketMod instance;
        public static Vector2 scrollPositionStatSettings = Vector2.zero;

        public RocketMod(ModContentPack content) : base(content)
        {
            instance = this;
            settings = GetSettings<RocketModSettings>();
            UpdateExceptions();
        }

        public override string SettingsCategory()
        {
            return "RocketMan";
        }

        public override void DoSettingsWindowContents(Rect inRect)
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

            var font = Text.Font;
            var anchor = Text.Anchor;
            var style = Text.CurFontStyle.fontStyle;

            var rightPart = inRect.RightHalf();
            rightPart.xMin += 10;
            var leftPart = inRect.LeftHalf();

            standard.Begin(rightPart);
            {
                Text.Font = GameFont.Medium;
                Text.CurFontStyle.fontStyle = FontStyle.Bold;
                standard.Label("RocketMan 2:");
                Text.Font = GameFont.Tiny;
                Text.CurFontStyle.fontStyle = FontStyle.Normal;
            }
            {
                standard.CheckboxLabeled("Enabled", ref Finder.enabled);
            }
            if (Finder.enabled)
            {
                standard.GapLine();
                {
                    standard.CheckboxLabeled("Adaptive mod", ref Finder.learning, "Only enable for 30 minutes.");
                    standard.CheckboxLabeled("Enable gear stat caching", ref Finder.statGearCachingEnabled,
                        "Can cause bugs.");
                    standard.CheckboxLabeled("Thought caching", ref Finder.thoughtsCaching,
                        "Only enable for 30 minutes.");
                    standard.CheckboxLabeled("Enable time dilation", ref Finder.timeDilation, "Experimental.");
                    standard.CheckboxLabeled("Enable time dilation for world pawns", ref Finder.timeDilationWorldPawns, "Experimental.");
                }
                standard.GapLine();
                {
                    GUI.color = Color.red;
                    Text.CurFontStyle.fontStyle = FontStyle.Bold;
                    standard.Label("Advanced settings");
                    Text.CurFontStyle.fontStyle = FontStyle.Normal;
                    GUI.color = Color.white;
                }
                {
                    standard.CheckboxLabeled("Debugging", ref Finder.debug, "Only for advanced users and modders");
                    if (Finder.debug)
                    {
                        standard.CheckboxLabeled("Enable Stat Logging (Will kill performance)", ref Finder.statLogging);
                        standard.CheckboxLabeled("Enable GlowGrid flashing", ref Finder.drawGlowerUpdates);
                        standard.CheckboxLabeled("Enable GlowGrid refresh", ref Finder.enableGridRefresh);
                        standard.CheckboxLabeled("Enable Dilation flashing dilated pawns",
                            ref Finder.flashDilatedPawns);
                        standard.CheckboxLabeled("Enable Simulate offscreen behavior", ref Finder.alwaysDilating);
                    }
                }
            }

            standard.End();
            DoStatSettings(leftPart);

            Text.Font = font;
            Text.Anchor = anchor;
            Text.CurFontStyle.fontStyle = style;
        }

        public static void DoStatSettings(Rect rect)
        {
            UpdateStats();

            var counter = 0;
            var font = Text.Font;
            var anchor = Text.Anchor;

            Text.Font = GameFont.Small;
            searchString = Widgets
                .TextArea(rect.TopPartPixels(25), searchString)
                .ToLower();

            var scrollRect = rect;
            rect.yMin += 35;
            Widgets.BeginScrollView(rect, ref scrollPositionStatSettings,
                new Rect(Vector2.zero, new Vector2(rect.width - 15, statsSettings.Count * 54)));
            Text.Font = GameFont.Tiny;
            var size = new Vector2(rect.width - 20, 54);
            var curRect = new Rect(new Vector2(2, 2), size);
            foreach (var settings in statsSettings)
                if (false
                    || searchString.Trim() == ""
                    || settings.stat.ToLower().Contains(searchString))
                {
                    var rowRect = curRect.ContractedBy(5);
                    Text.Font = GameFont.Tiny;
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.DrawMenuSection(curRect.ContractedBy(1));
                    Widgets.Label(rowRect.TopHalf(), string.Format("{0}. {1} set to expire in {2} ticks", counter++,
                        settings.stat,
                        settings.expireAfter));
                    settings.expireAfter =
                        (byte) Widgets.HorizontalSlider(rowRect.BottomHalf(), settings.expireAfter, 0, 255);
                    curRect.y += size.y;
                }

            Widgets.EndScrollView();
            Text.Font = font;
            Text.Anchor = anchor;

            foreach (var setting in statsSettings)
                Finder.statExpiry[DefDatabase<StatDef>.defsByName[setting.stat].index] = (byte) setting.expireAfter;

            instance.WriteSettings();
            UpdateExceptions();
        }

        public static void ReadStats()
        {
            if (statsSettings == null || statsSettings.Count == 0) return;

            foreach (var setting in statsSettings)
                setting.expireAfter = Finder.statExpiry[DefDatabase<StatDef>.defsByName[setting.stat].index];
        }

        public static void Reset()
        {
            var defs = DefDatabase<StatDef>.AllDefs;

            statsSettings.Clear();
            foreach (var def in defs)
                statsSettings.Add(new StatSettings
                    {stat = def.defName, expireAfter = def.defName.PredictValueFromString()});

            var failed = false;
            foreach (var setting in statsSettings)
            {
                if (setting?.stat == null)
                {
                    failed = true;
                    break;
                }

                Finder.statExpiry[DefDatabase<StatDef>.defsByName[setting.stat].index] = (byte) setting.expireAfter;
            }

            if (failed)
            {
                Log.Warning("Failed to reindex the statDef database");
                statsSettings.Clear();

                UpdateStats();
            }

            UpdateExceptions();
        }

        [Main.OnDefsLoaded]
        public static void UpdateStats()
        {
            if (statsSettings == null) statsSettings = new List<StatSettings>();

            var defs = DefDatabase<StatDef>.AllDefs;
            if (statsSettings.Count != defs.Count())
            {
                statsSettings.Clear();
                foreach (var def in defs)
                    statsSettings.Add(new StatSettings
                        {stat = def.defName, expireAfter = def.defName.PredictValueFromString()});
            }

            var failed = false;
            foreach (var setting in statsSettings)
            {
                if (setting?.stat == null)
                {
                    failed = true;
                    break;
                }

                Finder.statExpiry[DefDatabase<StatDef>.defsByName[setting.stat].index] = (byte) setting.expireAfter;
            }

            if (failed)
            {
                Log.Warning("Failed to reindex the statDef database");
                statsSettings.Clear();

                UpdateStats();
            }

            UpdateExceptions();
        }

        public class StatSettings : IExposable
        {
            public int expireAfter;
            public string stat;

            public void ExposeData()
            {
                Scribe_Values.Look(ref stat, "statDef");
                Scribe_Values.Look(ref expireAfter, "expiryTime", 5);
            }
        }

        public class RocketModSettings : ModSettings
        {
            public override void ExposeData()
            {
                base.ExposeData();
                if (Scribe.mode == LoadSaveMode.LoadingVars) ReadStats();
                Scribe_Values.Look(ref Finder.enabled, "enabled", true);
                Scribe_Values.Look(ref Finder.statGearCachingEnabled, "statGearCachingEnabled", true);
                Scribe_Values.Look(ref Finder.learning, "learning");
                Scribe_Values.Look(ref Finder.debug, "debug");
                Scribe_Values.Look(ref Finder.thoughtsCaching, "thoughtsCaching", true);
                Scribe_Values.Look(ref Finder.timeDilation, "timeDilation", true);
                Scribe_Values.Look(ref Finder.timeDilationWorldPawns, "timeDilationWorldPawns", true);
                Scribe_Values.Look(ref Finder.ageOfGetValueUnfinalizedCache, "ageOfGetValueUnfinalizedCache");
                Scribe_Values.Look(ref Finder.universalCacheAge, "universalCacheAge");
                Scribe_Collections.Look(ref statsSettings, "statsSettings", LookMode.Deep);
                UpdateExceptions();
            }
        }
    }
}