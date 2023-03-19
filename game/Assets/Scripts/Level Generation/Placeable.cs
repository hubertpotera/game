using UnityEngine;

namespace Game
{
    public class Placeable 
    {
        private GameObject _go;


        public Placeable(Transform parent, GameObject prefab, int rotationVariation = 0, float positionVariation01 = 0f)
        {
            _go = GameObject.Instantiate(prefab);
            Transform transform = _go.transform;

            transform.parent = parent;

            int rotation = Random.Range(-rotationVariation, rotationVariation);
            transform.Rotate(0f, rotation, 0f, Space.World);
            positionVariation01 = Mathf.Clamp01(positionVariation01);
            Vector3 positionOffset = new Vector3(
                0.5f*positionVariation01*Random.Range(-Tile.TILE_WIDTH,Tile.TILE_WIDTH), transform.position.y, 
                0.5f*positionVariation01*Random.Range(-Tile.TILE_WIDTH,Tile.TILE_WIDTH));
            transform.position = parent.position + positionOffset;
            transform.localScale = Vector3.one * Tile.TILE_WIDTH*0.5f;
            _go.name = prefab.name;
        }



        public void Delete()
        {
            Object.Destroy(_go);
        }
    }
}
