using System;
using System.Linq;
using RocketMan;
using RocketMan.Tabs;
using UnityEngine;
using Verse;

namespace Soyuz.Tabs
{
    public class TabContent_Soyuz : ITabContent
    {
        private static Vector2 scrollPosition = Vector2.zero;
        private Listing_Standard standard = new Listing_Standard();
        private static Listing_Standard standard_extras = new Listing_Standard();

        private static Rect viewRect = Rect.zero;
        private static string searchString;
        private static RaceSettings curSelection;

        public override string Label => "Soyuz";
        public override bool ShouldShow => true;

        public override void DoContent(Rect rect)
        {
            standard.Begin(rect.TopPartPixels(80 + (Finder.debug ? 54 : 0)));
            var font = Text.Font;
            Text.Font = GameFont.Tiny;
            standard.Gap();
            standard.CheckboxLabeled("Enable time dilation", ref Finder.timeDilation, "Experimental.");
            standard.CheckboxLabeled("Enable time dilation for world pawns", ref Finder.timeDilationWorldPawns, "Experimental.");
            standard.CheckboxLabeled("Enable time dilation for pawns with critical hediffs", ref Finder.timeDilationCriticalHediffs, "This will enable dilation for pawns with critical hediffs such as pregnant pawns or bleeding pawns. (Disable this in case of a hediff problem)");
            standard.CheckboxLabeled("Enable data logging", ref Finder.logData, "Experimental.");
            if (Finder.debug)
            {
                standard.GapLine();
                standard.CheckboxLabeled("Enable Dilation flashing dilated pawns",
                    ref Finder.flashDilatedPawns);
                standard.CheckboxLabeled("Enable Simulate offscreen behavior", ref Finder.alwaysDilating);
            }
            Text.Font = font;
            standard.GapLine();
            standard.End();
            rect.yMin += 74 + (Finder.debug ? 54 : 0);
            DoExtras(rect);
        }

        public void DoExtras(Rect rect)
        {
            var stage = 0;
            Text.CurFontStyle.fontStyle = FontStyle.Bold;
            Widgets.Label(rect.TopPartPixels(25), "Dilated races");
            Text.CurFontStyle.fontStyle = FontStyle.Normal;
            if (Context.settings == null || Find.Selector == null)
            {
                return;
            }
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleLeft;
            rect.yMin += 25;
            var searchRect = rect.TopPartPixels(25);
            searchString = Widgets.TextField(searchRect, searchString).ToLower().Trim();
            rect.yMin += 30;
            if (searchString == null)
            {
                searchString = string.Empty;
            }
            if (curSelection != null)
            {
                var height = 128;
                var selectionRect = rect.TopPartPixels(height);
                Widgets.DrawMenuSection(selectionRect);
                Text.Font = GameFont.Tiny;
                Widgets.DefLabelWithIcon(selectionRect.TopPartPixels(54), curSelection.pawnDef);
                if (Widgets.ButtonImage(selectionRect.RightPartPixels(30).TopPartPixels(30).ContractedBy(5),
                    TexButton.CloseXSmall))
                {
                    curSelection = null;
                    return;
                }
                selectionRect.yMin += 54;
                standard_extras.Begin(selectionRect.ContractedBy(3));
                Text.Font = GameFont.Tiny;
                standard_extras.CheckboxLabeled($"Enable dilation for {curSelection.pawnDef?.label ?? "_"}", ref curSelection.dilated);
                standard_extras.CheckboxLabeled($"Enable dilation for all factions except Player", ref curSelection.ignoreFactions);
                standard_extras.CheckboxLabeled($"Enable dilation for Player faction", ref curSelection.ignorePlayerFaction);
                standard_extras.End();
                rect.yMin += height + 8;
            }
            else if (Find.Selector.selected.Count == 1 && Find.Selector.selected.First() is Pawn pawn && pawn != null)
            {
                var height = 128;
                var selectionRect = rect.TopPartPixels(height);
                var model = pawn.GetPerformanceModel();
                if (Finder.debug) Log.Message($"SOYUZ: UI stage is {stage}:{1}");
                if (model != null)
                {
                    model.DrawGraph(selectionRect, 2000);
                    rect.yMin += height + 8;
                }
            }
            Widgets.DrawMenuSection(rect);
            rect = rect.ContractedBy(2);
            viewRect.size = rect.size;
            viewRect.height = 60 * Context.settings.raceSettings.Count;
            viewRect.width -= 15;
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect.AtZero());
            Rect curRect = viewRect.TopPartPixels(54);
            curRect.width -= 15;
            var counter = 0;
            foreach (var element in Context.settings.raceSettings)
            {
                if (element?.pawnDef?.label == null)
                    continue;
                if (!element.pawnDef.label.ToLower().Contains(searchString))
                    continue;
                counter++;
                if (counter % 2 == 0)
                    Widgets.DrawBoxSolid(curRect, new Color(0.1f, 0.1f, 0.1f, 0.2f));
                Widgets.DrawHighlightIfMouseover(curRect);
                Widgets.DefLabelWithIcon(curRect.ContractedBy(3), element.pawnDef);
                if (Widgets.ButtonInvisible(curRect))
                {
                    curSelection = element;
                    break;
                }
                curRect.y += 58;
            }
            Widgets.EndScrollView();
            Text.Font = font;
            Text.Anchor = anchor;
            Finder.rocketMod.WriteSettings();
            SoyuzSettingsUtility.CacheSettings();
        }

        public override void OnSelect()
        {
        }

        public override void OnDeselect()
        {
        }

        [Main.YieldTabContent]
        public static ITabContent YieldTab() => new TabContent_Soyuz();
    }
}
