using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Singletons/WorldPrefabs")]
    public class WorldPrefabsSO : ScriptableObject
    {
        public List<GameObject> TileEmpty;
        public List<GameObject> TilePathStraight;
        public List<GameObject> TilePathTurn;
        [Space]
        public List<GameObject> PlaceableTree;
        [Space]
        public List<GameObject> TilePathT;
        public List<GameObject> Areas;


        public static GameObject RandomGOFromList(List<GameObject> list)
        {
            return list[Random.Range(0, list.Count)];
        }
    }
}
