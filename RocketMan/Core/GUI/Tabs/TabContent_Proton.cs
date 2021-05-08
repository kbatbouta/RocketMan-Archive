using System;
using RocketMan.Tabs;
using UnityEngine;
using Verse;

namespace RocketMan.Tabs
{
    public class TabContent_Proton : ITabContent
    {
        private Listing_Standard standard = new Listing_Standard();
        public override string Label => "Proton";

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
    }
}
