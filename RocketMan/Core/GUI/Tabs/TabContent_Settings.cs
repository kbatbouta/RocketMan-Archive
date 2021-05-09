using UnityEngine;

namespace RocketMan.Tabs
{
    public class TabContent_Settings : ITabContent
    {
        public override string Label => "Home";
        public override bool ShouldShow => true;

        public override void DoContent(Rect rect)
        {
            RocketMod.DoSettings(rect);
        }

        public override void OnSelect()
        {
        }

        public override void OnDeselect()
        {
        }
    }
}