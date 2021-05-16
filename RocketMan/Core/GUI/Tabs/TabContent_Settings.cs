using UnityEngine;
using Verse;

namespace RocketMan.Tabs
{
    public class TabContent_Settings : ITabContent
    {
        public override string Label => "Home";
        public override bool ShouldShow => true;

        public override void DoContent(Rect rect)
        {
            GameFont font = Text.Font;
            TextAnchor anchor = Text.Anchor;

            if (Finder.WarmingUp)
            {
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.MiddleCenter;
                if (Find.TickManager.Paused)
                    Widgets.Label(rect, "Please unpause the game... RocketMan is warming up!");
                else
                    Widgets.Label(rect, "Please wait... RocketMan is warming up!");
            }
            else
            {
                RocketMod.DoSettings(rect);
            }

            Text.Font = font;
            Text.Anchor = anchor;
        }

        public override void OnSelect()
        {
        }

        public override void OnDeselect()
        {
        }
    }
}