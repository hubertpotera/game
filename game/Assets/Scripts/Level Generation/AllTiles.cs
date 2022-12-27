using UnityEngine;
using System.Collections.Generic;

namespace Game
{
    public static class AllTiles 
    {
        public static List<GameObject> Empty;
        public static List<GameObject> PathStraight;
        public static List<GameObject> PathTurn;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void UpdateVariables()
        {
            GameObject[] tiles;

            Empty = new List<GameObject>();
            tiles = Resources.LoadAll<GameObject>("Tiles/Empty");
            foreach (var tile in tiles)
            {
                Empty.Add(tile);
            }

            PathStraight = new List<GameObject>();
            tiles = Resources.LoadAll<GameObject>("Tiles/Path Straight");
            foreach (var tile in tiles)
            {
                PathStraight.Add(tile);
            }

            PathTurn = new List<GameObject>();
            tiles = Resources.LoadAll<GameObject>("Tiles/Path Turn");
            foreach (var tile in tiles)
            {
                PathTurn.Add(tile);
            }
        }
    }
}
