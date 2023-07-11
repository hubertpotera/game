using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class LevelGeneratorTutorial : MonoBehaviour
    {
        [SerializeField]
        private WorldPrefabsSO _prefabs;

        void Awake()
        {
            Generate();
        }

        private void Generate()
        {
            for (int x = -6 + 1; x < 6; x++)
            {
                for (int y = -6 + 1; y < 22; y++)
                {
                    Vector2Int coords = new Vector2Int(x, y);

                    Tile created;

                    if (x == 0 && y > -2) 
                    {
                        created = new Tile(coords, IMapBlock.BlockType.Path, new Vector2Int[] { Vector2Int.up, Vector2Int.down }, _prefabs, transform);
                    }
                    else 
                    {
                        created = new Tile(coords, IMapBlock.BlockType.Empty, new Vector2Int[0], _prefabs, transform);
                    }
                }
            }
        }
    }
}
