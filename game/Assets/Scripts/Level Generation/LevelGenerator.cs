using UnityEngine;
using System.Collections.Generic;

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

        private Vector2Int _prevPlayerCoords = Vector2Int.zero;

        private Dictionary<Vector2Int, WorldTile> _map;

        #endregion





        // ------------ INIT -------------
        #region init

        private void Awake() 
        {
            Vector2Int playerCoords = PosToCoords(_player.position);

            _map = new Dictionary<Vector2Int, WorldTile>();
            for (int x = -_generationDist+1; x < _generationDist; x++)
            {
                for (int y = -_generationDist+1; y < _generationDist; y++)
                {
                    Vector2Int coords = playerCoords + new Vector2Int(x, y);
                    _map.Add(coords, new WorldTile(coords));
                }
            }
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
            if((prevCoords - newCoords).sqrMagnitude > 1)
            {
                // Gotta update the whole map
                Debug.Log("Big generation");
                // for each chunk check if it was in the previous one
                // if so keep it
                // if not create a new one
                // replace old map with new one
            }
            else
            {
                Vector2Int change = newCoords-prevCoords;
                Vector2Int[] toDelete = new Vector2Int[0];
                Vector2Int[] toCreate = new Vector2Int[0];
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

                for(int i = 0; i < toDelete.Length; i++)
                {
                    _map[toDelete[i]].Delete();
                    _map.Remove(toDelete[i]);
                    _map[toCreate[i]] = new WorldTile(toCreate[i]);
                }
            }
        }

        /// <summary>Range 1 will return 1 block</summary>
        private Vector2Int[] GetHorizontalWall(Vector2Int middle, int range)
        {
            if(range <= 0) return new Vector2Int[0];

            Vector2Int[] wall = new Vector2Int[2*(range-1) + 1];
            int i = 0;
            for (int x = -range+1; x < range; x++)
            {
                wall[i] = middle + new Vector2Int(x, 0);
                i++;
            }
            return wall;
        }

        /// <summary>Range 1 will return 1 block</summary>
        private Vector2Int[] GetVerticalWall(Vector2Int middle, int range)
        {
            if(range <= 0) return new Vector2Int[0];

            Vector2Int[] wall = new Vector2Int[2*(range-1) + 1];
            int i = 0;
            for (int y = -range+1; y < range; y++)
            {
                wall[i] = middle + new Vector2Int(0, y);
                i++;
            }
            return wall;
        }
        
        private void OnValidate() 
        {
            if(_generationDist < 0) _generationDist = 0;
        }
        
        #endregion
    }
}
