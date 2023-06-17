using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Singletons/Inventory Progression")]
    public class InventoryProgressionSO : ScriptableObject
    {
        [SerializeField]
        public List<ArmourUpgrade> Head1Upgrades = new List<ArmourUpgrade>();
        [SerializeField]
        public List<ArmourUpgrade> Head2Upgrades = new List<ArmourUpgrade>();
        [SerializeField]
        public List<ArmourUpgrade> Body1Upgrades = new List<ArmourUpgrade>();
        [SerializeField]
        public List<ArmourUpgrade> Body2Upgrades = new List<ArmourUpgrade>();
        
        [System.Serializable]
        public struct ArmourUpgrade
        {
            public ItemArmorSO Item;
            public int Cost;
        }
    }
}
