using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
using static RocketMan.RocketShip;

namespace RocketMan
{
    public partial class Main
    {
        public static class GlowGrid_Patch
        {
            internal static GlowerPorperties currentProp;

            internal static Map[] maps = new Map[20];

            internal static bool deregister = false;
            internal static bool register = false;
            internal static bool calculating = false;
            internal static bool refreshAll = false;

            internal static HashSet<int>[] litCells = new HashSet<int>[20];
            internal static HashSet<int>[] removalIndices = new HashSet<int>[20];
            internal static HashSet<GlowerPorperties>[] removalList = new HashSet<GlowerPorperties>[20];
            internal static HashSet<GlowerPorperties>[] changedList = new HashSet<GlowerPorperties>[20];

            internal static Dictionary<CompGlower, GlowerPorperties>[] props = new Dictionary<CompGlower, GlowerPorperties>[20];

            internal class GlowerPorperties
            {
                public CompGlower glower;

                public HashSet<int> indices;

                public Vector3 position = Vector3.zero;

                public bool beingUpdated = false;
                public bool drawen = false;

                public bool IsValid => !glower.parent.Destroyed && glower.ShouldBeLitNow;
                public bool ShouldRemove => glower.parent.Destroyed;

                public GlowerPorperties(CompGlower glower)
                {
                    this.glower = glower;
                    this.indices = new HashSet<int>();

                    var dim = glower.Props.glowRadius * 2;
                    this.position = glower.parent.TrueCenter();
                    this.position.y = 0.0f;
                }

                public void Update()
                {
                    this.beingUpdated = true;
                    this.indices.Clear();
                }

                public void FinishUpdate()
                {
                    this.beingUpdated = false;
                }

                public bool Inersects(GlowerPorperties other)
                {
                    if (Vector3.Distance(other.position, this.position) + 1 < other.glower.Props.glowRadius + this.glower.Props.glowRadius)
                    {
                        return true;
                    }
                    return false;
                }

                public bool Contains(Vector3 loc)
                {
                    return Vector3.Distance(this.position, loc) + 1 < this.glower.Props.glowRadius;
                }

                public bool Contains(IntVec3 loc)
                {
                    return this.Contains(loc.ToVector3());
                }

                public void Reset()
                {
                    this.indices.Clear();
                    var dim = glower.Props.glowRadius * 2;
                    this.position = glower.parent.TrueCenter();
                    this.position.y = 0.0f;
                }

                public static GlowerPorperties GetGlowerPorperties([NotNull] CompGlower comp)
                {
                    if (comp == null)
                        return null;
                    if (props[comp.parent.Map.Index].TryGetValue(comp, out var prop))
                        return prop;
                    return props[comp.parent.Map.Index][comp] = new GlowerPorperties(comp);
                }
            }

            private static bool TryRegisterMap(Map map)
            {
                var index = map.Index;
                if (index < 0 || index >= 20)
                    return false;
                if (maps[index] != map)
                {
                    maps[map.Index] = map;
                    props[map.Index] = new Dictionary<CompGlower, GlowerPorperties>();
                    removalIndices[map.Index] = new HashSet<int>();
                    removalList[map.Index] = new HashSet<GlowerPorperties>();
                    changedList[map.Index] = new HashSet<GlowerPorperties>();
                    litCells[map.Index] = new HashSet<int>();
                }
                return true;
            }

            [HarmonyPatch(typeof(GlowGrid), nameof(GlowGrid.RegisterGlower))]
            internal static class RegisterGlower_Patch
            {
                internal static void Prefix(GlowGrid __instance, CompGlower newGlow)
                {
                    Map map = __instance.map;
                    register = true;
                    TryRegisterMap(map);

                    GlowerPorperties prop;
                    if (props[map.Index].ContainsKey(newGlow))
                    {
                        if (Finder.debug) Log.Warning(string.Format("ROCKETMAN: Double registering an registered glower {0}:{1}", newGlow, newGlow.parent));
                        return;
                    }
                    prop = new GlowerPorperties(newGlow);
                    props[map.Index][newGlow] = prop;
                }

                internal static void Postfix()
                {
                    register = false;
                }

            }

            [HarmonyPatch(typeof(GlowGrid), nameof(GlowGrid.DeRegisterGlower))]
            internal static class DeRegisterGlower_Patch
            {
                internal static void Prefix(GlowGrid __instance, CompGlower oldGlow)
                {
                    Map map = __instance.map;
                    deregister = true;
                    TryRegisterMap(map);

                    if (Finder.debug) Log.Message(string.Format("ROCKETMAN: Removed {0}", oldGlow));
                    if (!props[map.Index].ContainsKey(oldGlow))
                    {
                        if (Finder.debug) Log.Warning(string.Format("ROCKETMAN: Found an unregisterd {0}:{1}", oldGlow, oldGlow.parent));
                        return;
                    }
                    GlowerPorperties prop = props[map.Index][oldGlow];
                    prop.drawen = false;
                    removalList[map.Index].Add(prop);
                    props[map.Index].Remove(oldGlow);
                }

                internal static void Postfix()
                {
                    deregister = false;
                }
            }


            [HarmonyPatch(typeof(GlowGrid), nameof(GlowGrid.RecalculateAllGlow))]
            internal static class RecalculateAllGlow_Patch
            {
                private static Color32[] glowGridTemp;
                private static Color32[] glowGridEmpty;

                private static HashSet<GlowerPorperties> removalHolder = new HashSet<GlowerPorperties>();

                internal static void Prefix(GlowGrid __instance)
                {
                    if (Current.ProgramState == ProgramState.Playing)
                    {
                        var map = __instance.map;
                        var mapIndex = map.Index;
                        if (!TryRegisterMap(map))
                            return;
                        FixGridTemp(ref __instance.glowGrid);
                        calculating = true;
                    }
                }

                internal static void Postfix(GlowGrid __instance)
                {
                    if (Current.ProgramState == ProgramState.Playing)
                    {
                        var map = __instance.map;
                        var mapIndex = map.Index;

                        if (refreshAll)
                        {
                            RefreshAll(__instance);
                            FinalizeCleanUp(__instance);
                            return;
                        }
                        if (changedList[mapIndex].Count != 0)
                        {
                            RefreshChanged(__instance);
                        }
                        foreach (var prop in changedList[mapIndex].Intersect(removalHolder))
                        {
                            removalHolder.Remove(prop);
                        }
                        if (removalHolder.Count != 0)
                        {
                            RefreshRemoved(__instance);
                        }
                        FinalizeCleanUp(__instance);
                        calculating = false;
                    }
                }

                internal static void AddFloodGlowFor(CompGlower glower, Color32[] glowGrid)
                {
                    int mapIndex = glower.parent.Map.Index;
                    GlowFlooder flooder = glower.parent.Map.glowFlooder;
                    if (Current.ProgramState == ProgramState.Playing)
                    {
                        GlowerPorperties prop = GlowerPorperties.GetGlowerPorperties(glower);
                        if (glower == null)
                        {
                            return;
                        }
                        // -1. CASE:
                        //   - it's hopeless just redo it all.
                        if (refreshAll)
                        {
                            return;
                        }
                        // 0. CASE:
                        //   - Nothing happended expect maybe addition.
                        if (prop.drawen && removalList[mapIndex].Count == 0 && changedList[mapIndex].Count == 0)
                        {
                            return;
                        }
                        // 1. CASE:
                        //   - Adding new lights only.
                        if (prop.drawen == false)
                        {
                            prop.drawen = true;
                            flooder.AddFloodGlowFor(glower, glowGrid);

                            // 1.1 CASE:
                            //   - Adding new lights while removing or redrawing others.
                            if (removalList[mapIndex].Count != 0 || changedList[mapIndex].Count != 0)
                                flooder.AddFloodGlowFor(glower, glowGridTemp);
                            return;
                        }
                        // 2. CASE:
                        //   - Removing lights only.
                        if (removalList[mapIndex].Count != 0)
                        {
                            foreach (var other in removalList[mapIndex])
                                if (other.Inersects(prop) && other != prop)
                                {
                                    removalHolder.Add(prop);
                                    break;
                                }
                            return;
                        }
                    }
                    else
                    {
                        flooder.AddFloodGlowFor(glower, glowGrid);
                    }
                }

                private static void RefreshAll(GlowGrid instance)
                {
                    Map map = instance.map;
                    glowGridEmpty.CopyTo(instance.glowGrid, 0);
                    glowGridEmpty.CopyTo(instance.glowGridNoCavePlants, 0);
                    foreach (CompGlower litGlower in instance.litGlowers)
                    {
                        if (litGlower == null)
                            continue;
                        map.glowFlooder.AddFloodGlowFor(litGlower, instance.glowGrid);
                        if (litGlower.parent.def.category != ThingCategory.Plant || !litGlower.parent.def.plant.cavePlant)
                        {
                            map.glowFlooder.AddFloodGlowFor(litGlower, instance.glowGridNoCavePlants);
                        }
                    }
                }

                private static void RefreshChanged(GlowGrid instance)
                {
                    int mapIndex = instance.map.Index;
                    GlowFlooder flooder = instance.map.glowFlooder;
                    foreach (var changedProp in changedList[mapIndex])
                        foreach (var index in changedProp.indices)
                        {
                            instance.glowGrid[index] = glowGridTemp[index];
                        }
                    foreach (var prop in changedList[mapIndex])
                        if (prop != null)
                        {
                            flooder.AddFloodGlowFor(prop.glower, glowGridTemp);
                        }
                    foreach (var changedProp in changedList[mapIndex])
                        foreach (var index in changedProp.indices)
                        {
                            instance.glowGrid[index] = glowGridTemp[index];
                        }
                }

                private static void CleanRemoved(GlowGrid instance)
                {
                    int mapIndex = instance.map.Index;
                    GlowFlooder flooder = instance.map.glowFlooder;

                    foreach (var removedProp in removalList[mapIndex])
                        foreach (var index in removedProp.indices)
                        {
                            instance.glowGrid[index] = new Color32(0, 0, 0, 0);
                        }
                }

                private static void RefreshRemoved(GlowGrid instance)
                {
                    int mapIndex = instance.map.Index;
                    GlowFlooder flooder = instance.map.glowFlooder;

                    void DrawCell(int index, string message = "")
                    {
                        if (Finder.debug && Finder.drawGlowerUpdates)
                        {
                            IntVec3 cell = instance.map.cellIndices.IndexToCell(index);
                            instance.map.debugDrawer.FlashCell(cell, colorPct: 0.1f, duration: 100, text: message);
                        }
                    }

                    HashSet<int> removedIndices = new HashSet<int>();
                    foreach (var removedProp in removalList[mapIndex])
                        foreach (var index in removedProp.indices)
                        {
                            removedIndices.Add(index);
                            instance.glowGrid[index] = glowGridTemp[index];
                            DrawCell(index, "0_");
                        }
                    foreach (var prop in removalHolder)
                        if (prop != null && !removalList[mapIndex].Contains(prop))
                        {
                            flooder.AddFloodGlowFor(prop.glower, glowGridTemp);
                        }
                    foreach (var prop in removalHolder)
                        foreach (var index in removedIndices)
                            if (prop.indices.Contains(index))
                            {
                                instance.glowGrid[index] = glowGridTemp[index];
                                DrawCell(index, "_0");
                            }
                }

                private static void FinalizeCleanUp(GlowGrid instance)
                {
                    var map = instance.map;
                    var mapIndex = map.Index;
                    changedList[mapIndex].Clear();
                    removalList[mapIndex].Clear();
                    removalIndices[mapIndex].Clear();
                    removalHolder.Clear();
                }

                private static void FixGridTemp(ref Color32[] aGrid)
                {
                    if (glowGridTemp == null || aGrid.Length != glowGridTemp.Length)
                    {
                        glowGridTemp = new Color32[aGrid.Length];
                        glowGridEmpty = new Color32[aGrid.Length];

                        var limit = aGrid.Length;
                        for (int i = 0; i < limit; i++)
                            glowGridEmpty[i] = new Color32(0, 0, 0, 0);
                    }
                    glowGridEmpty.CopyTo(glowGridTemp, 0);
                }

                private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
                {
                    var codes = instructions.ToList();
                    var finished = false;
                    var mNumGridCells = AccessTools.PropertyGetter(typeof(CellIndices), nameof(CellIndices.NumGridCells));
                    var mAddFloodGlowFor = AccessTools.Method(typeof(GlowFlooder), nameof(GlowFlooder.AddFloodGlowFor));
                    var fMap = AccessTools.Field(typeof(GlowGrid), nameof(GlowGrid.map));
                    var fGlowFlooder = AccessTools.Field(typeof(Map), nameof(Map.glowFlooder));
                    for (int i = 0; i < codes.Count; i++)
                    {
                        CodeInstruction code = codes[i];
                        if (!finished)
                        {
                            if (code.Calls(mNumGridCells))
                            {
                                yield return codes[i];
                                yield return new CodeInstruction(OpCodes.Pop);
                                yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                                continue;
                            }

                            if (i + 2 < codes.Count
                                && codes[i].opcode == OpCodes.Ldarg_0
                                && codes[i + 1].opcode == OpCodes.Ldfld
                                && codes[i + 1].OperandIs(fMap)
                                && codes[i + 2].opcode == OpCodes.Ldfld
                                && codes[i + 2].OperandIs(fGlowFlooder))
                            {
                                i += 2;
                                continue;
                            }

                            if (code.Calls(mAddFloodGlowFor))
                            {
                                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RecalculateAllGlow_Patch), nameof(RecalculateAllGlow_Patch.AddFloodGlowFor)));
                                finished = true;
                                continue;
                            }
                        }
                        yield return code;
                    }
                }
            }

            [HarmonyPatch(typeof(GlowGrid), nameof(GlowGrid.MarkGlowGridDirty))]
            internal static class MarkGlowGridDirty_Patch
            {
                public static void Prefix(GlowGrid __instance, IntVec3 loc)
                {
                    if (Current.ProgramState == ProgramState.Playing)
                    {
                        var map = __instance.map;
                        var mapIndex = map.Index;
                        if (TryRegisterMap(map))
                        {
                            if (!loc.IsValid || !loc.InBounds(map))
                            {
                                return;
                            }
                            if (deregister || calculating || register)
                            {
                                return;
                            }
                            if (Finder.debug) Log.Message(string.Format("ROCKETMAN: Map glow grid dirty at {0}", loc));
                            Vector3 changePoint = loc.ToVector3();
                            GlowerPorperties[] glowers = props[mapIndex].Values.ToArray();
                            foreach (var prop in glowers)
                                if (prop.Contains(changePoint))
                                {
                                    changedList[mapIndex].Add(prop);
                                }
                            while (true)
                            {
                                var count = changedList[mapIndex].Count;
                                foreach (GlowerPorperties prop in changedList[mapIndex].ToArray())
                                    for (int i = 0; i < glowers.Length; i++)
                                    {
                                        var other = glowers[i];
                                        if (true
                                            && other != null
                                            && prop != other
                                            && other.Inersects(prop))
                                        {
                                            changedList[mapIndex].Add(other);
                                            glowers[i] = null;
                                        }
                                    }
                                if (count == changedList[mapIndex].Count)
                                    break;
                            }
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(GlowFlooder), nameof(GlowFlooder.AddFloodGlowFor))]
            internal static class AddFloodGlow_Patch
            {
                internal static void Prefix(CompGlower theGlower)
                {
                    currentProp = GlowerPorperties.GetGlowerPorperties(theGlower);
                    currentProp.Update();
                }

                internal static void Postfix(CompGlower theGlower)
                {
                    if (currentProp == null && theGlower == null)
                    {
                        Log.Warning("ROCKETMAN: AddFloodGlow_Patch with null currentProp");
                        return;
                    }
                    if (currentProp == null)
                    {
                        currentProp = GlowerPorperties.GetGlowerPorperties(theGlower);
                        if (currentProp == null)
                            throw new InvalidDataException("ROCKETMAN: AddFloodGlow_Patch with null currentProp");
                    }
                    currentProp.FinishUpdate();
                    currentProp.drawen = true;
                    currentProp = null;
                }
            }

            [HarmonyPatch(typeof(GlowFlooder), nameof(GlowFlooder.SetGlowGridFromDist))]
            internal static class SetGlowGridFromDist_Patch
            {
                internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
                {
                    var codes = instructions.ToList();

                    for (int i = 0; i < codes.Count - 1; i++)
                        yield return codes[i];

                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SetGlowGridFromDist_Patch), nameof(SetGlowGridFromDist_Patch.PushIndex)));
                    yield return codes.Last();
                }

                internal static void PushIndex(int index, ColorInt color, float distance)
                {
                    Map map = currentProp.glower.parent.Map;
                    currentProp.indices.Add(index);
                    if (Finder.debug && Finder.drawGlowerUpdates)
                    {
                        IntVec3 cell = map.cellIndices.IndexToCell(index);
                        map.debugDrawer.FlashCell(cell, colorPct: 0.1f, duration: 100, text: "a");
                    }
                }
            }
        }
    }
}
