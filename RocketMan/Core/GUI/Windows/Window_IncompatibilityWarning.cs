using System;
using UnityEngine;
using Verse;

namespace RocketMan
{
    public class Window_IncompatibilityWarning : Window
    {
        public override Vector2 InitialSize
        {
            get => new Vector2(Math.Min(UI.screenWidth / 3f, 400), Math.Min(UI.screenHeight / 2f, 400));
        }

        public Window_IncompatibilityWarning()
        {
            draggable = false;
            absorbInputAroundWindow = false;
            preventCameraMotion = true;
            resizeable = false;
            drawShadow = true;
            doCloseButton = false;
            doCloseX = false;
            layer = WindowLayer.SubSuper;
        }

        public override void DoWindowContents(Rect inRect)
        {

        }
    }
}
