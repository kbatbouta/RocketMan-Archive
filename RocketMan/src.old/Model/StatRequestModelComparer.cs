using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace RocketMan
{
    public class StatRequestModelComparer : EqualityComparer<StatRequestModel>
    {
        public static StatRequestModelComparer Instance { get; } = new StatRequestModelComparer();

        public override bool Equals(StatRequestModel x, StatRequestModel y)
        {
            if (x.Stat != y.Stat)
                return false;

            var xReq = x.StatRequest;
            var yReq = y.StatRequest;

            if (xReq.Thing == yReq.Thing
                && xReq.StuffDef == yReq.StuffDef
                && xReq.QualityCategory == yReq.QualityCategory
                && xReq.Def == yReq.Def
                && xReq.Faction == yReq.Faction
                && xReq.Pawn == yReq.Pawn
                && x.ApplyPostProcess == y.ApplyPostProcess)
                return true;

            return false;
        }

        public override int GetHashCode(StatRequestModel obj)
        {
            var statRequest = obj.StatRequest;

            unchecked
            {
                int hash;
                hash = HashUtility.HashOne(obj.Stat.shortHash);
                hash = HashUtility.HashOne(statRequest.Thing?.thingIDNumber ?? 0, hash);
                hash = HashUtility.HashOne(statRequest.StuffDef?.GetHashCode() ?? 0, hash);
                hash = HashUtility.HashOne((int) statRequest.QualityCategory, hash);
                hash = HashUtility.HashOne(statRequest.Def?.GetHashCode() ?? 0, hash);
                hash = HashUtility.HashOne(statRequest.Faction?.loadID ?? 0, hash);
                hash = HashUtility.HashOne(statRequest.Pawn?.thingIDNumber ?? 0, hash);
                hash = HashUtility.HashOne(obj.ApplyPostProcess ? 1 : 0, hash);

                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            return obj as StatRequestModelComparer != null;
        }

        public override int GetHashCode()
        {
            return nameof(StatRequestModelComparer).GetHashCode();
        }
    }
}