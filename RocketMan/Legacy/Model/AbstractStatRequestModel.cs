using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RocketMan
{
    public class AbstractStatRequestModel : EqualityComparer<AbstractStatRequestModel>
    {
        private Def Def { get; set; }

        private Def Stuff { get; set; }

        public override bool Equals(AbstractStatRequestModel x, AbstractStatRequestModel y)
        {
            if (x.Def == y.Def && x.Stuff == y.Stuff)
                return true;

            return false;
        }

        public override int GetHashCode(AbstractStatRequestModel obj)
        {
            return Gen.HashCombineInt(obj.Def.shortHash, obj.Stuff?.shortHash ?? 0);
        }
    }
}