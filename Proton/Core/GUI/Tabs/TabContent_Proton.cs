using System;
using RimWorld;
using RocketMan;
using RocketMan.Tabs;
using UnityEngine;
using Verse;

namespace Proton
{
    public class TabContent_Proton : ITabContent
    {
        private const float rowHeight = 50;
        private readonly Color difColor = new Color(0.2f, 0.2f, 0.2f);
        private Vector2 scrollPosition = Vector2.zero;

        public override string Label => "Alerts settings";
        public override bool ShouldShow => Finder.enabled;

        public override void DoContent(Rect rect)
        {
            int count = Context.alerts.Length;
            Widgets.DrawMenuSection(rect);
            Widgets.BeginScrollView(rect.ContractedBy(3), ref scrollPosition, new Rect(0, 0, rect.width - 15, count * rowHeight));
            RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
            {
                Rect current = new Rect(0, 0, rect.width - 15, rowHeight);
                for (int i = 0; i < count; i++)
                {
                    if (i % 2 == 0)
                    {
                        Widgets.DrawBoxSolid(current, difColor);
                    }
                    Widgets.DrawHighlightIfMouseover(current);
                    RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
                    {
                        DoAlertRow(current, Context.alertsSettings[i], Context.alerts[i]);
                    });
                    current.y += rowHeight;
                }
            });
            Widgets.EndScrollView();
        }

        private void DoAlertRow(Rect rect, AlertSettings settings, Alert alert)
        {
            if (settings == null)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = GameFont.Small;
                Widgets.Label(rect, $"Alert not configurable!");
            }
            else
            {
                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Tiny;
                rect.xMin += 15;
                rect.xMax -= 15;
                Text.CurFontStyle.fontStyle = FontStyle.Bold;
                bool before = settings.enabled;
                Widgets.CheckboxLabeled(rect.TopHalf(),
                    $"{settings.typeId.Replace('_', ' ').CapitalizeFirst()}",
                    ref settings.enabled);
                if (before && !settings.enabled)
                {
                    RemoveAlert(alert);
                }
                Text.CurFontStyle.fontStyle = FontStyle.Normal;
                string lastExecutionTime = settings.TimeSinceLastExecution > 0 ? $"{(int)settings.TimeSinceLastExecution / 1000f} Seconds" : "<color=red>Not being tracked</color>";
                Widgets.Label(rect.BottomHalf(), $"Average execution time is <color=orange>{settings.AverageExecutionTime} MS</color>. " +
                    $"Time since last check { lastExecutionTime }");
            }
        }

        private void RemoveAlert(Alert alert)
        {
            if (alert == null)
            {
                return;
            }
            alert.cachedActive = false;
            alert.cachedLabel = string.Empty;
        }

        public void DoExtras(Rect rect)
        {
        }

        public override void OnSelect()
        {
        }

        public override void OnDeselect()
        {
        }

        [Main.YieldTabContent]
        public static ITabContent YieldTab() => new TabContent_Proton();
    }
}
