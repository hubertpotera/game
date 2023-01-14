using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.Analytics;

namespace Game
{
    public class LevelGenerator : MonoBehaviour
    {
        // ---------- VARIABLES ----------
        #region variables

        [SerializeField]
        private Transform _player;
        [SerializeField]
        private int _generationDist;
        [SerializeField]
        private int _treeDistFromPath;
        [SerializeField]
        [Range(1f, 10f)]
        private float _tSplitChanceIncrease;
        [SerializeField]
        [Range(0f, 1f)]
        private float _tSplitInitialChance;


        private Vector2Int _prevPlayerCoords = Vector2Int.zero;

        private Dictionary<Vector2Int, Tile> _map;
        private List<float> _pathAngles;
        private List<Vector2Int> _lastPathTile;
        private List<Vector2Int> _lastPathDir;
        private float _tSplitChance;        
        
        #endregion





        // ------------ INIT -------------
        #region init

        private void Awake() 
        {
            _tSplitChance = _tSplitInitialChance;
            _pathAngles = new List<float>();
            _lastPathTile = new List<Vector2Int>();
            _lastPathDir = new List<Vector2Int>();
            GenerateStart();
        }

        #endregion





        // ------------ LOOPS ------------
        #region loops

        private void Update() 
        {
            Vector2Int playerCoords = PosToCoords(_player.position);
            if(playerCoords != _prevPlayerCoords)
            {
                UpdateMap(_prevPlayerCoords, playerCoords);
                _prevPlayerCoords = playerCoords;
            }
        }

        #endregion





        // ----------- METHODS -----------
        #region  methods

        private void UpdateMap(Vector2Int prevCoords, Vector2Int newCoords)
        {
            Vector2Int coordsChange = newCoords-prevCoords;

            if((prevCoords - newCoords).sqrMagnitude > 1)
            {
                // Right now this only works when going diagonaly
                // May cause a bug when going a bigger distance than just that
                Vector2Int nextCoords = prevCoords + new Vector2Int(coordsChange.x, 0);
                UpdateMap(prevCoords, nextCoords);
                UpdateMap(nextCoords, nextCoords + new Vector2Int(0, coordsChange.y));
                return;
            }

            List<Vector2Int> toDelete = new List<Vector2Int>();
            List<Vector2Int> toCreate = new List<Vector2Int>();
            if(coordsChange.y == 0) // Horizonatal movement
            {
                toDelete = GetVerticalLine(newCoords - coordsChange*_generationDist, _generationDist);
                toCreate = GetVerticalLine(newCoords + coordsChange*(_generationDist-1), _generationDist);
            }
            else // Vertical movement
            {
                toDelete = GetHorizontalLine(newCoords - coordsChange*_generationDist, _generationDist);
                toCreate = GetHorizontalLine(newCoords + coordsChange*(_generationDist-1), _generationDist);
            }

            // Update paths
            Debug.Log("count" + _lastPathTile.Count);
            for(int i = 0; i < _lastPathTile.Count; i++)
            {
                // Deleting and finding new last tiles
                if(toDelete.Contains(_lastPathTile[i]))
                {
                    FindLostLastPathTile(i, ref toDelete);
                    continue;
                }

                // Continuing the path
                Vector2Int nextTile = _lastPathTile[i] + _lastPathDir[i];
                if(toCreate.Contains(nextTile)) 
                {
                    _pathAngles[i] += 10f*(2*Mathf.PerlinNoise(0.01f*newCoords.x, 0.01f*newCoords.y)-1);
                    ContinuePath(i, nextTile, coordsChange, newCoords, ref toCreate);
                }
            }

            // Merging merged paths
            for (int i = 0; i < _lastPathTile.Count; i++)
            {
                for (int j = 0; j < _lastPathTile.Count; j++)
                {
                    if (i == j) continue;
                    if (_lastPathTile[i] == _lastPathTile[j])
                    {
                        Debug.Log("removing");
                        _pathAngles.RemoveAt(j);
                        _lastPathTile.RemoveAt(j);
                        _lastPathDir.RemoveAt(j);
                    }
                }
            }

            // Finally, place and remove all tiles that weren't handlet yet
            for (int i = 0; i < toDelete.Count; i++)
            {
                _map[toDelete[i]].Delete();
                _map.Remove(toDelete[i]);
            }
            for(int i = 0; i < toCreate.Count; i++)
            {
                _map[toCreate[i]] = new Tile(toCreate[i], Tile.TileType.Empty, new Vector2Int[0]);
            }
            
            // PlaceTrees
            //TODO everything here
            if(coordsChange.y == 0) // Horizonatal movement
            {
                toDelete = GetVerticalLine(newCoords - coordsChange*(_generationDist-_treeDistFromPath), _generationDist-_treeDistFromPath);
                toCreate = GetVerticalLine(newCoords + coordsChange*(_generationDist-1-_treeDistFromPath), _generationDist-_treeDistFromPath);
            }
            else // Vertical movement
            {
                toDelete = GetHorizontalLine(newCoords - coordsChange*(_generationDist-_treeDistFromPath), _generationDist-_treeDistFromPath);
                toCreate = GetHorizontalLine(newCoords + coordsChange*(_generationDist-1-_treeDistFromPath), _generationDist-_treeDistFromPath);
            }

            for (int i = 0; i < toCreate.Count; i++)
            {
                bool valid = true;
                for (int x = -_treeDistFromPath; x <= _treeDistFromPath; x++)
                {
                    for (int y = -_treeDistFromPath; y <= _treeDistFromPath; y++)
                    {
                        Vector2Int check = toCreate[i] + new Vector2Int(x, y);
                        if(_map[check].Type != Tile.TileType.Empty)
                        {
                            valid = false;
                            break;
                        }
                    }
                    if(!valid) break;
                }
                if(!valid) continue;

                _map[toCreate[i]].PlaceTree();
            }
            for (int i = 0; i < toDelete.Count; i++)
            {
                _map[toDelete[i]].RemovePlaceable();
            }
            return;
        }

        private void GenerateStart()
        {
            Vector2Int playerCoords = PosToCoords(_player.position);

            _map = new Dictionary<Vector2Int, Tile>();
            for (int x = -_generationDist+1; x < _generationDist; x++)
            {
                for (int y = -_generationDist+1; y < _generationDist; y++)
                {
                    Vector2Int coords = playerCoords + new Vector2Int(x, y);

                    if(x == 0) _map.Add(coords, new Tile(coords, Tile.TileType.Path, new Vector2Int[]{Vector2Int.up, Vector2Int.down}));
                    else _map.Add(coords, new Tile(coords, Tile.TileType.Empty, new Vector2Int[0]));
                }
            }
            _pathAngles.Add(0);
            _pathAngles.Add(180);
            _lastPathTile.Add(new Vector2Int(0, _generationDist-1));
            _lastPathTile.Add(new Vector2Int(0, -_generationDist+1));
            _lastPathDir.Add(Vector2Int.up);
            _lastPathDir.Add(Vector2Int.down);
        }

        private void FindLostLastPathTile(int pathIdx, ref List<Vector2Int> toDelete)
        {
            int count = 0;
            // Find next viable path tile, that isn't set for deletion
            Vector2Int theTile = _lastPathTile[pathIdx];
            while (toDelete.Contains(theTile))
            {
                bool found = false;
                foreach (var dir in _map[theTile].ConnectionDirs)
                {
                    if(!_map.ContainsKey(theTile+dir)) continue;
                    if(_map[theTile+dir].Type == Tile.TileType.Path)
                    {
                        // Delete the path
                        _map[theTile].Delete();
                        _map.Remove(theTile);
                        toDelete.Remove(theTile);
                        // Continue check for the next tile
                        theTile = theTile+dir;
                        // Update path variables
                        _lastPathTile[pathIdx] = theTile;
                        _lastPathDir[pathIdx] = -dir;
                        found= true;
                        break;
                    }
                }
                if(!found)
                {
                    _pathAngles.RemoveAt(pathIdx);
                    _lastPathTile.RemoveAt(pathIdx);
                    _lastPathDir.RemoveAt(pathIdx);

                    _map[theTile].Delete();
                    _map.Remove(theTile);
                    toDelete.Remove(theTile);
                    return;
                }

                count++; //TODO i think this isnt necesary now
                if(count > 100)
                {
                    Debug.LogError("Infinite Loop when finding lost path");
                    break;
                }
            }

            // Calculate the path angle
            Vector2 avgEndTile = Vector2.zero;
            foreach (var tile in _lastPathTile)
            {
                avgEndTile += tile;
            }
            avgEndTile /= _lastPathTile.Count;
            Vector2 pathDir = _lastPathTile[pathIdx] - avgEndTile;
            _pathAngles[pathIdx] = -Vector2.SignedAngle(Vector2.up, pathDir);
        }

        private void ContinuePath(int pathIdx, Vector2Int nextTile, Vector2Int coordsChange, Vector2Int newCoords, ref List<Vector2Int> toCreate)
        {
            do
            {
                float goingToPathAngle = -Vector2.SignedAngle(coordsChange, Quaternion.Euler(0f, 0f, -_pathAngles[pathIdx]) * Vector2.up);

                // Decide if this tile should be a T Split
                _tSplitChance *= _tSplitChanceIncrease;
                if(Random.value < _tSplitChance)
                {
                    Debug.Log("Split");
                    _tSplitChance = _tSplitInitialChance;

                    // Continue path turning
                    Vector2Int turn = Misc.RotateV2Int(coordsChange, Sign(goingToPathAngle));
                    _map[nextTile] = new Tile(nextTile, Tile.TileType.Path, 
                        new Vector2Int[] { -_lastPathDir[pathIdx], turn, coordsChange});
                    toCreate.Remove(nextTile);
                    float continueAngle = -Vector2.SignedAngle(Vector2.up, turn);
                    _pathAngles[pathIdx] = continueAngle;
                    _lastPathTile[pathIdx] = nextTile;
                    _lastPathDir[pathIdx] = turn;

                    //Create new path
                    float splitAngle = -Vector2.SignedAngle(Vector2.up, coordsChange);
                    _pathAngles.Add(splitAngle);
                    _lastPathTile.Add(nextTile);
                    _lastPathDir.Add(coordsChange);
                    Debug.Log(coordsChange);
                    Debug.Log(turn + " " + coordsChange);
                    Debug.Log(continueAngle + " " + splitAngle);

                    nextTile += turn;

                    continue;
                }


                // Add new tiles to the side, until the angle from player to tile excedes the path angle
                float goingToTileAngle = -Vector2.SignedAngle(coordsChange, nextTile-newCoords);
                Vector2Int nextTileDir = Vector2Int.zero;
                if(Mathf.Abs(goingToTileAngle) < Mathf.Abs(goingToPathAngle))
                {
                    nextTileDir = Misc.RotateV2Int(coordsChange, Sign(goingToPathAngle));
                }
                else 
                {
                    nextTileDir = coordsChange;
                }

                // Make the new tile
                _map[nextTile] = new Tile(nextTile, Tile.TileType.Path, new Vector2Int[]{-_lastPathDir[pathIdx],nextTileDir});
                toCreate.Remove(nextTile);

                // Update the path variables
                _lastPathTile[pathIdx] = nextTile;
                _lastPathDir[pathIdx] = nextTileDir;

                nextTile += nextTileDir;
            } while(toCreate.Contains(nextTile));
        }

        /// <summary>Range 1 will return 1 block</summary>
        private List<Vector2Int> GetHorizontalLine(Vector2Int middle, int range)
        {
            if(range <= 0) return new List<Vector2Int>();

            List<Vector2Int> wall = new List<Vector2Int>();
            int i = 0;
            for (int x = -range+1; x < range; x++)
            {
                wall.Add(middle + new Vector2Int(x, 0));
                i++;
            }
            return wall;
        }

        /// <summary>Range 1 will return 1 block</summary>
        private List<Vector2Int> GetVerticalLine(Vector2Int middle, int range)
        {
            if(range <= 0) return new List<Vector2Int>();

            List<Vector2Int> wall = new List<Vector2Int>();
            int i = 0;
            for (int y = -range+1; y < range; y++)
            {
                wall.Add(middle + new Vector2Int(0, y));
                i++;
            }
            return wall;
        }

        private Vector2Int PosToCoords(Vector3 pos)
        {
            Vector2Int coords = new Vector2Int((int)(pos.x/Tile.TileWidth), (int)(pos.z/Tile.TileWidth));
            coords += pos.x < 0 ? Vector2Int.left : Vector2Int.zero; 
            coords += pos.z < 0 ? Vector2Int.down : Vector2Int.zero; 
            return coords;
        }

        private static int Sign(float x)
        {
            return (x < 0f) ? -1 : 1;
        }
        
        private void OnValidate() 
        {
            if(_generationDist < 0) _generationDist = 0;
        }
        
        #endregion
    }
}
