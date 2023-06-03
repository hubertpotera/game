using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Items/Item")]
    public class ItemSO : ScriptableObject
    {
        public Texture2D Texture;
    }
}
