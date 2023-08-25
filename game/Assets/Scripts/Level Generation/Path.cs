using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Game
{
    public class Path
    {
        public Vector2Int LastTile { get; private set; }
        public Vector2Int LastDir { get; private set; }
        public float Angle { get; private set; }

        public GameObject PathEventPrefab;
        public GameObject EventIndicator;

        private bool _isBossArea = false;

        public Path(Vector2Int lastTile, float angle, Vector2Int dir, WorldPrefabsSO worldPrefabs, bool areaCanBeShop)
        {
            LastTile = lastTile;
            Angle = angle;
            LastDir = dir;
            
            // Decide the event
            float bossChance = Mathf.Sqrt(RunManager.Instance.KilledThisLevel-10) * 0.25f;
            if(areaCanBeShop && Random.value < 0.3f)
            {
                PathEventPrefab = worldPrefabs.ShopArea;
                EventIndicator = worldPrefabs.ShopIndicator;
            }
            else if(areaCanBeShop && Random.value < 0.1f)
            {
                PathEventPrefab = WorldPrefabsSO.RandomGOFromList(worldPrefabs.TilePathT);
            }
            else if(Random.value < bossChance)
            {
                _isBossArea = true;
                PathEventPrefab = worldPrefabs.BossArea;
                EventIndicator = worldPrefabs.BossIndicator;
            }
            else
            {
                PathEventPrefab = worldPrefabs.ChooseCombatArea(RunManager.Instance.KilledThisLevel);
            }
        }

        public void ContinuePath(Vector2Int coordsChange, int generationDist, List<Vector2Int> toCreate, WorldPrefabsSO worldPrefabs,
            ref Dictionary<Vector2Int, IMapBlock> map, ref List<Path> paths, ref List<Area> areas, Transform parent)
        {
            Vector2Int nextTile = LastTile + LastDir;
            Vector2Int root = Backtrack(generationDist / 2, map, out bool _);

            // Decide if this tile should be an event
            if (!RunManager.Instance.BossKilled && areas.Count == 0 && CombatFella.AllTheFellas.Count == 1 
                && (!_isBossArea || LastDir.y == 0)) // Make sure if its a boss area it generates to the side
            {
                paths.Remove(this);
                areas.Add(new Area(LastTile, LastDir, PathEventPrefab, worldPrefabs, ref map, ref paths, parent));

                return;
            }

            // Continue path
            while (toCreate.Contains(nextTile))
            {
                float goingToPathAngle = -Vector2.SignedAngle(coordsChange, Quaternion.Euler(0f, 0f, -Angle) * Vector2.up);

                // Add new tiles to the side, until the angle from player to tile excedes the path angle
                float goingToTileAngle = -Vector2.SignedAngle(coordsChange, nextTile - root);
                Vector2Int nextTileDir;
                if (Mathf.Abs(goingToTileAngle) < Mathf.Abs(goingToPathAngle))
                {
                    nextTileDir = Misc.RotateV2Int(coordsChange, Misc.Sign(goingToPathAngle));
                }
                else
                {
                    nextTileDir = coordsChange;
                }

                // Make the new tile
                map[nextTile] = new Tile(nextTile, IMapBlock.BlockType.Path, new Vector2Int[] { -LastDir, nextTileDir }, worldPrefabs, parent);

                // Update the path variables
                LastTile = nextTile;
                LastDir = nextTileDir;

                nextTile += nextTileDir;
            }
        }

        public void TrimForgotenPath(Vector2Int playerPos, int generationRange, ref Dictionary<Vector2Int, IMapBlock> map, ref List<Path> paths)
        {
            while(LastTile.x <= (playerPos.x - generationRange) || LastTile.x >= (playerPos.x + generationRange) 
                || LastTile.y <= (playerPos.y - generationRange) || LastTile.y >= (playerPos.y + generationRange))
            {
                Vector2Int newTile = Backtrack(1, map, out bool _);

                if (LastTile-newTile == Vector2Int.zero) 
                {
                    for(int i = 0; i < paths.Count; i++)
                    {
                        if (paths[i].LastTile == newTile)
                        {
                            paths.Remove(paths[i]);
                            i--;
                        }
                    }
                    break;
                }
                if (map[newTile].GetBlockType() != IMapBlock.BlockType.Path) break;

                map[LastTile].Delete(ref map);  
                map.Remove(LastTile);

                LastDir = LastTile - newTile;
                LastTile = newTile;
            }
        }

        // <symmary> Find coords of a tile n times back, or until a t split </summary>
        public Vector2Int Backtrack(int n, Dictionary<Vector2Int, IMapBlock> map, out bool earlyEnd)
        {
            earlyEnd= false;

            if(map[LastTile].GetConnectionDirs().Length != 2)
            {
                earlyEnd = true;
                return LastTile;
            }

            Vector2Int wentDir = map[LastTile].GetConnectionDirs()[0] == LastDir ? map[LastTile].GetConnectionDirs()[1] : map[LastTile].GetConnectionDirs()[0];
            Vector2Int checkingTile = LastTile + wentDir; 
            if (!map.ContainsKey(checkingTile))
            {
                return checkingTile - wentDir;
            }

            while (n > 1)
            {
                n--;

                if (map[checkingTile].GetConnectionDirs().Length != 2)
                {
                    earlyEnd = true;
                    return checkingTile;
                }

                wentDir = -wentDir == map[checkingTile].GetConnectionDirs()[0] ? map[checkingTile].GetConnectionDirs()[1] : map[checkingTile].GetConnectionDirs()[0];
                checkingTile += wentDir;
                if (!map.ContainsKey(checkingTile))
                {
                    return checkingTile - wentDir;
                }
            }

            return checkingTile;
        }

        public float AngleFromBacktrack(int n, Dictionary<Vector2Int, IMapBlock> map)
        {
            Vector2Int backtrackedCoords = Backtrack(n, map, out _);
            Vector2Int dif= LastTile - backtrackedCoords;
            return -Vector2.SignedAngle(Vector2.up, dif);
        }

        public void UpdateAngle(List<Path> paths, Dictionary<Vector2Int, IMapBlock> map, int generationDist, float strength = 10f)
        {
            Angle = AngleFromBacktrack(generationDist / 2, map);
            float repellForce = 0f;
            foreach (var path in paths)
            {
                if (path == this) continue;
                float dif = Mathf.DeltaAngle(Angle, path.Angle);
                float forceT = 1-(Mathf.Abs(dif) / 180f);
                repellForce += forceT*forceT* forceT * forceT * 30f * -Misc.Sign(dif);
            }
            Angle += strength * (2 * Mathf.PerlinNoise(0.01f * LastTile.x, 0.01f * LastTile.y) - 1) + repellForce;
        }
    }
}
