using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using HarmonyLib;
using RimWorld;
using RocketMan;
using RocketMan.Tabs;
using UnityEngine;
using Verse;

namespace Soyuz
{
    public class RaceSettings : IExposable
    {
        public ThingDef pawnDef;
        public string pawnDefName;

        public bool dilated;
        public bool ignoreFactions;
        public bool ignorePlayerFaction;

        public int DilationInt
        {
            get
            {
                int val = 0;
                if (dilated) val = val | 1;
                if (ignoreFactions) val = val | 2;
                if (ignorePlayerFaction) val = val | 4;
                return val;
            }
        }

        public RaceSettings()
        {
        }

        public RaceSettings(string pawnDefName)
        {
            this.pawnDefName = pawnDefName;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref pawnDefName, "pawnDefName");
            Scribe_Values.Look(ref dilated, "dilated");
            Scribe_Values.Look(ref ignoreFactions, "ignoreFactions");
            Scribe_Values.Look(ref ignorePlayerFaction, "ignorePlayerFaction");
        }

        public void ResolveContent()
        {
            if (DefDatabase<ThingDef>.defsByName.TryGetValue(this.pawnDefName, out var def))
                this.pawnDef = def;
        }

        public void Cache()
        {
            Context.dilationByDef[pawnDef] = this;
            Context.dilationInts[pawnDef.index] = DilationInt;
        }
    }

    public class SoyuzSettings : IExposable
    {
        public List<RaceSettings> raceSettings = new List<RaceSettings>();

        public void ExposeData()
        {
            Scribe_Collections.Look(ref raceSettings, "raceSettings", LookMode.Deep);
            if (raceSettings == null) raceSettings = new List<RaceSettings>();
        }
    }
}