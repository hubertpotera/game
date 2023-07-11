using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Items/Item")]
    public class ItemSO : ScriptableObject
    {
        public Texture2D Texture;

        public enum Quality
        {
            Null, Old, Decent, Normal, Quality, Pristine    // Don't add anymore or the damage system will break (and some other stuff too)
        }
        
        public Quality ItemQuality = Quality.Null;
        public string Description = "uhhh";
    }
}
