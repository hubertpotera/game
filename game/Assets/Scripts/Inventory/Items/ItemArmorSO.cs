using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Items/ItemArmor")]
    public class ItemArmorSO : ItemSO
    {
        [Space]
        public int ArmorValue = 2;
    }
}
