using Verse;
using System.Collections.Generic;
using static RocketMan.RocketShip;
using RimWorld.Planet;
using HarmonyLib;

namespace RocketMan.Core
{
    public partial class Main
    {
        [HarmonyPatch(typeof(WorldReachability), nameof(WorldReachability.CanReach), new[] { typeof(int), typeof(int) })]
        public static class WorldReachability_CanReach_Patch
        {
            internal static HashSet<int> visitedTiles;

            internal static int visitedTilesCount = 0;
            internal static int islandCounter = 0;
            internal static int[] tilesToIsland;

            internal static World world;

            private static Dictionary<int, List<int>> islands = new Dictionary<int, List<int>>();
            private static bool finished = false;

            internal static void GenerateIslands()
            {
                var world = Find.World;
                var offsets = Find.WorldGrid.tileIDToNeighbors_offsets;
                var tilesIDsFromNeighbor = Find.WorldGrid.tileIDToNeighbors_values;

                Queue<Pair<int, int>> queue = new Queue<Pair<int, int>>(100);

                var passableTiles = new List<int>();

                for (int i = 0; i < Find.WorldGrid.TilesCount; i++)
                    if (!world.Impassable(i))
                    {
                        passableTiles.Add(i);
                    }

                var currentIslandCounter = 0;
                IEnumerable<int> GetNeighbors(int tile)
                {
                    int limit = (tile + 1 < offsets.Count) ? offsets[tile + 1] : tilesIDsFromNeighbor.Count;
                    for (int k = offsets[tile]; k < limit; k++)
                        yield return tilesIDsFromNeighbor[k];
                }

                while ((visitedTilesCount < passableTiles.Count && world == Find.World) || queue.Count > 0)
                {
                    if (queue.Count > 0)
                    {
                        var current = queue.Dequeue();
                        var currentIsland = current.First;
                        var currentTile = current.Second;
                        visitedTilesCount++;
                        visitedTiles.Add(currentTile);

                        tilesToIsland[currentTile] = currentIsland;
                        foreach (int neighbor in GetNeighbors(currentTile))
                        {
                            if ((tilesToIsland[neighbor] == currentIsland && tilesToIsland[neighbor] != 0) || world.Impassable(neighbor))
                                continue;
                            else if (tilesToIsland[neighbor] == 0)
                            {
                                tilesToIsland[neighbor] = currentIsland;
                                queue.Enqueue(new Pair<int, int>(currentIsland, neighbor));
                                currentIslandCounter++;
                            }
                            else
                            {
                                var otherIsland = tilesToIsland[neighbor];
                                for (int i = 0; i < tilesToIsland.Length; i++)
                                    if (tilesToIsland[i] == otherIsland)
                                    {
                                        tilesToIsland[i] = currentIsland;
                                        currentIslandCounter++;
                                    }
                            }
                        }
                    }
                    else
                    {
                        if (Prefs.DevMode)
                            Log.Message(string.Format("ROCKETMAN: Island counter {0}, visited {1}", currentIslandCounter, visitedTilesCount));
                        var randomTile = passableTiles.RandomElement();
                        if (Find.World.Impassable(randomTile))
                            continue;
                        if (tilesToIsland[randomTile] != 0)
                            continue;
                        var nextIsland = islandCounter++;
                        currentIslandCounter = 1;
                        queue.Enqueue(new Pair<int, int>(nextIsland, randomTile));
                    }
                }

                for (int i = 0; i < tilesToIsland.Length; i++)
                    if (islands.TryGetValue(tilesToIsland[i], out var island))
                        island.Add(i);
                    else
                    {
                        islands[tilesToIsland[i]] = new List<int>();
                        islands[tilesToIsland[i]].Add(i);
                    }

                if (world != Find.World) return;
                finished = true;
                if (Prefs.DevMode)
                {
                    Log.Message(string.Format("ROCKETMAN: Island counter {0}, visited {1}", currentIslandCounter, visitedTilesCount));
                    Log.Message(string.Format("ROCKETMAN: FINISHED BUILDING ISLANDS!, {0}, {1}, {2}, {3}", islandCounter, visitedTilesCount, passableTiles.Count, currentIslandCounter));
                }
            }

            internal static void Initialize()
            {
                world = Find.World;
                tilesToIsland = new int[Find.WorldGrid.TilesCount];
                visitedTilesCount = 0;
                visitedTiles = new HashSet<int>();
                islandCounter = 1;
                islands.Clear();

                GenerateIslands();
            }

            internal static bool Prefix(ref bool __result, int startTile, int destTile)
            {
                if (Finder.enabled)
                {
                    if (world != Find.World)
                    {
                        if (Finder.debug) Log.Message("ROCKETMAN: Creating world map cache");
                        Initialize();
                    }
                    if (!finished)
                    {
                        if (Finder.debug) Log.Message("ROCKETMAN: Tried to call WorldReachability while still processing");
                        return true;
                    }
                    if (tilesToIsland[startTile] == 0 || tilesToIsland[destTile] == 0 || tilesToIsland[startTile] != tilesToIsland[destTile])
                    {
                        if (Finder.debug) Log.Message("ROCKETMAN: Not Allowed");
                        __result = false;
                    }
                    if (tilesToIsland[startTile] == tilesToIsland[destTile])
                    {
                        if (Finder.debug) Log.Message("ROCKETMAN: Allowed");
                        __result = true;
                    }
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
    }
}
