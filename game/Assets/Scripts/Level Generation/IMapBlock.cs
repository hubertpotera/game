using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public interface IMapBlock
    {

        public void Delete(ref Dictionary<Vector2Int,IMapBlock> map);
        public BlockType GetBlockType();
        public Vector2Int[] GetConnectionDirs();
        public void PlaceTree(WorldPrefabsSO worldPrefabs, int treeRange, Transform parent);
        public void RemovePlaceable();

        public enum BlockType
        {
            Area,
            Empty,Path
        }
    }
}
