using UnityEngine;

namespace Game
{
    public class Placeable 
    {
        private Tile _parentTile;
        private GameObject _go;

        public Placeable(Tile parent, GameObject prefab, int rotationVariation = 0, float positionVariation01 = 0)
        {
            _parentTile = parent;

            _go = GameObject.Instantiate(prefab);
            Transform transform = _go.transform;

            transform.parent = parent.Go.transform;

            int rotation = Random.Range(-rotationVariation, rotationVariation);
            transform.Rotate(0f, rotation, 0f, Space.World);
            positionVariation01 = Mathf.Clamp01(positionVariation01);
            Vector3 positionOffset = new Vector3(
                0.5f*positionVariation01*Random.Range(-Tile.TileWidth,Tile.TileWidth), 0f, 
                0.5f*positionVariation01*Random.Range(-Tile.TileWidth,Tile.TileWidth));
            transform.position = parent.Go.transform.position + positionOffset;
            _go.name = prefab.name;
        }

        public void Delete()
        {
            Object.Destroy(_go);
        }
    }
}
