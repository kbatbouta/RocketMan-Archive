using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace RocketMan
{
    public readonly struct StatRequestModel : IEquatable<StatRequestModel>
    {
        public StatRequestModel(StatRequest statRequest, bool applyPostProcess, StatDef stat)
        {
            StatRequest = statRequest;
            ApplyPostProcess = applyPostProcess;
            Stat = stat;
        }

        public StatRequest StatRequest { get; }

        public bool ApplyPostProcess { get; }

        public StatDef Stat { get; }

        public bool Equals(StatRequestModel other)
        {
            return StatRequestModelComparer.Instance.Equals(this, other);
        }

        public override int GetHashCode()
        {
            return StatRequestModelComparer.Instance.GetHashCode(this);
        }

        public override bool Equals(object obj)
        {
            if (obj is StatRequestModel model)
                return Equals(model);

            return false;
        }
    }
}