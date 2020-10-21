using System;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;

namespace RocketMan.src
{
    public static class Tools
    {
        public static string GetStringHandler(this MethodBase method)
        {
            return string.Format("{0}:{1}", method.ReflectedType.Name, method.Name);
        }

        public static byte PredictValueFromString(this String name)
        {
            if (false
                || name.Contains("Combat")
                || name.Contains("Melee")
                || name.Contains("Range")
                || name.Contains("Ability")
                || name.Contains("Gain"))
            {
                return 0;
            }
            if (false
                || name.Contains("Stuff")
                || name.Contains("Cold")
                || name.Contains("Hot")
                || name.Contains("Insulation")
                || name.Contains("WorkSpeed")
                || name.Contains("Beauty")
                || name.Contains("Comfort")
                || name.Contains("Max")
                || name.Contains("Min"))
            {
                return 128;
            }
            return 32;
        }

        public static int GetKey(StatWorker statWorker, StatRequest req, bool applyPostProcess)
        {
            unchecked
            {
                int hash;
                hash = HashUtility.HashOne(statWorker.stat.shortHash);
                hash = HashUtility.HashOne(req.thingInt?.thingIDNumber ?? 0, hash);
                hash = HashUtility.HashOne(req.stuffDefInt?.shortHash ?? 0, hash);
                hash = HashUtility.HashOne((int)req.qualityCategoryInt, hash);
                hash = HashUtility.HashOne(req.defInt?.shortHash ?? 0, hash);
                hash = HashUtility.HashOne(req.faction?.loadID ?? 0, hash);
                hash = HashUtility.HashOne(req.pawn?.thingIDNumber ?? 0, hash);
                hash = HashUtility.HashOne(applyPostProcess ? 1 : 0, hash);
                return hash;
            }
        }

        public static int GetKey(StatRequest req)
        {
            unchecked
            {
                int hash;
                hash = HashUtility.HashOne(req.thingInt?.thingIDNumber ?? 0);
                hash = HashUtility.HashOne(req.stuffDefInt?.shortHash ?? 0, hash);
                hash = HashUtility.HashOne((int)req.qualityCategoryInt, hash);
                hash = HashUtility.HashOne(req.defInt?.shortHash ?? 0, hash);
                hash = HashUtility.HashOne(req.faction?.loadID ?? 0, hash);
                hash = HashUtility.HashOne(req.pawn?.thingIDNumber ?? 0, hash);

                return hash;
            }
        }

        public static Dictionary<A, B> DeepCopy<A, B>(this Dictionary<A, B> dict)
        {
            var other = new Dictionary<A, B>();
            foreach (var unit in dict)
                other[unit.Key] = unit.Value;
            return other;
        }
    }
}
