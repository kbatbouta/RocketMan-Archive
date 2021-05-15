using System;
using RimWorld;
using RimWorld.Planet;
using RocketMan;
using Verse;

namespace Soyuz
{
    public static partial class ContextualExtensions
    {
        public static bool IsValidWildlifeOrWorldPawnInternal_newtemp(this Pawn pawn)
        {
            if (pawn?.def == null)
                return false;
            if (!Finder.enabled || !Finder.timeDilation)
                return false;
            if (!Context.dilationEnabled[pawn.def.index])
                return false;
            if (WorldPawnsTicker.isActive)
            {
                if (!Finder.timeDilationCaravans && pawn.IsCaravanMember() && pawn.GetCaravan().Faction == Faction.OfPlayer)
                    return false;
                if (!Finder.timeDilationWorldPawns)
                    return false;
                return true;
            }
            if (pawn.IsBleeding() || (!Finder.timeDilationCriticalHediffs && pawn.HasCriticalHediff()))
                return false;
            if (pawn.def.race.Humanlike)
            {
                Faction playerFaction = Faction.OfPlayer;
                if (pawn.factionInt == playerFaction)
                    return false;
                if (pawn.guest?.isPrisonerInt ?? false && pawn.guest?.hostFactionInt == playerFaction)
                    return false;
                if (Finder.timeDilationVisitors)
                {
                    JobDef jobDef = pawn.jobs?.curJob?.def;
                    if (jobDef == null)
                        return false;
                    if (jobDef == JobDefOf.Wait_Wander)
                        return true;
                    if (jobDef == JobDefOf.Wait)
                        return true;
                    if (jobDef == JobDefOf.SocialRelax)
                        return true;
                    if (jobDef == JobDefOf.LayDown)
                        return true;
                    if (jobDef == JobDefOf.Follow)
                        return true;
                }
                return WorldPawnsTicker.isActive;
            }
            RaceSettings raceSettings = pawn.GetRaceSettings();
            if (pawn.factionInt == Faction.OfPlayer)
                return !raceSettings.ignorePlayerFaction;
            if (pawn.factionInt != null)
                return !raceSettings.ignoreFactions;
            return true;
        }

        public static bool IsSkippingTicks_newtemp(this Pawn pawn)
        {
            if (!pawn.Spawned && WorldPawnsTicker.isActive)
                return true;
            if (pawn.OffScreen())
                return true;
            if (Context.zoomRange == CameraZoomRange.Far || Context.zoomRange == CameraZoomRange.Furthest || Context.zoomRange == CameraZoomRange.Middle)
                return true;
            return false;
        }

        public static bool ShouldTick_newtemp(this Pawn pawn)
        {
            if (pawn == null)
                return true;
            int tick = GenTicks.TicksGame;
            shouldTick = ShouldTickInternal_newtemp(pawn);
            if (timers.TryGetValue(pawn.thingIDNumber, out var val))
                curDelta = GenTicks.TicksGame - val;
            else
                curDelta = 1;
            if (shouldTick)
                timers[pawn.thingIDNumber] = tick;
            return shouldTick;
        }

        public static bool ShouldTickInternal_newtemp(this Pawn pawn)
        {
            int tick = GenTicks.TicksGame;
            int id = pawn.thingIDNumber;
            if ((id + tick) % 30 == 0 || (tick) % 250 == 0 || tick % DilationRate == 0)
                return true;
            if (true
                && pawn.jobs?.curJob?.expiryInterval > 0
                && (tick - pawn.jobs.curJob.startTick) % (pawn.jobs.curJob.expiryInterval * 2) == 0)
                return true;
            if (Context.zoomRange == CameraZoomRange.Far || Context.zoomRange == CameraZoomRange.Furthest)
                return (pawn.thingIDNumber + tick) % 3 == 0;
            if (pawn.OffScreen())
                return (pawn.thingIDNumber + tick) % DilationRate == 0;
            return true;
        }
    }
}
