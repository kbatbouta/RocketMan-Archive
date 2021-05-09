using System;
using RocketMan;
using RocketMan.Tabs;
using UnityEngine;
using Verse;

namespace Rocketeer.Tabs
{
    public class TabContent_Rocketeer : ITabContent
    {
        private Listing_Standard standard = new Listing_Standard();

        public override string Label => "Logging";
        public override bool ShouldShow => Finder.debug;

        public override void DoContent(Rect rect)
        {
        }

        public override void OnDeselect()
        {
        }

        public override void OnSelect()
        {
        }

        [Main.YieldTabContent]
        public static ITabContent YieldTab() => new TabContent_Rocketeer();
    }
}
