using UnityEngine;
using System.Collections.Generic;

namespace Game
{
    public static class AllTiles 
    {
        public static List<TileSO> Empty;
        public static List<TileSO> PathStraight;
        public static List<TileSO> PathTurn;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void UpdateVariables()
        {
            TileSO[] tiles;

            Empty = new List<TileSO>();
            tiles = Resources.LoadAll<TileSO>("Tile SOs/Empty");
            foreach (var tile in tiles)
            {
                tile.UpdateVariabes();
                Empty.Add(tile);
            }

            PathStraight = new List<TileSO>();
            tiles = Resources.LoadAll<TileSO>("Tile SOs/Path Straight");
            foreach (var tile in tiles)
            {
                tile.UpdateVariabes();
                PathStraight.Add(tile);
            }

            PathTurn = new List<TileSO>();
            tiles = Resources.LoadAll<TileSO>("Tile SOs/Path Turn");
            foreach (var tile in tiles)
            {
                tile.UpdateVariabes();
                PathTurn.Add(tile);
            }
        }
    }
}
