using System;
using System.Collections.Generic;
using System.Reflection;
using RocketMan.Tabs;
using UnityEngine;
using Verse;

namespace RocketMan
{
    public class RocketWindow : Window
    {
        private TabHolder tabs;
        private int _errors = 0;
        private Listing_Standard standard = new Listing_Standard();

        public override Vector2 InitialSize => new Vector2(650, 450);

        public RocketWindow()
        {
            draggable = true;
            absorbInputAroundWindow = false;
            preventCameraMotion = false;
            resizeable = true;
            drawShadow = true;
            doCloseButton = false;
            doCloseX = true;
            layer = WindowLayer.SubSuper;
            tabs = new TabHolder(new List<ITabContent>()
            {
                new TabContent_Settings(){ Selected = true },
                new TabContent_Stats(){ Selected = false },
            }, useSidebar: true);
            for (var i = 0; i < Main.yieldTabContent.Count; i++) tabs.AddTab(Main.yieldTabContent[i].Invoke());
        }

        public override void DoWindowContents(Rect inRect)
        {
            FontStyle style = Text.CurFontStyle.fontStyle;
            Color color = GUI.color;
            GameFont font = Text.Font;
            try
            {
                // TODO fix this mess
                // For profiling reasons...
                Finder.lastFrame = Time.frameCount;
                // For Stat settings reason...
                RocketMod.ReadStats();
                // Actual work
                standard.Begin(inRect.TopPart(25));
                Text.CurFontStyle.fontStyle = FontStyle.Bold;
                GUI.color = Color.white;
                standard.Label("RocketMan");
                Text.CurFontStyle.fontStyle = FontStyle.Normal;
                Text.Font = GameFont.Tiny;
                GUI.color = Color.white;
                standard.End();
                inRect.yMin += 25;
                tabs.DoContent(inRect);
                // Reduce the error counter
                _errors = Math.Max(_errors - 1, 0);
            }
            catch (Exception er)
            {
                if (_errors <= 60 && _errors % 2 == 0) Log.Warning($"ROCKETMAN: UI Minor error:{er}\n{er.StackTrace}\nError count:{_errors}");
                else if (_errors <= 60) Log.Warning($"ROCKETMAN: UI error:{er}\n{er.StackTrace}\nError count:{_errors}");
                else Log.Error($"ROCKETMAN: UI Major error:{er}\n{er.StackTrace}\nError count:{_errors}");
                _errors += 3;
            }
            finally
            {
                Text.Font = font;
                Text.CurFontStyle.fontStyle = style;
                GUI.color = color;
            }
        }

        public override void Close(bool doCloseSound = true)
        {
            base.Close(doCloseSound);
            Finder.logData = false;
        }
    }
}