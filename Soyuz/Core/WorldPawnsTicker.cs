using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RocketMan;
using UnityEngine;
using Verse;

namespace Soyuz
{
    public class WorldPawnsTicker : GameComponent
    {
        public const int BucketCount = 30;

        private static HashSet<Pawn> pawns = new HashSet<Pawn>();
        private static HashSet<Pawn> colonists = new HashSet<Pawn>();
        private static HashSet<Pawn>[] buckets;
        private static Game game;
        private static HashSet<Pawn> previousBucket = new HashSet<Pawn>();
        private static readonly HashSet<Pawn> emptySet = new HashSet<Pawn>();
        public static int curIndex = 0;
        public static int curCycle = 0;

        public static bool isActive = false;

        public static HashSet<Pawn> PreviousBucket
        {
            get => previousBucket;
        }

        public WorldPawnsTicker(Game game)
        {
            TryInitialize();
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            TryInitialize();
        }

        public override void LoadedGame()
        {
            base.LoadedGame();
            TryInitialize();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref curIndex, "curIndex", 0);
            if (curIndex >= BucketCount)
            {
                curIndex = 0;
                curCycle = 0;
            }
        }

        public static void TryInitialize()
        {
            if (game != Current.Game || buckets == null)
            {
                curIndex = curCycle = 0;
                game = Current.Game;
                buckets = new HashSet<Pawn>[BucketCount];
                for (int i = 0; i < BucketCount; i++)
                    buckets[i] = new HashSet<Pawn>();
            }
        }

        public static void Rebuild(WorldPawns instance)
        {
            curCycle = 0;
            curIndex = 0;
            for (int i = 0; i < BucketCount; i++) buckets[i].Clear();
            foreach (Pawn pawn in instance.pawnsAlive) Register(pawn);
        }

        public static void Register(Pawn pawn)
        {
            var index = GetBucket(pawn);
            if (buckets == null)
                TryInitialize();
            if (buckets[index] == null)
                buckets[index] = new HashSet<Pawn>();
            if (pawn.IsCaravanMember() && pawn.GetCaravan().Faction == Faction.OfPlayerSilentFail)
                colonists.Add(pawn);
            pawns.Add(pawn);
            buckets[index].Add(pawn);
        }

        public static void Deregister(Pawn pawn)
        {
            var index = GetBucket(pawn);
            if (buckets[index] == null) return;
            pawns.RemoveWhere(p => p.thingIDNumber == pawn.thingIDNumber);
            colonists.RemoveWhere(p => p.thingIDNumber == pawn.thingIDNumber);
            buckets[index].Remove(pawn);
        }

        public static HashSet<Pawn> GetPawns()
        {
            HashSet<Pawn> bucket = buckets[curIndex];
            curIndex = GenTicks.TicksGame % 30;
            if (curIndex == 0)
                curCycle++;
            if (Finder.timeDilationCaravans) previousBucket = bucket ?? emptySet;
            else previousBucket = AddExtraPawns(bucket).ToHashSet() ?? emptySet;
            return previousBucket;
        }

        public static bool IsCustomWorldTickInterval(Thing thing, int interval)
        {
            return interval <= BucketCount ? true : curCycle % ((int)(interval / BucketCount)) == 0;
        }

        private static IEnumerable<Pawn> AddExtraPawns(IEnumerable<Pawn> bucket)
        {
            foreach (Pawn pawn in bucket)
            {
                if (pawn.IsCaravanMember())
                    continue;
                yield return pawn;
            }
            foreach (Pawn pawn in colonists)
                yield return pawn;
        }

        private static int GetBucket(Pawn pawn)
        {
            int hash;
            unchecked
            {
                hash = pawn.thingIDNumber + GenTicks.TicksGame;
                if (hash < 0) hash *= -1;
            }
            return hash % BucketCount;
        }
    }
}