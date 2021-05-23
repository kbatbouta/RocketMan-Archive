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
        private string searchString;

        public override string Label => "Alerts settings";
        public override bool ShouldShow => Finder.enabled;

        public override void DoContent(Rect rect)
        {
            int count = Context.alerts.Length;
            RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
            {
                Text.Font = GameFont.Tiny;
                Text.CurFontStyle.fontStyle = FontStyle.Normal;
                Widgets.CheckboxLabeled(rect.TopPartPixels(20), "Enable alerts controls", ref Finder.alertThrottling);
                bool disabled = Finder.disableAllAlert;
                Widgets.CheckboxLabeled(rect.TopPartPixels(40).BottomHalf(), "<color=red>Disable</color> all alerts", ref Finder.disableAllAlert);
                if (disabled != Finder.disableAllAlert && Finder.disableAllAlert)
                {
                    foreach (Alert alert in Context.alerts)
                    {
                        alert.cachedActive = false;
                        alert.cachedLabel = string.Empty;
                    }
                }
            });
            rect.yMin += 45;
            RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
            {
                Rect curRect = rect.TopPartPixels(55);
                Widgets.DrawMenuSection(curRect);
                curRect = curRect.ContractedBy(4);
                Widgets.Label(curRect.TopHalf(), "Max execution time (If an alert takes more than this it won't be executed again)");
                Context.settings.executionTimeLimit = Widgets.HorizontalSlider(
                    curRect.BottomHalf(), Context.settings.executionTimeLimit, 0.25f, 50f, label: $"{Context.settings.executionTimeLimit} MS");
            });
            rect.yMin += 60;
            RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
            {
                Rect curRect = rect.TopPartPixels(55);
                Widgets.DrawMenuSection(curRect);
                curRect = curRect.ContractedBy(4);
                Widgets.Label(curRect.TopHalf(), "Min refresh interval (The lower this is the the more updates but at the cost of performance)");
                Context.settings.minInterval = Widgets.HorizontalSlider(
                    curRect.BottomHalf(), Context.settings.minInterval, 1.0f, 7.5f, label: $"{Context.settings.minInterval} Seconds");
            });
            rect.yMin += 60;
            string oldSearchString = searchString;
            searchString = Widgets.TextField(rect.TopPartPixels(25), searchString).ToLower();
            if (oldSearchString != searchString)
                scrollPosition = Vector2.zero;
            rect.yMin += 30;
            Widgets.DrawMenuSection(rect);
            if (!Finder.disableAllAlert)
            {
                Widgets.BeginScrollView(rect.ContractedBy(3), ref scrollPosition, new Rect(0, 0, rect.width - 20, count * rowHeight));
                RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
                {
                    Rect current = new Rect(0, 0, rect.width - 15, rowHeight);
                    int j = 0;
                    for (int i = 0; i < count; i++)
                    {
                        AlertSettings alertSettings = Context.alertSettingsByIndex[i];
                        Alert alert = Context.alerts[i];
                        if (!searchString.Trim().NullOrEmpty() && !alert.GetName().ToLower().Contains(searchString))
                            continue;
                        if (j++ % 2 == 0)
                        {
                            Widgets.DrawBoxSolid(current, difColor);
                        }
                        Widgets.DrawHighlightIfMouseover(current);
                        RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
                        {
                            DoAlertRow(current, alertSettings, alert);
                        });
                        current.y += rowHeight;
                    }
                });
                Widgets.EndScrollView();
            }
            else
            {
                RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
                {
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Text.Font = GameFont.Medium;
                    Widgets.Label(rect, "Alerts are disabled!");
                });
            }
        }

        private void DoAlertRow(Rect rect, AlertSettings settings, Alert alert)
        {
            if (alert.cachedActive)
                Widgets.DrawBoxSolid(rect.LeftPartPixels(3), Color.green);
            else
                Widgets.DrawBoxSolid(rect.LeftPartPixels(3), settings != null && settings.enabledInt ? Color.gray : Color.red);
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
                bool before = settings.Enabled;
                Widgets.CheckboxLabeled(rect.TopHalf(), alert.GetName(), ref settings.enabledInt);
                if (before && !settings.enabledInt)
                {
                    settings.alert = alert;
                    settings.UpdateAlert();
                }
                Text.CurFontStyle.fontStyle = FontStyle.Normal;
                if (settings.AverageExecutionTime < Context.settings.executionTimeLimit)
                {
                    string lastExecutionTime = settings.TimeSinceLastExecution > 0 ? $"{(int)settings.TimeSinceLastExecution / 1000f} Seconds" : "<color=red>Not being tracked</color>";
                    Widgets.Label(rect.BottomHalf(), $"Average execution time is <color=orange>{settings.AverageExecutionTime} MS</color>. " +
                        $"Time since last check { lastExecutionTime }");
                }
                else
                {
                    Widgets.Label(rect.BottomHalf(), $"Alert is taking <color=red>too long to finish!</color> Change the <color=blue>max execution time</color> so this alert can be executed again!");
                }
            }
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
