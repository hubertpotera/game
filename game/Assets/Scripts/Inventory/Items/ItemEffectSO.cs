using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Items/ItemEffect")]
    public class ItemEffectSO : ItemSO
    {
        public GameObject PrefabWithEffect; 
    }
}
