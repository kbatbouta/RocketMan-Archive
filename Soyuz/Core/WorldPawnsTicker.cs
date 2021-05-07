using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Soyuz
{
    public class WorldPawnsTicker : GameComponent
    {
        public const int BucketCount = 30;

        private const int TransformationCacheSize = 2500; 
        
        private static int[] _transformationCache = new int[TransformationCacheSize];

        private static HashSet<Pawn>[] buckets;
        private static Game game;
        
        public static int curIndex = 0;
        public static int curCycle = 0;     
        
        public static bool isActive = false;

        public WorldPawnsTicker(Game game)
        {
            TryInitialize();
            for (int i = 0; i < _transformationCache.Length; i++)
                _transformationCache[i] = (int) Mathf.Max((float) i / WorldPawnsTicker.BucketCount, 1);
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

        public static int Transform(int interval)
        {
            if (interval >= TransformationCacheSize)
                return (int) Mathf.Max((float) interval / WorldPawnsTicker.BucketCount, 1);
            return _transformationCache[interval];
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
            if (buckets == null) TryInitialize();
            if (buckets[index] == null) buckets[index] = new HashSet<Pawn>();
            buckets[index].Add(pawn);
        }

        public static void Deregister(Pawn pawn)
        {            
            var index = GetBucket(pawn);
            if(buckets[index] == null) return;
            buckets[index].Remove(pawn);
        }

        public static HashSet<Pawn> GetPawns()
        {
            var result = buckets[curIndex];
            curIndex = curIndex + 1;
            if (curIndex >= BucketCount)
            {
                curIndex = 0;
                curCycle += 1;
            }
            return result;
        }

        private static int GetBucket(Pawn pawn)
        {
            int hash;
            unchecked
            {
                hash = pawn.GetHashCode();
                if (hash < 0) hash *= -1;
            }
            return hash % BucketCount;
        }
    }
}