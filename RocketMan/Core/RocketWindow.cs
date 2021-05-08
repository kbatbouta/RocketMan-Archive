﻿using System;
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
            if (Finder.debug) Log.Message("ROCKETMAN: UI DoWindowContents 0");
            Finder.lastFrame = Time.frameCount;
            var debuggingOld = Finder.debug;
            if (Finder.debug) Log.Message("ROCKETMAN: UI DoWindowContents 1");
            tabs.DoContent(inRect);
            if (debuggingOld != Finder.debug || Rand.Chance(0.05f)) DebuggingChanged();
            if (Finder.debug) Log.Message("ROCKETMAN: UI DoWindowContents 2");
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
                tabs.AddTab(new TabContent_Soyuz() { Selected = false });
            if (Finder.protonLoaded)
                tabs.AddTab(new TabContent_Proton() { Selected = false });
            for (var i = 0; i < Main.yieldTabContent.Count; i++) tabs.AddTab(Main.yieldTabContent[i].Invoke());
        }
    }
}