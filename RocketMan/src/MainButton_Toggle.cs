using HugsLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RocketMan
{
    internal class MainButton_Toggle : MainButtonWorker
    {
        public override bool Disabled
        {
            get
            {
                return Find.CurrentMap == null
                    && (!def.validWithoutMap || def == MainButtonDefOf.World) || Find.WorldRoutePlanner.Active
                    && Find.WorldRoutePlanner.FormingCaravan
                    && (!def.validWithoutMap || def == MainButtonDefOf.World);
            }
        }

        public override void Activate()
        {
            if (Event.current.button == 0)
            {
                if (Find.WindowStack.WindowOfType<RocketWindow>() != null)
                {
                    Find.WindowStack.RemoveWindowsOfType(typeof(RocketWindow));
                }
                else
                {
                    Find.WindowStack.Add(new RocketWindow());
                }
            }
            else
            {
                if (Find.WindowStack.WindowOfType<RocketWindow>() == null)
                {
                    Find.WindowStack.Add(new RocketWindow());
                }
            }
        }
    }
}
