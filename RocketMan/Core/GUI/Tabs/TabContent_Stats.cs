using UnityEngine;
using Verse;

namespace RocketMan.Tabs
{
    public class TabContent_Stats : ITabContent
    {
        private Listing_Standard standard = new Listing_Standard();
        public override string Label => "Statistics";
        public override void DoContent(Rect rect)
        {
            standard.Begin(rect.TopPart(60));
            standard.Gap();
            var font = Text.Font;
            Text.Font = GameFont.Tiny;
            standard.CheckboxLabeled("Adaptive mode", ref Finder.learning, "Only enable for 30 minutes.");
            standard.CheckboxLabeled("Enable gear stat caching", ref Finder.statGearCachingEnabled,
                "Can cause bugs.");
            standard.GapLine();
            Text.Font = font;
            standard.End();
            rect.yMin += 64;
            RocketMod.DoStatSettings(rect);
        }

        public override void OnSelect()
        {
        }

        public override void OnDeselect()
        {
        }
    }
}