using UnityEngine;
using System.Collections.Generic;

namespace Game
{
    public class WorldTile
    {
        public const float TileWidth = 1.5f;

        public readonly Vector2Int Coords;
        public readonly TileType Type;
        public readonly Vector2Int[] ConnectionDirs;
        
        private GameObject _go;

        public enum TileType
        {
            Empty,
            Path
        }

        public WorldTile(Vector2Int coords, TileType tileType, Vector2Int[] connectionDirs,  System.Random prng)
        {
            Coords = coords;
            ConnectionDirs = connectionDirs;
            Type = tileType;
            
            _go = GameObject.Instantiate(DecideTile(tileType, prng, out float rotation));
            Debug.Log(_go.name);

            Transform transform = _go.transform;
            transform.Rotate(0f, rotation, 0f, Space.World);
            transform.position = TileWidth * (new Vector3(Coords.x, 0f, Coords.y) + new Vector3(0.5f, 0f, 0.5f));
            transform.localScale = TileWidth * Vector3.one;
            _go.name = coords.ToString();
        }

        private GameObject DecideTile(TileType tileType, System.Random prng, out float rotation)
        {
            if(tileType == TileType.Empty)
            {
                rotation = 90f*prng.Next(4);
                return RandomTileFromList(AllTiles.Empty, prng);
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
                    rotation += 180f*prng.Next(2);
                    
                    return RandomTileFromList(AllTiles.PathStraight, prng);
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
                    return RandomTileFromList(AllTiles.PathTurn, prng);
                }
            }

            Debug.LogError("Couldn't find viable tile");
            rotation = 0;
            return AllTiles.Empty[0];
        }

        public static GameObject RandomTileFromList(List<GameObject> list, System.Random prng)
        {
            return list[prng.Next(list.Count)];
        }

        public void Delete()
        {
            Object.Destroy(_go);
        }
    }
}
