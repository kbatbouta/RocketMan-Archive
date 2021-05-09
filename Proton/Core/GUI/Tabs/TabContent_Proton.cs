using System;
using RocketMan;
using RocketMan.Tabs;
using UnityEngine;
using Verse;

namespace Proton.GUI
{
    public class TabContent_Proton : ITabContent
    {
        private Listing_Standard standard = new Listing_Standard();

        public override string Label => "Proton";
        public override bool ShouldShow => true;

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
        private ITabContent YieldTab() => new TabContent_Proton();
    }
}
