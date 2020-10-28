using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using static RocketMan.RocketShip;

namespace RocketMan
{
    public partial class Main
    {
        [HarmonyPatch(typeof(WorldGrid), nameof(WorldGrid.TraversalDistanceBetween))]
        public static class WorldGrid_TraversalDistanceBetween
        {
            private static WorldGrid grid;

            internal static Vector3[] verts;
            internal static int[] offsets;

            private static int target;
            private static object locker = new object();

            private struct QPair : IComparable<QPair>
            {
                public int tile;
                public float cost;
                public int n;
                public int g;

                public QPair(int tile, int n, int cost)
                {
                    this.tile = tile;
                    this.cost = cost;
                    this.g = (int)GetDistance(tile, target);
                    this.n = n;
                }

                public int CompareTo(QPair other)
                {
                    return (n * cost + g).CompareTo(other.n * other.cost + other.g);
                }

                public static bool operator >(QPair operand1, QPair operand2) =>
                    operand1.CompareTo(operand2) == 1;
                public static bool operator <(QPair operand1, QPair operand2) =>
                    operand1.CompareTo(operand2) == -1;
                public static bool operator >=(QPair operand1, QPair operand2) =>
                    operand1.CompareTo(operand2) >= 0;
                public static bool operator <=(QPair operand1, QPair operand2) =>
                    operand1.CompareTo(operand2) <= 0;
            }

            private static Vector3 GetVert(int tile)
            {
                return verts[offsets[tile]];
            }

            private static float GetDistance(int tile, int other)
            {
                return Vector3.Distance(GetVert(tile), GetVert(other));
            }

            private static void Initialize()
            {
                grid = Find.WorldGrid;
                verts = grid.verts.ToArray();
                offsets = grid.tileIDToVerts_offsets.ToArray();
            }

            private static int Search(int start, int end)
            {
                if (!Find.WorldReachability.CanReach(start, end))
                    return int.MaxValue;
                var visited = new bool[grid.TilesCount];
                var visitedCounter = 1;
                var world = Find.World;
                var queue = new FastPriorityQueue<QPair>();
                target = end;
                IEnumerable<int> GetNeighbors(int tile)
                {
                    int limit = (tile + 1 < grid.tileIDToNeighbors_offsets.Count) ? grid.tileIDToNeighbors_offsets[tile + 1] : grid.tileIDToNeighbors_values.Count;
                    for (int k = grid.tileIDToNeighbors_offsets[tile]; k < limit; k++)
                        yield return grid.tileIDToNeighbors_values[k];
                }

                visited[start] = true;
                queue.Push(new QPair() { tile = start, n = 1, cost = grid.tiles[start].biome.movementDifficulty });
                while (queue.Count > 0 && queue.Count < 10000)
                {
                    var current = queue.Pop();
                    var currentTile = current.tile;

                    if (currentTile == end)
                        return current.n;

                    var neighbors = GetNeighbors(currentTile);
                    foreach (var neighbor in neighbors)
                    {
                        if (neighbor == end)
                            return current.n;
                        if (visited[neighbor])
                            continue;
                        // So we don't visit the same shit over and over.
                        visited[neighbor] = true;
                        visitedCounter++;
                        if (world.Impassable(neighbor))
                            continue;
                        queue.Push(new QPair()
                        {
                            n = current.n + 1,
                            tile = neighbor,
                            cost = current.cost + grid.tiles[start].biome.movementDifficulty
                        });
                    }
                }

                throw new Exception(string.Format("ROCKETMAN: target not reachable {0}, visited {1}", queue.Count, visitedCounter));
            }

            internal static bool Prefix(ref int __result, WorldGrid __instance, int start, int end)
            {
                if (Finder.enabled)
                {
                    lock (locker)
                    {
                        if (grid != Find.WorldGrid)
                            Initialize();
                        //
                        // using a* to find the "best" path in "minimal" time
                        if (Finder.debug == true)
                        {
                            Log.Warning(string.Format("ROCKETMAN: [R] travel distance between {1}, {2} is {0}", Search(start, end), start, end));
                        }
                        else
                        {
                            __result = Search(start, end);
                        }
                    }
                    return Finder.debug;
                }
                else
                {
                    return true;
                }
            }

            internal static void Postfix(ref int __result, WorldGrid __instance)
            {
                if (Finder.debug == true)
                {
                    Log.Warning(string.Format("ROCKETMAN: [C] travel distance in vanilla between  is {0}", __result));
                }
            }
        }
    }
}
