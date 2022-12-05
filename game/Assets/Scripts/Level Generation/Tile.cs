using UnityEngine;

namespace Game
{
    public class Tile
    {
        public const float TileWidth = 1.5f;

        public readonly Vector2Int Coords;
        
        private GameObject _go;

        public Tile(Vector2Int coords)
        {
            Coords = coords;
            _go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _go.transform.position = TileWidth * new Vector3(Coords.x, 0f, Coords.y) + TileWidth * new Vector3(0.5f, 0f, 0.5f);
            _go.transform.localScale = 0.95f * TileWidth * Vector3.one;
            _go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }

        public void Delete()
        {
            Object.Destroy(_go);
        }
    }
}
