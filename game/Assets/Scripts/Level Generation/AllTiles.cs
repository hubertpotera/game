using UnityEngine;

namespace Game
{
    public static class AllTiles 
    {
        public static TileSO[] Tiles { get; private set; }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void UpdateVariables()
        {
            Tiles = Resources.LoadAll<TileSO>("Tile SOs");
            foreach (var tile in Tiles)
            {
                tile.UpdateVariabes();
            }
        }
    }
}
