using UnityEngine;
using Verse;

namespace RocketMan.Tabs
{
    public class TabContent_Debug : ITabContent
    {
        private Listing_Standard standard = new Listing_Standard();
        public override string Label => "Debugging";
        public override void DoContent(Rect rect)
        {
            standard.Begin(rect);
        
            GUI.color = Color.red;
            Text.CurFontStyle.fontStyle = FontStyle.Bold;
            
            standard.Label("Advanced settings");
            Text.CurFontStyle.fontStyle = FontStyle.Normal;
            GUI.color = Color.white;
            var font = Text.Font;
            Text.Font = GameFont.Tiny;
            
            standard.CheckboxLabeled("Enable Stat Logging (Will kill performance)", ref Finder.statLogging);
            standard.CheckboxLabeled("Enable GlowGrid flashing", ref Finder.drawGlowerUpdates);
            standard.CheckboxLabeled("Enable GlowGrid refresh", ref Finder.enableGridRefresh);            
            standard.GapLine();
            standard.CheckboxLabeled("Set tick multiplier to 150", ref Finder.debug150MTPS,  "Dangerous!");
            standard.CheckboxLabeled("Enable data logging", ref Finder.logData, "Experimental.");
            standard.CheckboxLabeled("Enable time dilation", ref Finder.timeDilation, "Experimental.");
            Text.Font = font;
            standard.End();
            rect.yMin += 165;
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