using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public abstract class EffectItem : MonoBehaviour 
    {
        protected ItemEffectSO _item;
        protected CombatFella _holder;

        public virtual void Initialize(CombatFella holder, ItemEffectSO item)
        {
            _holder = holder;
            _item = item;
            if(item.ItemQuality == ItemSO.Quality.Null) Debug.LogError("Null quality");
        }

        public virtual bool Use() { return false; }
    }
}
