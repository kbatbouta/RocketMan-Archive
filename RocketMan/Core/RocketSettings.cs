using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private static List<DilationSettings> dilationSettings = new List<DilationSettings>();
        private static string searchString = "";

        private const string PluginDir = "Plugins";

        public static RocketMod instance;
        public static Vector2 scrollPositionStatSettings = Vector2.zero;

        public RocketMod(ModContentPack content) : base(content)
        {
            Finder.rocketMod = this;
            try
            {
                LoadPlugins(content, "Soyuz.dll", "Soyuz");
                LoadPlugins(content, "Proton.dll", "Proton");
                LoadPlugins(content, "Rocketeer.dll", "Rocketeer");
            }
            catch (Exception er)
            {
                Log.Error($"ROCKETMAN: loading plugin failed {er.Message}:{er.StackTrace}");
            }
            finally
            {
                Main.ReloadActions();
                foreach (var action in Main.onInitialization)
                    action.Invoke();

                instance = this;
                settings = GetSettings<RocketModSettings>();
                UpdateExceptions();
            }
        }

        private static void LoadPlugins(ModContentPack content, string pluginAssemblyName, string name)
        {
            var pluginsPath = Path.Combine(content.RootDir, PluginDir);
            if (File.Exists(Path.Combine(pluginsPath, pluginAssemblyName)) &&
                !LoadedModManager.runningMods.Any(m => m.Name.Contains(name)))
            {
                Log.Message($"{Path.Combine(pluginsPath, pluginAssemblyName)}");
                byte[] rawAssembly = File.ReadAllBytes(Path.Combine(pluginsPath, pluginAssemblyName));

                Assembly asm;
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                if (assemblies.All(a => a != null && a.GetName().Name != name))
                {
                    asm = AppDomain.CurrentDomain.Load(rawAssembly);
                    Log.Message(asm.GetName().Name);
                }
                else
                {
                    asm = assemblies.First(a => a.GetName().Name == name);
                }
                if (content.assemblies.loadedAssemblies.Any(a => a.FullName == asm.FullName))
                {
                    return;
                }
                Finder.assemblies.Add(asm);
                content.assemblies.loadedAssemblies.Add(asm);
            }
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
            UpdateDilationDefs();
            UpdateExceptions();
            base.WriteSettings();
        }

        public static void DoSettings(Rect inRect, bool doStats = true, Action<Listing_Standard> extras = null)
        {
            ReadStats();
            ReadDilationSettings();

            var font = Text.Font;
            var anchor = Text.Anchor;
            var style = Text.CurFontStyle.fontStyle;

            var rect = inRect;
            rect.xMin += 10;

            standard.Begin(rect);

            Text.Font = GameFont.Medium;
            Text.CurFontStyle.fontStyle = FontStyle.Bold;
            standard.Label("RocketMan 2:");
            Text.CurFontStyle.fontStyle = style;
            Text.Font = GameFont.Tiny;
            Text.CurFontStyle.fontStyle = FontStyle.Normal;

            standard.CheckboxLabeled("Enabled", ref Finder.enabled);

            if (Finder.enabled)
            {
                standard.GapLine();
                standard.CheckboxLabeled("Adaptive mod", ref Finder.learning, "Only enable for 30 minutes.");
                standard.CheckboxLabeled("Enable gear stat caching", ref Finder.statGearCachingEnabled,
                    "Can cause bugs.");
                standard.GapLine();
                standard.CheckboxLabeled("Enable automatic corpses removal", ref Finder.corpsesRemovalEnabled,
                    "This removes corpses that aren't in view for a while and that aren't near your base to avoid breaking the game balance.");

            }

            standard.GapLine();
            standard.CheckboxLabeled("Debugging", ref Finder.debug, "Only for advanced users and modders");

            standard.End();

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
                        (byte)Widgets.HorizontalSlider(rowRect.BottomHalf(), settings.expireAfter, 0, 255);
                    curRect.y += size.y;
                }

            Widgets.EndScrollView();
            Text.Font = font;
            Text.Anchor = anchor;

            foreach (var setting in statsSettings)
                Finder.statExpiry[DefDatabase<StatDef>.defsByName[setting.stat].index] = (byte)setting.expireAfter;

            instance.WriteSettings();
            UpdateExceptions();
        }

        public static void ReadStats()
        {
            if (statsSettings == null || statsSettings.Count == 0) return;

            foreach (var setting in statsSettings)
                setting.expireAfter = Finder.statExpiry[DefDatabase<StatDef>.defsByName[setting.stat].index];
        }

        public static void ReadDilationSettings()
        {
            if (dilationSettings == null || dilationSettings.Count == 0) return;

            foreach (var setting in dilationSettings)
            {
                if (DefDatabase<ThingDef>.defsByName.TryGetValue(setting.def, out var td))
                {
                    setting.dilated = Finder.dilatedDefs[td.index];
                }
                else
                {
                    Log.Warning("ROCKETMAN: Failed to find stat upon reloading!");
                }
            }
        }

        [Main.OnDefsLoaded]
        public static void UpdateDilationDefs()
        {
            if (dilationSettings == null) dilationSettings = new List<DilationSettings>();
            var failed = false;
            var defs = DefDatabase<ThingDef>.AllDefs.Where(
                d => d.race != null).ToList();
            try
            {
                if (statsSettings.Count != defs.Count())
                {
                    dilationSettings.Clear();
                    foreach (var def in defs)
                        dilationSettings.Add(new DilationSettings()
                        {
                            def = def.defName,
                            dilated = def.race.Animal && !def.race.IsMechanoid && !def.race.Humanlike
                        });
                }

                foreach (var setting in dilationSettings)
                {
                    if (setting?.def == null)
                    {
                        failed = true;
                        break;
                    }
                    Finder.dilatedDefs[DefDatabase<ThingDef>.defsByName[setting.def].index] = setting.dilated;
                }
            }
            catch (Exception er)
            {
                Log.Error($"SOYUZ: {er}");
            }
            if (failed)
            {
                Log.Warning("SOYUZ: Failed to reindex the ThingDef database");
                statsSettings.Clear();

                UpdateStats();
            }
        }

        public static void Reset()
        {
            var defs = DefDatabase<StatDef>.AllDefs;
            statsSettings.Clear();
            foreach (var def in defs)
                statsSettings.Add(new StatSettings
                { stat = def.defName, expireAfter = def.defName.PredictValueFromString() });
            var failed = false;
            foreach (var setting in statsSettings)
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
                Log.Warning("SOYUZ: Failed to reindex the statDef database");
                statsSettings.Clear();

                UpdateStats();
            }
            dilationSettings.Clear();
            UpdateDilationDefs();
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
                    { stat = def.defName, expireAfter = def.defName.PredictValueFromString() });
            }

            var failed = false;
            foreach (var setting in statsSettings)
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

        public class DilationSettings : IExposable
        {
            public bool dilated = true;
            public string def;

            public void ExposeData()
            {
                Scribe_Values.Look(ref def, "def");
                Scribe_Values.Look(ref dilated, "dilated");
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
                Scribe_Values.Look(ref Finder.debug, "debug", false);
                Scribe_Values.Look(ref Finder.timeDilation, "timeDilation", true);
                Scribe_Values.Look(ref Finder.timeDilationVisitors, "timeDilationVisitors", false);
                Scribe_Values.Look(ref Finder.timeDilationWorldPawns, "timeDilationWorldPawns", true);
                Scribe_Values.Look(ref Finder.timeDilationColonyAnimals, "timeDialationColonyAnimals", true);
                Scribe_Values.Look(ref Finder.timeDilationCriticalHediffs, "timeDilationCriticalHediffs", true);
                Scribe_Values.Look(ref Finder.ageOfGetValueUnfinalizedCache, "ageOfGetValueUnfinalizedCache");
                Scribe_Values.Look(ref Finder.universalCacheAge, "universalCacheAge");
                Scribe_Values.Look(ref Finder.corpsesRemovalEnabled, "corpsesRemovalEnabled", true);
                Scribe_Collections.Look(ref statsSettings, "statsSettings", LookMode.Deep);
                Scribe_Collections.Look(ref dilationSettings, "dilationSettings", LookMode.Deep);
                foreach (var action in Main.onScribe)
                    action.Invoke();
                UpdateExceptions();
            }
        }
    }
}