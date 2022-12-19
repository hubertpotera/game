using UnityEngine;

namespace Game
{
    public class WorldTile
    {
        public const float TileWidth = 1.5f;

        public readonly Vector2Int Coords;
        public readonly TileSO Tile;
        
        private GameObject _go;

        public enum TileType
        {
            Empty,
            Path
        }

        public WorldTile(Vector2Int coords, TileType tileType, Vector2Int[] connectionDirs)
        {
            Coords = coords;
            
            Tile = AllTiles.Empty[0]; //TODO empty, path, path connections

            Vector3 pos = TileWidth * (new Vector3(Coords.x, 0f, Coords.y) + new Vector3(0.5f, 0f, 0.5f));
            _go =  GameObject.Instantiate(Tile.prefab.gameObject, pos, Quaternion.Euler(90f, 0f, 0f));
            _go.transform.localScale = TileWidth * Vector3.one;
            _go.name = coords.ToString();
        }

        public void Delete()
        {
            Object.Destroy(_go);
        }
    }
}
