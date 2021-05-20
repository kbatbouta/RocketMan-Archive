using System;
using RocketMan;
using RocketMan.Tabs;
using UnityEngine;
using Verse;

namespace Proton
{
    public class TabContent_Proton : ITabContent
    {
        private Listing_Standard standard = new Listing_Standard();

        public override string Label => "Alerts";
        public override bool ShouldShow => RocketDebugPrefs.debug;

        public override void DoContent(Rect rect)
        {

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
