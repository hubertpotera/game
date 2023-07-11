using UnityEngine;
using System.Collections.Generic;

namespace Game
{
    public class Tile : IMapBlock
    {
        public const float TILE_WIDTH = 2f;

        private Vector2Int _coords;
        private IMapBlock.BlockType _blockType;
        private Vector2Int[] _connectionDirs;
        
        private GameObject _go;
        private GameObject _placeable;

        public Vector2Int[] GetConnectionDirs() => _connectionDirs;
        public IMapBlock.BlockType GetBlockType() => _blockType;

        private static List<GameObject> _treePool;
        private static int _treeIndexer = 0;


        public Tile(Vector2Int coords, IMapBlock.BlockType tileType, Vector2Int[] connectionDirs, WorldPrefabsSO worldPrefabs, Transform parent)
        {
            _coords = coords;
            _connectionDirs = connectionDirs;
            _blockType = tileType;
            
            _go = GameObject.Instantiate(DecideTile(tileType, out float rotation, worldPrefabs));
            _go.transform.parent = parent;

            Transform transform = _go.transform;
            transform.Rotate(0f, rotation, 0f, Space.World);
            transform.position = TILE_WIDTH * (new Vector3(_coords.x, 0f, _coords.y) + new Vector3(0.5f, 0f, 0.5f));
            transform.localScale = TILE_WIDTH * Vector3.one;
            _go.name = coords.ToString();
        }



        private GameObject DecideTile(IMapBlock.BlockType tileType, out float rotation, WorldPrefabsSO worldPrefabs)
        {
            if(tileType == IMapBlock.BlockType.Empty)
            {
                rotation = 90f*Random.Range(0,4);
                return WorldPrefabsSO.RandomGOFromList(worldPrefabs.TileEmpty);
            }
            if(tileType == IMapBlock.BlockType.Path)
            {
                if(_connectionDirs.Length == 2 && _connectionDirs[0] == -_connectionDirs[1])
                {
                    // Straight
                    if(_connectionDirs[0] == Vector2Int.up || _connectionDirs[1] == Vector2Int.up)
                        rotation = 0f;
                    else
                        rotation = 90f;
                    rotation += 180f*Random.Range(0,2);

                    return WorldPrefabsSO.RandomGOFromList(worldPrefabs.TilePathStraight);
                }
                if(_connectionDirs.Length == 2)
                {
                    // Turn

                    //   
                    //   #--
                    //   |    -root
                    
                    Vector2Int root = _connectionDirs[0];
                    if(Misc.RotateV2Int(root, 1) == _connectionDirs[1])
                    {
                        root = _connectionDirs[1];
                    }

                    rotation = 0f;
                    if(root.y == 0) rotation += 90f;
                    if(root.y + root.x == 1) rotation += 180f;
                    return WorldPrefabsSO.RandomGOFromList(worldPrefabs.TilePathTurn);
                }
                if (_connectionDirs.Length == 3)
                {
                    bool leftFound= false;
                    bool rightFound= false;
                    bool downFound = false;
                    bool upFound= false;
                    foreach(var dir in _connectionDirs)
                    {
                        if(dir.y == 1) upFound= true;
                        else if(dir.y == -1) downFound= true;
                        else if(dir.x == 1) rightFound= true;
                        else if(dir.x == -1) leftFound= true;
                    }
                    rotation = 0f;
                    if (!upFound) rotation = 0f;
                    if (!rightFound) rotation = 90f;
                    if (!downFound) rotation = 180f;
                    if (!leftFound) rotation = 270f;
                    return WorldPrefabsSO.RandomGOFromList(worldPrefabs.TilePathT);
                }
            }

            Debug.LogError("Couldn't find viable tile");
            rotation = 0;
            return worldPrefabs.TileEmpty[0];
        }
        
        public void PlaceTree(WorldPrefabsSO worldPrefabs, int treeRange)
        {
            // Instanciate tree pool
            if(_treePool == null)
            {
                _treePool = new List<GameObject>();
                for (int i = 0; i < treeRange*treeRange; i++)
                {
                    _treePool.Add(GameObject.Instantiate(WorldPrefabsSO.RandomGOFromList(worldPrefabs.PlaceableTree)));
                    _treePool[i].SetActive(false);  
                    _treePool[i].transform.localScale = _go.transform.localScale;
                }
            }

            while(_treePool[_treeIndexer].activeInHierarchy == true) 
            {
                _treeIndexer = (_treeIndexer+1)%(treeRange*treeRange);
            }
            _placeable = _treePool[_treeIndexer];
            _placeable.SetActive(true);
            _placeable.transform.position = _go.transform.position;
        }

        public void PlaceIndicator(GameObject prefab, Vector3 offset)
        {
            GameObject indicator = GameObject.Instantiate(prefab);
            
            indicator.transform.parent = _go.transform;
            indicator.transform.position = _go.transform.position + offset;
        }
        
        public void RemovePlaceable()
        {
            if(_placeable != null)
            {
                _placeable.SetActive(false);
                _placeable = null;
            }
        }

        public void Delete(ref Dictionary<Vector2Int, IMapBlock> map)
        {
            RemovePlaceable();
            map.Remove(_coords);
            Object.Destroy(_go);
        }

        public static void ClearTreePool()
        {
            _treePool = null;
            _treeIndexer = 0;
        }
    }
}
