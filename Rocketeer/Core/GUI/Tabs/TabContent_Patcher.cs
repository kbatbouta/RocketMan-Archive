using System;
using System.Reflection;
using HarmonyLib;
using RocketMan;
using RocketMan.Tabs;
using UnityEngine;
using Verse;

namespace Rocketeer.Tabs
{
    public class TabContent_Patcher : ITabContent
    {
        private Listing_Standard standard = new Listing_Standard();

        public override string Label => "Debugger";
        public override bool ShouldShow => Finder.debug;

        private RocketeerPatchTracker report;
        private string target = string.Empty;

        public override void DoContent(Rect rect)
        {
            standard.Begin(rect);
            standard.Label("Method:");
            target = standard.TextEntry(target);
            if (standard.ButtonText("Patch") && AccessTools.Method(target) is MethodBase method && method != null)
            {
                report = RocketeerPatcher.Patch(method);
            }
            if (standard.ButtonText("Ping patches"))
            {
                Context.__MARCO = 10;
                Log.Message($"ROCKETEER: Ping counter {Context.__MARCO}!");
            }
            if (report == null)
            {
                standard.End();
                return;
            }
            standard.End();
        }

        public override void OnDeselect()
        {
        }

        public override void OnSelect()
        {
        }

        [Main.YieldTabContent]
        public static ITabContent YieldTab() => new TabContent_Patcher();
    }
}
