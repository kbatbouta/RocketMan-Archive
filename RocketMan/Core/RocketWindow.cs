using UnityEngine;
using Verse;

namespace RocketMan
{
    public class RocketWindow : Window
    {
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
        }

        public override Vector2 InitialSize => new Vector2(800, 450);

        public override void DoWindowContents(Rect inRect)
        {
            RocketMod.ReadStats();
            RocketMod.DoSettings(inRect, false, listing =>
            {
                if (listing.ButtonText("Open StatsSettings"))
                {
                    if (Find.WindowStack.WindowOfType<RocketStatWindow>() != null)
                        Find.WindowStack.RemoveWindowsOfType(typeof(RocketStatWindow));
                    else
                        Find.WindowStack.Add(new RocketStatWindow());
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

        public RocketStatWindow()
        {
            draggable = true;
            absorbInputAroundWindow = false;
            preventCameraMotion = false;
            resizeable = true;
            drawShadow = true;
            doCloseButton = false;
            doCloseX = true;
            layer = WindowLayer.Super;
        }

        public override void DoWindowContents(Rect inRect)
        {
            RocketMod.DoStatSettings(inRect);
        }
    }
}