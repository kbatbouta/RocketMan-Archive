using UnityEngine;
using Verse;

namespace RocketMan.Tabs
{
    public class TabContent_Soyuz : ITabContent
    {
        private Listing_Standard standard = new Listing_Standard();
        public override string Label => "Soyuz";
        
        public override void DoContent(Rect rect)
        {
            standard.Begin(rect.TopPartPixels(80 + (Finder.debug ? 54 : 0)));
            var font = Text.Font;
            Text.Font = GameFont.Tiny;
            standard.Gap();
            standard.CheckboxLabeled("Enable time dilation", ref Finder.timeDilation, "Experimental.");
            standard.CheckboxLabeled("Enable time dilation for world pawns", ref Finder.timeDilationWorldPawns, "Experimental.");
            standard.CheckboxLabeled("Enable data logging", ref Finder.logData, "Experimental.");
            if (Finder.debug)
            {
                standard.GapLine();
                standard.CheckboxLabeled("Enable Dilation flashing dilated pawns",
                    ref Finder.flashDilatedPawns);
                standard.CheckboxLabeled("Enable Simulate offscreen behavior", ref Finder.alwaysDilating);
            }
            Text.Font = font;
            standard.GapLine();
            standard.End();
            rect.yMin += 74 + (Finder.debug ? 54 : 0);
            DoExtras(rect);
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