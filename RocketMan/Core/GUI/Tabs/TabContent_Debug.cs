using System.Runtime.InteropServices;
using UnityEngine;
using Verse;

namespace RocketMan.Tabs
{
    public class TabContent_Debug : ITabContent
    {
        private Listing_Standard standard = new Listing_Standard();
        public override string Label => "Advanced settings";

        public override bool ShouldShow => Finder.debug;

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
            if (standard.ButtonText("Disable debugging related stuff"))
            {
                Finder.debug = false;
                Finder.debug150MTPS = false;
                Finder.logData = false;
                Finder.statLogging = false;
                Finder.flashDilatedPawns = false;
            }
            Text.Font = font;
            standard.End();
            rect.yMin += 165;
        }

        public override void OnSelect()
        {
        }

        public override void OnDeselect()
        {
        }
    }
}