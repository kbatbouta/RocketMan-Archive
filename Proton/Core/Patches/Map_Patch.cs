using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Proton.Core;
using Verse;

namespace Proton
{
    [ProtonPatch(typeof(Map), nameof(Map.MapPostTick))]
    public class Map_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var skipLabel = generator.DefineLabel();
            yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Find), nameof(Find.TickManager)));
            yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(TickManager), nameof(TickManager.TicksGame)));
            yield return new CodeInstruction(OpCodes.Dup);
            yield return new CodeInstruction(OpCodes.Ldc_I4, 30);
            yield return new CodeInstruction(OpCodes.Rem);
            yield return new CodeInstruction(OpCodes.Brtrue_S, skipLabel);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Tools), nameof(Tools.GetJunkTracker)));
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MapJunkTracker), nameof(MapJunkTracker.TickRare)));
            var codes = instructions.ToList();
            codes[0].labels.Add(skipLabel);
            foreach (var code in codes)
                yield return code;
        }
    }
}
