using UnityEngine;
using System.Collections.Generic;

namespace Game
{
    public static class PrefabStorage 
    {
        public static List<GameObject> TileEmpty;
        public static List<GameObject> TilePathStraight;
        public static List<GameObject> TilePathTurn;
        public static List<GameObject> TilePathT;
        
        public static List<GameObject> PlaceableTree;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void UpdateVariables()
        {
            Debug.Log("Loading Prefabs");

            GameObject[] folder;

            // Tiles --------------------------------------------------
            TileEmpty = new List<GameObject>();
            folder = Resources.LoadAll<GameObject>("Prefabs/Tiles/Empty");
            foreach (var file in folder)
            {
                TileEmpty.Add(file);
            }

            TilePathStraight = new List<GameObject>();
            folder = Resources.LoadAll<GameObject>("Prefabs/Tiles/Path Straight");
            foreach (var file in folder)
            {
                TilePathStraight.Add(file);
            }

            TilePathTurn = new List<GameObject>();
            folder = Resources.LoadAll<GameObject>("Prefabs/Tiles/Path Turn");
            foreach (var file in folder)
            {
                TilePathTurn.Add(file);
            }

            TilePathT = new List<GameObject>();
            folder = Resources.LoadAll<GameObject>("Prefabs/Tiles/Path T");
            foreach (var file in folder)
            {
                TilePathT.Add(file);
            }

            // Placeables --------------------------------------------------
            PlaceableTree = new List<GameObject>();
            folder = Resources.LoadAll<GameObject>("Prefabs/Placeables");
            foreach (var file in folder)
            {
                PlaceableTree.Add(file);
            }
        }
    }
}
