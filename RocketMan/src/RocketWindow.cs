using System;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RocketMan
{
    public class RocketWindow : Window
    {
        public override void DoWindowContents(Rect inRect)
        {
            RocketMod.ReadStats();
            RocketMod.DoSettings(inRect, doStats: false, extras: (Listing_Standard listing) =>
            {
                if (listing.ButtonText("Open StatsSettings"))
                {
                    if (Find.WindowStack.WindowOfType<RocketStatWindow>() != null)
                    {
                        Find.WindowStack.RemoveWindowsOfType(typeof(RocketStatWindow));
                    }
                    else
                    {
                        Find.WindowStack.Add(new RocketStatWindow());
                    }
                }
            });
        }

        public override void PostClose()
        {
            base.PostClose();
        }
    }

    public class RocketStatWindow : Window
    {
        public static Vector2 scroll = Vector2.zero;
        public static Rect view = Rect.zero;

        public override void DoWindowContents(Rect inRect)
        {
            var listRect = new Rect(inRect.x, inRect.y + 10f, inRect.width, inRect.height - 50f);
            var contentRect = new Rect(0f, 0f, inRect.width - 20f, 50f * DefDatabase<StatDef>.AllDefs.Count() + 200);

            Widgets.BeginScrollView(listRect, ref scroll, contentRect, true);

            var listing = new Listing_Standard();
            listing.Begin(contentRect);

            var font = Text.Font;

            Text.Font = GameFont.Medium;
            listing.Label("Stat Settings:");
            Text.Font = GameFont.Small;

            if (listing.ButtonText("Close"))
            {
                Close();
            }
            Text.Font = GameFont.Medium;
            RocketMod.DoStatSettings(listing);

            Text.Font = font;
            listing.End();

            Widgets.EndScrollView();
        }
    }
}
