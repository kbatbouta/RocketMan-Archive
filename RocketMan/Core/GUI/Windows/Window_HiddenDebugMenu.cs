using System;
using UnityEngine;
using Verse;

namespace RocketMan
{
    public class Window_HiddenDebugMenu : Window
    {
        public Window_HiddenDebugMenu()
        {
            draggable = true;
            absorbInputAroundWindow = false;
            preventCameraMotion = true;
            resizeable = false;
            drawShadow = true;
            doCloseButton = false;
            doCloseX = true;
            layer = WindowLayer.Super;
        }

        public override void DoWindowContents(Rect inRect)
        {

        }
    }
}
