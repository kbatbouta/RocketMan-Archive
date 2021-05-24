using System;
using System.Collections.Generic;
using RimWorld;
using RocketMan;
using RocketMan.Tabs;
using UnityEngine;
using Verse;

namespace Proton
{
    public class TabContent_Alerts : ITabContent
    {
        private Vector2 scrollPosition = Vector2.zero;

        private Alert curAlert;

        private AlertSettings curSettings;

        private string searchString = string.Empty;

        public override bool ShouldShow => Finder.enabled;

        public override string Label => "Alerts settings";

        public static readonly Color warningColor = new Color(1f, 0.913f, 0.541f, 0.2f);
        public static readonly Color dangerColor = new Color(0.972f, 0.070f, 0.137f, 0.2f);

        private string buffer1;
        private string buffer2;

        public static List<Pair<Color, string>> descriptionBoxes;

        public TabContent_Alerts()
        {
            if (descriptionBoxes == null)
            {
                descriptionBoxes = new List<Pair<Color, string>>();
                descriptionBoxes.Add(new Pair<Color, string>(Color.green, "Active now"));
                descriptionBoxes.Add(new Pair<Color, string>(Color.yellow, "Ignored and behaving badly"));
                descriptionBoxes.Add(new Pair<Color, string>(Color.red, "Disabled or behaving badly"));
                descriptionBoxes.Add(new Pair<Color, string>(Color.blue, "Ignored (for critical alerts)"));
            }
        }

        public override void OnDeselect()
        {
        }

        public override void OnSelect()
        {
        }

        public override void DoContent(Rect rect)
        {
            RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
            {
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.CheckboxLabeled(rect.TopPartPixels(20), "Enable alerts controls", ref Finder.alertThrottling);
                rect.yMin += 20;
                bool disabled = Finder.disableAllAlert;
                Widgets.CheckboxLabeled(rect.TopPartPixels(20), "<color=red>DISABLE</color> all alerts", ref Finder.disableAllAlert);
                if (disabled != Finder.disableAllAlert && Finder.disableAllAlert)
                {
                    foreach (Alert alert in Context.alerts)
                    {
                        alert.cachedActive = false;
                        alert.cachedLabel = string.Empty;
                    }
                }
                rect.yMin += 25;
            });
            RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
            {
                rect.yMin += 5;
                Text.Anchor = TextAnchor.MiddleLeft;
                if (buffer1 == null)
                {
                    buffer1 = $"{Context.settings.executionTimeLimit}";
                }
                Widgets.TextFieldNumericLabeled(rect.TopPartPixels(20), "Max execution time in <color=blue>MS</color> is ", ref Context.settings.executionTimeLimit, ref buffer1, 1.0f, 100.0f);
                rect.yMin += 25;
                if (buffer2 == null)
                {
                    buffer2 = $"{Context.settings.minInterval}";
                }
                Widgets.TextFieldNumericLabeled(rect.TopPartPixels(20), "Min update interval in <color=blue>seconds</color> is ", ref Context.settings.minInterval, ref buffer2, 0.5f, 25f);
                rect.yMin += 25;
            });
            if (Finder.alertThrottling && !Finder.disableAllAlert)
                DoScrollView(rect);
            else RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
               {
                   Text.Anchor = TextAnchor.MiddleCenter;
                   Text.Font = GameFont.Medium;
                   Widgets.DrawMenuSection(rect);
                   Widgets.Label(rect, Finder.disableAllAlert ? "Alerts are disabled!" : "Alerts contorls disabled!");
               });
        }

        private void DoScrollView(Rect inRect)
        {
            RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
            {
                string tempSearchString = Widgets.TextField(inRect.TopPartPixels(25), searchString).ToLower();
                if (tempSearchString != searchString)
                {
                    scrollPosition = Vector2.zero;
                    searchString = tempSearchString;
                }
                inRect.yMin += 30;
                if (curAlert != null)
                {
                    RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
                    {
                        Text.Anchor = TextAnchor.MiddleLeft;
                        Rect curRect = inRect.TopPartPixels(75);
                        Widgets.DrawMenuSection(curRect);
                        curRect.xMax -= 2;
                        Rect closeRect = curRect.TopPartPixels(20).RightPartPixels(20);
                        closeRect.x -= 3;
                        closeRect.y += 3;
                        if (Widgets.ButtonImage(closeRect, TexButton.CloseXSmall, true))
                        {
                            curAlert = null;
                            curSettings = null;
                            return;
                        }
                        curRect.xMin += 5;
                        RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
                        {
                            Text.Font = GameFont.Tiny;
                            Text.CurFontStyle.fontStyle = FontStyle.Bold;
                            Widgets.Label(curRect.TopPartPixels(25), $"{curAlert.GetName()}");
                        });
                        curRect.yMin += 25;
                        bool enabled = curSettings.enabledInt;
                        string color = enabled ? "white" : "red";
                        Widgets.CheckboxLabeled(curRect.TopPartPixels(20), $"<color={color}>Enabled</color>", ref enabled);
                        if (enabled != curSettings.enabledInt && enabled)
                        {
                            curSettings.UpdateAlert(true);
                        }
                        curSettings.Enabled = enabled;
                        curRect.yMin += 20;
                        Widgets.CheckboxLabeled(curRect.TopPartPixels(20), "Ignore this (will revert to vanilla behavior)", ref curSettings.ignored);
                        inRect.yMin += 80;
                    });
                }
            });
            RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
            {
                Rect curRect = inRect.TopPartPixels(45);
                Widgets.DrawMenuSection(curRect);
                Text.Font = GameFont.Tiny;
                RocketMan.GUIUtility.GridView<Pair<Color, string>>(curRect, 2, descriptionBoxes, (rect, pair) =>
                {
                    RocketMan.GUIUtility.ColorBoxDescription(rect, pair.first, pair.second);
                }, drawBackground: false);
            });
            inRect.yMin += 45;
            RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
            {
                Rect tempRect = inRect.TopPartPixels(25);
                Widgets.DrawMenuSection(tempRect);
                tempRect.xMin += 10;
                tempRect.xMax -= 25;
                RocketMan.GUIUtility.GridView<Action<Rect>>(tempRect.TopPartPixels(25), 3,
                        new List<Action<Rect>>()
                        {
                        (curRect) =>
                        {
                            Widgets.Label(curRect, "Alert name");
                        },
                        (curRect) =>
                        {
                            Widgets.Label(curRect, "Average execution time");
                        },
                        (curRect) =>
                        {
                            Widgets.Label(curRect, "Time since last update");
                        }
                        }, (rect, action) => { action.Invoke(rect); }, drawBackground: false);
            });
            inRect.yMin += 25;
            RocketMan.GUIUtility.ScrollView(inRect, ref scrollPosition, Context.readoutInstance.AllAlerts,
            heightLambda: (alert) =>
            {
                if (alert == null)
                    return -1.0f;
                if (!Context.alertToSettings.TryGetValue(alert, out _))
                    return -1.0f;
                if (searchString == null || searchString.NullOrEmpty())
                    return 35;
                return alert.GetNameLower().Contains(searchString) ? 40f : -1.0f;
            },
            elementLambda: (rect, alert) =>
            {
                AlertSettings settings = Context.alertToSettings[alert];
                if (settings.AverageExecutionTime > Context.settings.executionTimeLimit)
                {
                    if (settings.ignored) Widgets.DrawBoxSolid(rect, warningColor);
                    else Widgets.DrawBoxSolid(rect, dangerColor);
                }
                if (Widgets.ButtonInvisible(rect))
                {
                    curAlert = alert;
                    curSettings = settings;
                }
                Widgets.DrawBoxSolid(rect.LeftPartPixels(3), !settings.ignored ? (settings.enabledInt ? (alert.cachedActive ? Color.green : Color.grey) : Color.red) : Color.blue);
                RocketMan.GUIUtility.GridView<Action<Rect>>(rect, 3,
                    new List<Action<Rect>>()
                    {
                        (curRect) =>
                        {
                            RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
                            {
                                Text.Font = GameFont.Tiny;
                                Text.CurFontStyle.fontStyle = FontStyle.Bold;
                                string color = settings.Enabled ? "yello" : "while";
                                curRect.xMin += 3;
                                Widgets.Label(curRect, $"<color={color}>{alert.GetName().Fit(curRect)}</color>");
                            });
                        },
                        (curRect) =>
                        {
                            Widgets.Label(curRect, $"{Math.Round(settings.AverageExecutionTime, 3)} MS");
                        },
                        (curRect) =>
                        {
                             Widgets.Label(curRect, $"{Math.Round(settings.TimeSinceLastExecution, 3)} Seconds");
                        }
                    }, (tempRect, action) => { action.Invoke(tempRect); }, drawBackground: false);
            });
        }

        [Main.YieldTabContent]
        public static ITabContent YieldTab() => new TabContent_Alerts();
    }
}
