using UnityEngine;
using System.Collections.Generic;

namespace Game
{
    public class Tile
    {
        public const float TileWidth = 1.5f;

        public readonly Vector2Int Coords;
        public readonly TileType Type;
        public readonly Vector2Int[] ConnectionDirs;
        
        public readonly GameObject Go;
        private Placeable _placeable;

        public enum TileType
        {
            Empty,
            Path
        }

        public Tile(Vector2Int coords, TileType tileType, Vector2Int[] connectionDirs)
        {
            Coords = coords;
            ConnectionDirs = connectionDirs;
            Type = tileType;
            
            Go = GameObject.Instantiate(DecideTile(tileType, out float rotation));

            Transform transform = Go.transform;
            transform.Rotate(0f, rotation, 0f, Space.World);
            transform.position = TileWidth * (new Vector3(Coords.x, 0f, Coords.y) + new Vector3(0.5f, 0f, 0.5f));
            transform.localScale = TileWidth * Vector3.one;
            Go.name = coords.ToString();
        }

        private GameObject DecideTile(TileType tileType, out float rotation)
        {
            if(tileType == TileType.Empty)
            {
                rotation = 90f*Random.Range(0,4);
                return RandomTileFromList(PrefabStorage.TileEmpty);
            }
            if(tileType == TileType.Path)
            {
                if(ConnectionDirs.Length == 2 && ConnectionDirs[0] == -ConnectionDirs[1])
                {
                    // Straight
                    if(ConnectionDirs[0] == Vector2Int.up || ConnectionDirs[1] == Vector2Int.up)
                        rotation = 0f;
                    else
                        rotation = 90f;
                    rotation += 180f*Random.Range(0,2);
                    
                    return RandomTileFromList(PrefabStorage.TilePathStraight);
                }
                if(ConnectionDirs.Length == 2)
                {
                    // Turn

                    //   
                    //   #--
                    //   |    -root
                    
                    Vector2Int root = ConnectionDirs[0];
                    if(Misc.RotateV2Int(ConnectionDirs[0], 1) == ConnectionDirs[1])
                    {
                        root = ConnectionDirs[1];
                    }

                    rotation = 0f;
                    if(root.y == 0) rotation += 90f;
                    if(root.y + root.x == 1) rotation += 180f;
                    return RandomTileFromList(PrefabStorage.TilePathTurn);
                }
            }

            Debug.LogError("Couldn't find viable tile");
            rotation = 0;
            return PrefabStorage.TileEmpty[0];
        }
        
        public void PlaceTree()
        {
            _placeable = new Placeable(this, PrefabStorage.PlaceableTree[0], 45, .5f);
        }
        
        public void RemovePlaceable()
        {
            if(_placeable != null)
                _placeable.Delete();
        }

        private static GameObject RandomTileFromList(List<GameObject> list)
        {
            return list[Random.Range(0,list.Count)];
        }

        public void Delete()
        {
            RemovePlaceable();
            Object.Destroy(Go);
        }
    }
}
