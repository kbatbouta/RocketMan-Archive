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
            return 0;
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
