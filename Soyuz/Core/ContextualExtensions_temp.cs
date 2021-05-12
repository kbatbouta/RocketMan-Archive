using System;
using RimWorld;
using RimWorld.Planet;
using RocketMan;
using Verse;

namespace Soyuz
{
    public static partial class ContextualExtensions
    {
        public static bool IsValidWildlifeOrWorldPawn_newtemp(this Pawn pawn)
        {
            if (!Finder.timeDilationCriticalHediffs && pawn.HasCriticalHediff())
                return false;
            if (pawn.IsBleeding())
                return false;
            if (pawn.factionInt != Faction.OfPlayer && WorldPawnsTicker.isActive)
                return true;
            if (pawn.def.race.Humanlike)
            {
                Faction playerFaction = Faction.OfPlayer;
                if (pawn.factionInt == playerFaction)
                    return WorldPawnsTicker.isActive;
                if (pawn.guest?.isPrisonerInt ?? false && pawn.guest?.hostFactionInt == playerFaction)
                    return WorldPawnsTicker.isActive;
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
                }
                return WorldPawnsTicker.isActive;
            }
            return true;
        }

        public static bool IsSkippingTicks_newtemp(this Pawn pawn)
        {
            if (Context.zoomRange == CameraZoomRange.Far || Context.zoomRange == CameraZoomRange.Furthest)
                return true;
            if (WorldPawnsTicker.isActive)
                return true;
            if (!pawn.Spawned)
                return true;
            if (pawn.OffScreen())
                return true;
            return false;
        }

        public static bool ShouldTick_newtemp(this Pawn pawn)
        {
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
