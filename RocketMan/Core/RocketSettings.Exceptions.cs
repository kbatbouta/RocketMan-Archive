using RimWorld;
using Verse;

namespace RocketMan
{
    public partial class RocketMod
    {
        [Main.OnTick]
        [Main.OnDefsLoaded]
        public static void UpdateExceptions()
        {
            DefDatabase<StatDef>.ResolveAllReferences();
            if (StatDefOf.MarketValue != null && StatDefOf.MarketValueIgnoreHp != null)
            {
                Finder.statExpiry[StatDefOf.MarketValue.index] = 0;
                Finder.statExpiry[StatDefOf.MarketValueIgnoreHp.index] = 0;
            }
        }
    }
}