using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Proton.Core
{
    public class MapJunkTracker
    {
        public readonly int mapId;
        public readonly Map map;

        private int curGameTick = -1;

        private List<Thing> things = new List<Thing>();

        public MapJunkTracker(Map map)
        {
            this.map = map;
            this.mapId = map.Index;
        }

        public void Start(int gameTick)
        {
            this.curGameTick = gameTick;
            this.TickRare();
            if (gameTick % 600 == 0) this.TickLong();
        }

        public void TickRare()
        {

        }

        public void TickLong()
        {

        }

        public void AddThing(Thing thing)
        {

        }

        public void RemoveThing(Thing thing)
        {

        }
    }
}
