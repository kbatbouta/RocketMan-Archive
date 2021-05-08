using System;
using RocketMan;
using RocketMan.Tabs;
using UnityEngine;
using Verse;

namespace Rocketeer.Tabs
{
    public class TabContent_Patcher : ITabContent
    {
        private Listing_Standard standard = new Listing_Standard();
        public override string Label => "Patcher";

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
        public static ITabContent YieldTab() => new TabContent_Patcher();
    }
}
