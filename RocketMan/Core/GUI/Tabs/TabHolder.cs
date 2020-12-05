using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RocketMan.Tabs
{
    public class TabHolder
    {
        public ITabContent curTab;
        private int curTabIndex;

        private Vector2 scrollPosition = Vector2.zero;
        private Rect tabBarRect;
        public List<ITabContent> tabs;
        private readonly List<TabRecord> tabsRecord;

        private readonly bool useSidebar = true;

        public TabHolder(List<ITabContent> tabs, bool useSidebar = false)
        {
            this.useSidebar = useSidebar;
            if (tabs.Any(i => i.Selected))
            {
                curTab = tabs.First(i => i.Selected);
                curTab.Selected = true;
            }
            else
            {
                curTab = tabs[0];
                curTab.Selected = true;
            }

            tabsRecord = new List<TabRecord>();
            this.tabs = tabs;
            MakeRecords();
        }

        public void DoContent(Rect inRect)
        {
            var selectedFound = false;
            var counter = 0;
            foreach (var tab in tabs)
            {
                if (tab.Selected)
                {
                    selectedFound = true;
                    curTabIndex = counter;
                    continue;
                }
                if (tab.Selected && selectedFound)
                    tab.Selected = false;
                counter++;
            }
            if (selectedFound == false)
            {
                curTabIndex = 0;
                tabs[0].Selected = true;
            }
            var font = Text.Font;
            var anchor = Text.Anchor;
            curTab = tabs[curTabIndex];
            if (useSidebar)
            {
                var tabsRect = inRect.LeftPartPixels(170);
                var contentRect = new Rect(inRect);
                contentRect.xMin += 180;
                DoSidebar(tabsRect);
                curTab.DoContent(contentRect);
            }
            else
            {
                inRect.yMin += 40;
                var tabRect = new Rect(inRect);
                tabRect.height = 0;

                MakeRecords();
                TabDrawer.DrawTabs(tabRect, tabsRecord);
                curTab.DoContent(inRect);
            }

            Text.Anchor = anchor;
            Text.Font = font;
        }

        public void AddTab(ITabContent newTab)
        {
            tabs.Add(newTab);
            tabsRecord.Add(new TabRecord(newTab.Label, () => { curTabIndex = tabs.Count; }, false));
        }

        public void RemoveTab(ITabContent tab)
        {
            if (tab.Selected)
            {
                tab.Selected = false;
                tabs.RemoveAll(t => t == tab);
                curTabIndex = 0;
                tabs.First().Selected = true;
            }
            else
            {
                tabs.RemoveAll(t => t == tab);
                for (var i = 0; i < tabs.Count; i++)
                    if (tabs[i].Selected)
                        curTabIndex = i;
            }
        }

        private void MakeRecords()
        {
            tabsRecord.Clear();
            var counter = 0;
            foreach (var tab in tabs)
            {
                var localTab = tab;
                var localCounter = counter;
                tabsRecord.Add(new TabRecord(tab.Label, () =>
                {
                    tab.Selected = true;
                    curTabIndex = localCounter;
                    curTab.Selected = false;
                    curTab = localTab;
                }, tab.Selected));
                counter++;
            }
        }

        private void DoSidebar(Rect rect)
        {
            tabBarRect = rect;
            tabBarRect.width -= 2;
            tabBarRect.height = 30 * tabs.Count;
            Widgets.DrawMenuSection(rect);
            Widgets.BeginScrollView(rect, ref scrollPosition, tabBarRect);
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Tiny;
            var curRect = new Rect(5, 5, 160, 30);
            var counter = 0;
            foreach (var tab in tabs)
            {
                if (tab.Selected)
                    Widgets.DrawWindowBackgroundTutor(curRect);
                Widgets.DrawHighlightIfMouseover(curRect);
                var textRect = new Rect(curRect);
                textRect.xMin += 10;
                Widgets.Label(textRect, tab.Label);
                var localTab = tab;
                var localCounter = counter;
                if (!tab.Selected && Widgets.ButtonInvisible(curRect))
                {
                    localTab.Selected = true;
                    curTab.Selected = false;
                    curTab = localTab;
                    curTabIndex = localCounter;
                }

                curRect.y += 30;
                counter++;
            }

            Widgets.EndScrollView();
        }
    }
}