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
            CreateTabs();
        }
        
        public override Vector2 InitialSize => new Vector2(650, 450);

        public override void DoWindowContents(Rect inRect)
        {
            Finder.lastFrame = Time.frameCount;
            RocketMod.ReadStats();
            var debuggingOld = Finder.debug;
            tabs.DoContent(inRect);
            if (debuggingOld != Finder.debug || Rand.Chance(0.05f)) DebuggingChanged();
        }

        public override void Close(bool doCloseSound = true)
        {
            base.Close(doCloseSound);
            Finder.logData = false;
        }

        private void DebuggingChanged()
        {
            if (Finder.debug && !tabs.tabs.Any(t => t.GetType() == typeof(TabContent_Debug)))
            {
                tabs.tabs.Add(new TabContent_Debug());
            }
            else if (!Finder.debug && tabs.tabs.Any(t => t.GetType() == typeof(TabContent_Debug)))
            {
                tabs.tabs.RemoveAll(t => t.GetType() == typeof(TabContent_Debug));
                foreach (var tab in tabs.tabs) tab.Selected = false;
                tabs.tabs.RandomElement().Selected = true;
            }
        }

        private void CreateTabs()
        {
            tabs = new TabHolder(new List<ITabContent>()
            {
                new TabContent_Settings(){Selected = true},
                new TabContent_Stats(){Selected = false},
            }, useSidebar: true);
            if (Finder.soyuzLoaded)
                tabs.AddTab(new TabContent_Soyuz(){Selected = false});
        }
    }
}