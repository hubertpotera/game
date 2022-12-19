using UnityEngine;
using System.Collections.Generic;

namespace Game
{
    public class LevelGenerator : MonoBehaviour
    {
        // ---------- VARIABLES ----------
        #region variables
        [SerializeField]
        private float test;

        [SerializeField]
        private Transform _player;
        [SerializeField]
        private int _generationDist;
        [SerializeField]
        private int _seed;


        private System.Random _prng;

        private Vector2Int _prevPlayerCoords = Vector2Int.zero;

        private Dictionary<Vector2Int, WorldTile> _map;

        private List<float> _pathAngles;
        private List<Vector2Int> _lastPathTile;
        private List<Vector2Int> _lastPathDir;

        #endregion





        // ------------ INIT -------------
        #region init

        private void Awake() 
        {
            _prng = new System.Random(_seed);
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

        private Vector2Int PosToCoords(Vector3 pos)
        {
            Vector2Int coords = new Vector2Int((int)(pos.x/WorldTile.TileWidth), (int)(pos.z/WorldTile.TileWidth));
            coords += pos.x < 0 ? Vector2Int.left : Vector2Int.zero; 
            coords += pos.z < 0 ? Vector2Int.down : Vector2Int.zero; 
            return coords;
        }

        private void UpdateMap(Vector2Int prevCoords, Vector2Int newCoords)
        {
            Vector2Int change = newCoords-prevCoords;

            if((prevCoords - newCoords).sqrMagnitude > 1)
            {
                // May cause a bug when going a bigger distance than just diagonaly
                Vector2Int nextCoords = prevCoords + new Vector2Int(change.x, 0);
                UpdateMap(prevCoords, nextCoords);
                UpdateMap(nextCoords, nextCoords + new Vector2Int(0, change.y));
                return;
            }

            List<Vector2Int> toDelete = new List<Vector2Int>();
            List<Vector2Int> toCreate = new List<Vector2Int>();
            if(change.y == 0)
            {
                // Horizonatal movement
                toDelete = GetVerticalWall(newCoords - change*_generationDist, _generationDist);
                toCreate = GetVerticalWall(newCoords + change*(_generationDist-1), _generationDist);
            }
            else
            {
                // Vertical movement
                toDelete = GetHorizontalWall(newCoords - change*_generationDist, _generationDist);
                toCreate = GetHorizontalWall(newCoords + change*(_generationDist-1), _generationDist);
            }

            Vector2 middle = newCoords;
            for(int i = 0; i < _pathAngles.Count; i++)
            {
                int count = 0;
                if(toDelete.Contains(_lastPathTile[i]))
                {
                    Vector2Int theTile = _lastPathTile[i];
                    Vector2Int[] dirs = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
                    while (toDelete.Contains(theTile))
                    {
                        foreach (var dir in dirs)
                        {
                            if(!_map.ContainsKey(theTile+dir)) continue;
                            if(_map[theTile+dir].Tile.Connections.Contains(TileSO.Connection.Path))
                            {
                                _map[theTile].Delete();
                                _map.Remove(theTile);
                                toDelete.Remove(theTile);
                                theTile = theTile+dir;
                                _lastPathTile[i] = theTile;
                                _lastPathDir[i] = -dir;
                                break;
                            }
                        }
                        count++;
                        if(count > 100) break;
                    }
                    Vector2 avgEndTile = Vector2.zero;
                    foreach (var tile in _lastPathTile)
                    {
                        avgEndTile += tile;
                    }
                    avgEndTile /= _lastPathTile.Count;
                    Vector2 pathDir = _lastPathTile[i] - avgEndTile;
                    _pathAngles[i] = -Vector2.SignedAngle(Vector2.up, pathDir); //TODO make this good
                    continue;
                }

                Vector2Int nextTile = _lastPathTile[i] + _lastPathDir[i];
                if(toCreate.Contains(nextTile)) 
                {
                    _pathAngles[i] += 10f*(2*Mathf.PerlinNoise(0.01f*newCoords.x, 0.01f*newCoords.y)-1);
                }

                while(toCreate.Contains(nextTile)) 
                {
                    float goingToPathAngle = -Vector2.SignedAngle(change, Quaternion.Euler(0f, 0f, -_pathAngles[i]) * Vector2.up);
                    float goingToTileAngle = -Vector2.SignedAngle(change, nextTile-middle);
                    Vector2Int nextTileDir = Vector2Int.zero;
                    if(Mathf.Abs(goingToTileAngle) < Mathf.Abs(goingToPathAngle)) 
                    {
                        nextTileDir = RotateV2Int(change, Sign(goingToPathAngle));
                    }
                    else 
                    {
                        nextTileDir = change;
                    }
                    _lastPathTile[i] = nextTile;
                    _lastPathDir[i] = nextTileDir;

                    _map[nextTile] = new WorldTile(nextTile, WorldTile.TileType.Path, new Vector2Int[0]);
                    toCreate.Remove(nextTile);

                    nextTile += nextTileDir;
                }
            }

            for(int i = 0; i < toDelete.Count; i++)
            {
                _map[toDelete[i]].Delete();
                _map.Remove(toDelete[i]);
            }
            for(int i = 0; i < toCreate.Count; i++)
            {
                _map[toCreate[i]] = new WorldTile(toCreate[i], WorldTile.TileType.Empty, new Vector2Int[0]);
            }
        }

        private void GenerateStart()
        {
            Vector2Int playerCoords = PosToCoords(_player.position);

            _map = new Dictionary<Vector2Int, WorldTile>();
            for (int x = -_generationDist+1; x < _generationDist; x++)
            {
                for (int y = -_generationDist+1; y < _generationDist; y++)
                {
                    Vector2Int coords = playerCoords + new Vector2Int(x, y);

                    if(x == 0) _map.Add(coords, new WorldTile(coords, WorldTile.TileType.Path, new Vector2Int[0]));
                    else _map.Add(coords, new WorldTile(coords, WorldTile.TileType.Empty, new Vector2Int[0]));
                }
            }
            _pathAngles.Add(0);
            _pathAngles.Add(180);
            _lastPathTile.Add(new Vector2Int(0, _generationDist-1));
            _lastPathTile.Add(new Vector2Int(0, -_generationDist+1));
            _lastPathDir.Add(Vector2Int.up);
            _lastPathDir.Add(Vector2Int.down);
        }

        /// <summary>Range 1 will return 1 block</summary>
        private List<Vector2Int> GetHorizontalWall(Vector2Int middle, int range)
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
        private List<Vector2Int> GetVerticalWall(Vector2Int middle, int range)
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

        /// <summary>1 for right, -1 for left</summary>
        private static Vector2Int RotateV2Int(Vector2Int v, int dir)
        {
            dir = Mathf.Clamp(dir, -1, 1);
            if(v.x == 0)    return new Vector2Int(dir*v.y, 0);
            else            return new Vector2Int(0, dir*-v.x);
        }

        private static Vector2Int RotateV2IntRight(Vector2Int v)
        {
            if(v.x == 0)    return new Vector2Int(v.y, 0);
            else            return new Vector2Int(0, -v.x);
        }

        private static Vector2Int RotateV2IntLeft(Vector2Int v)
        {
            if(v.x == 0)    return new Vector2Int(-v.y, 0);
            else            return new Vector2Int(0, v.x);
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
