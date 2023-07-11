using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class PassiveHealthItem : EffectItem
    {
        public override void Initialize(CombatFella holder, ItemEffectSO item)
        {
            base.Initialize(holder, item);
            _holder.ItemHealthMod += ((int)_item.ItemQuality*0.2f);
            _holder.ChangeHealth(0);
        }
    }
}
