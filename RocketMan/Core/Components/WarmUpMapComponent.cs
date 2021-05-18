using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RocketMan
{
    public class WarmUpMapComponent : MapComponent
    {
        private const int WARMUP_TIME = 4;

        private bool finished = false;
        private bool started = false;
        private bool settingsStashed = false;
        private int startingTicksGame = -1;
        private int ticksPassed = 0;
        private bool showUI = true;

        public float Progress
        {
            get => ticksPassed.TicksToSeconds() / WARMUP_TIME;
        }

        public bool Finished
        {
            get => finished;
        }

        public bool Started
        {
            get => started;
        }

        public bool Expired
        {
            get => Progress == 1.0f;
        }

        public static WarmUpMapComponent current;
        public static int warmUpsCount = 0;
        public static bool settingsBeingStashed = false;

        private Dictionary<FieldInfo, object> stashedValues = new Dictionary<FieldInfo, object>();

        public WarmUpMapComponent(Map map) : base(map)
        {
            current?.AbortWarmUp();
            showUI = Finder.enabled;
            Initialize();
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            Initialize();
        }

        public override void MapComponentOnGUI()
        {
            base.MapComponentOnGUI();
            if (finished || !started || !settingsBeingStashed || !showUI)
                return;
            if (!Finder.showWarmUpPopup)
                return;
            int width = 450;
            int height = 65;
            Rect rect = new Rect(UI.screenWidth / 2 - ((float)width / 2), UI.screenHeight / 5, width, height);
            GUIUtility.StashGUIState();
            DoPopupContent(rect);
            GUIUtility.RestoreGUIState();
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (finished)
                return;
            if (!started)
                return;
            int tick = GenTicks.TicksGame;
            if ((tick - startingTicksGame).TicksToSeconds() < WARMUP_TIME)
            {
                ticksPassed++;
                return;
            }
            finished = true;
            current = null;
            PopSettings();
            Log.Message("ROCKETMAN: <color=red>Warm up</color> Finished for new map!");
        }

        public override void MapRemoved()
        {
            base.MapRemoved();
            if (settingsStashed)
            {
                PopSettings();
                settingsBeingStashed = false;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public void AbortWarmUp()
        {
            if (settingsStashed || !finished)
            {
                current = null;
                finished = true;
                PopSettings();
                Log.Message("ROCKETMAN: <color=red>Warm up ABORTED!</color> for new map!");
            }
        }

        private void Initialize()
        {
            int tick = GenTicks.TicksGame;
            if (!started)
            {
                if (settingsBeingStashed)
                    return;
                StashSettings();
                warmUpsCount++;
                current = this;
                started = true;
                startingTicksGame = tick;
                Log.Message("ROCKETMAN: <color=red>Warm up</color> started for new map!");
            }
        }

        private void DoPopupContent(Rect inRect)
        {
            FontStyle style = Text.CurFontStyle.fontStyle;
            Color color = GUI.color;
            GameFont font = Text.Font;
            TextAnchor anchor = Text.Anchor;
            try
            {
                Widgets.DrawWindowBackground(inRect.ExpandedBy(20));
                Rect textRect = inRect.TopHalf();
                Rect progressRect = inRect.BottomHalf();
                progressRect.xMin += 25;
                progressRect.xMax -= 25;
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(textRect.TopPart(0.6f), (Find.TickManager?.Paused ?? false) ?
                    "<color=red>Please unpause the game!</color>" :
                    "<color=orange>RocketMan</color> is warming up! Please wait...");
                Text.Font = GameFont.Tiny;
                Widgets.Label(textRect.BottomPart(0.4f), "<color=grey>You can hide/disable in RocketMan's settings</color>");
                DoProgressBar(progressRect);
            }
            catch (Exception er)
            {
                Log.Warning($"ROCKETMAN: Warmup Popup error! {er}");
            }
            finally
            {
                Text.Anchor = anchor;
                Text.Font = font;
                Text.CurFontStyle.fontStyle = style;
                GUI.color = color;
            }
        }

        private void DoProgressBar(Rect rect)
        {
            float progress = Progress;
            rect = rect.ContractedBy(7);
            Widgets.DrawBoxSolid(rect, Color.grey);
            rect = rect.ContractedBy(1);
            Rect progressRect = rect.LeftPart(progress);
            Widgets.DrawBoxSolid(rect, Color.black);
            Widgets.DrawBoxSolid(progressRect, (Find.TickManager?.Paused ?? false) ? Color.yellow : Color.cyan);
        }

        private void StashSettings()
        {
            try
            {
                settingsStashed = true;
                settingsBeingStashed = true;
                StashSettingsInternal();
            }
            catch (Exception er)
            {
                Log.Error($"ROCKETMAN: Stashing settings failed! {er}");
                settingsStashed = false;
                settingsBeingStashed = false;
                current = null;
            }
        }

        private void PopSettings()
        {
            try
            {
                settingsStashed = false;
                settingsBeingStashed = false;
                PopSettingsInternal();
                stashedValues.Clear();
            }
            catch (Exception er)
            {
                Log.Error($"ROCKETMAN: Popping settings failed! {er}");
                settingsStashed = false;
                settingsBeingStashed = false;
                current = null;
            }
        }

        private void StashSettingsInternal()
        {
            foreach (FieldInfo field in Finder.settingsFields)
            {
                object value = field.GetValue(null);
                if (field.TryGetAttribute<Main.SettingsField>(out Main.SettingsField config))
                {
                    stashedValues[field] = value;
                    field.SetValue(null, config.warmUpValue);
                }
            }
        }

        private void PopSettingsInternal()
        {
            foreach (FieldInfo field in Finder.settingsFields)
            {
                object value = stashedValues[field];
                field.SetValue(null, value);
            }
        }
    }
}
