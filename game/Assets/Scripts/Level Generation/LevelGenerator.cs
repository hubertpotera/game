using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Game
{
    public class LevelGenerator : MonoBehaviour
    {
        public static LevelGenerator Instance { get; private set; }

        void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError("replacing level gen");
                Destroy(Instance.gameObject);
            }

            _paths= new List<Path>();
            _areas= new List<Area>();

            GenerateStart();
        }

        [SerializeField]
        private WorldPrefabsSO _prefabs;

        [Space]
        public Transform Player;
        [SerializeField]
        private int _generationDist = 16;
        [SerializeField]
        private float _pathNudge = 5f;
        [SerializeField]
        private int _treeDistFromPath = 2;

        public int SeeingRange = 10;

        private Vector2Int _prevPlayerCoords = Vector2Int.zero;

        private Dictionary<Vector2Int, IMapBlock> _map;
        private List<Path> _paths;
        private List<Area> _areas;



        private void Update() 
        {
            if(Player == null) return;
            
            Vector2Int playerCoords = PosToCoords(Player.position);
            if(playerCoords != _prevPlayerCoords)
            {
                UpdateMap(_prevPlayerCoords, playerCoords);
                _prevPlayerCoords = playerCoords;
            }
        }



        public void Regenerate()
        {
            _prevPlayerCoords = Vector2Int.zero;

            Tile.ClearTreePool();
            foreach(Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            
            foreach (var area in _areas)
            {
                area.Destroy(ref _paths, ref _map);
            }
            _areas= new List<Area>();
            _paths= new List<Path>();

            GenerateStart();
        }

        private void UpdateMap(Vector2Int prevCoords, Vector2Int newCoords)
        {
            Vector2Int coordsChange = newCoords-prevCoords;

            if((prevCoords - newCoords).sqrMagnitude > 1)
            {
                Vector2Int coord = prevCoords;
                int dirX = Misc.Sign(coordsChange.x);
                int dirY = Misc.Sign(coordsChange.y);
                for (int i = 0; i < Mathf.Abs(coordsChange.x); i++)
                {
                    UpdateMap(coord, coord + Vector2Int.right * dirX);
                    coord += Vector2Int.right * dirX;
                }
                for (int i = 0; i < Mathf.Abs(coordsChange.y); i++)
                {
                    UpdateMap(coord, coord + Vector2Int.up * dirY);
                    coord += Vector2Int.up * dirY;
                }
                return;
            }

            List<Vector2Int> toCreate;
            List<Vector2Int> toDelete;
            if (coordsChange.y == 0) // Horizonatal movement
            {
                toDelete = GetVerticalLine(newCoords - coordsChange * _generationDist, _generationDist);
                toCreate = GetVerticalLine(newCoords + coordsChange * (_generationDist - 1), _generationDist);
            }
            else // Vertical movement
            {
                toDelete = GetHorizontalLine(newCoords - coordsChange * _generationDist, _generationDist);
                toCreate = GetHorizontalLine(newCoords + coordsChange * (_generationDist - 1), _generationDist);
            }

            // Remove all unimportant (empty) tiles
            for (int i = 0; i < toDelete.Count; i++)
            {
                if (!_map.ContainsKey(toDelete[i])) continue;
                if (_map[toDelete[i]].GetBlockType() != IMapBlock.BlockType.Empty) continue;
                _map[toDelete[i]].Delete(ref _map);
            }

            // Continuing the paths
            for (int i = 0; i < _paths.Count; i++)
            {
                _paths[i].UpdateAngle(_paths, _map, _generationDist, _pathNudge);
                if (!toCreate.Contains(_paths[i].LastTile + _paths[i].LastDir)) continue;
                _paths[i].ContinuePath(coordsChange, _generationDist, toCreate, _prefabs, ref _map, ref _paths, ref _areas, transform);
            }

            // Forgeting not seen paths
            for (int i = 0; i < _paths.Count; i++)
            {
                _paths[i].TrimForgotenPath(newCoords, _generationDist, ref _map, ref _paths);
            }

            // Forgeting areas
            if(!RunManager.Instance.BossKilled)
            {
                for (int i = 0; i < _areas.Count; i++)
                {
                    if(_areas[i].ConciderDeletion(newCoords, coordsChange, _generationDist, ref _paths, ref _map, _prefabs))
                        _areas.RemoveAt(i);
                }
            }

            // Fill the rest with empty tiles
            for (int i = 0; i < toCreate.Count; i++)
            {
                if (_map.ContainsKey(toCreate[i])) continue;
                _map[toCreate[i]] = (IMapBlock) new Tile(toCreate[i], IMapBlock.BlockType.Empty, new Vector2Int[0], _prefabs, transform);
            }

            PlaceTrees(coordsChange, newCoords);
        }

        private void PlaceTrees(Vector2Int coordsChange, Vector2Int newCoords)
        {
            List<Vector2Int> toDelete;
            List<Vector2Int> toCreate;
            if (coordsChange.y == 0) // Horizonatal movement
            {
                toDelete = GetVerticalLine(newCoords - coordsChange * (_generationDist - _treeDistFromPath), _generationDist - _treeDistFromPath);
                toCreate = GetVerticalLine(newCoords + coordsChange * (_generationDist - 1 - _treeDistFromPath), _generationDist - _treeDistFromPath);
            }
            else // Vertical movement
            {
                toDelete = GetHorizontalLine(newCoords - coordsChange * (_generationDist - _treeDistFromPath), _generationDist - _treeDistFromPath);
                toCreate = GetHorizontalLine(newCoords + coordsChange * (_generationDist - 1 - _treeDistFromPath), _generationDist - _treeDistFromPath);
            }

            for (int i = 0; i < toCreate.Count; i++)
            {
                bool valid = true;
                for (int x = -_treeDistFromPath; x <= _treeDistFromPath; x++)
                {
                    for (int y = -_treeDistFromPath; y <= _treeDistFromPath; y++)
                    {
                        Vector2Int check = toCreate[i] + new Vector2Int(x, y);
                        if (_map[check].GetBlockType() != IMapBlock.BlockType.Empty)
                        {
                            valid = false;
                            break;
                        }
                    }
                    if (!valid) break;
                }
                if (!valid) continue;
                _map[toCreate[i]].PlaceTree(_prefabs, (_generationDist-_treeDistFromPath)*2-1, transform);
            }

            for (int i = 0; i < toDelete.Count; i++)
            {
                _map[toDelete[i]].RemovePlaceable();
            }
        }

        private void GenerateStart()
        {
            _map = new Dictionary<Vector2Int, IMapBlock>();
            for (int x = -_generationDist + 1; x < _generationDist; x++)
            {
                for (int y = -_generationDist + 1; y < _generationDist; y++)
                {
                    Vector2Int coords = new(x, y);

                    if (x == 0) _map.Add(coords, new Tile(coords, IMapBlock.BlockType.Path, new Vector2Int[] { Vector2Int.up, Vector2Int.down }, _prefabs, transform));
                    else _map.Add(coords, new Tile(coords, IMapBlock.BlockType.Empty, new Vector2Int[0], _prefabs, transform));
                    if(Mathf.Abs(x)>1 && Mathf.Abs(x)<5 && Mathf.Abs(y)<5) _map[coords].PlaceTree(_prefabs, (_generationDist-_treeDistFromPath)*2-1, transform);
                }
            }

            _paths.Add(new Path(new Vector2Int(0, _generationDist - 1), 0, Vector2Int.up, _prefabs, false));
            _paths.Add(new Path(new Vector2Int(0, -_generationDist + 1), 180, Vector2Int.down, _prefabs, false));
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
            Vector2Int coords = new Vector2Int((int)(pos.x/Tile.TILE_WIDTH), (int)(pos.z/Tile.TILE_WIDTH));
            coords += pos.x < 0 ? Vector2Int.left : Vector2Int.zero; 
            coords += pos.z < 0 ? Vector2Int.down : Vector2Int.zero; 
            return coords;
        }

        void OnDestroy()
        {
            Tile.ClearTreePool();
            Instance = null;
        }
        
        private void OnValidate() 
        {
            if(_generationDist < 0) _generationDist = 0;
        }
    }
}
