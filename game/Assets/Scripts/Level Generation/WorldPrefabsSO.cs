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
        public List<CombatAreaCondition> CombatAreas;
        public GameObject ShopArea;
        public GameObject ShopIndicator;
        public GameObject BossArea;
        public GameObject BossIndicator;


        public static GameObject RandomGOFromList(List<GameObject> list)
        {
            return list[Random.Range(0, list.Count)];
        }

        public GameObject ChooseCombatArea(int fellasKilled)
        {
            CombatAreaCondition area;
            do
            {
                area = CombatAreas[Random.Range(0, CombatAreas.Count)];
            }while(fellasKilled < area.KilledThreshhold);
            return area.Prefab;
        }

        [System.Serializable]
        public struct CombatAreaCondition
        {
            public GameObject Prefab;
            public int KilledThreshhold;
        }
    }
}
