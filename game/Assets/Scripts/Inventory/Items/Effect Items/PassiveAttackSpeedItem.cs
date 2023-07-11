using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class PassiveAttackSpeedItem : EffectItem
    {
        public override void Initialize(CombatFella holder, ItemEffectSO item)
        {
            base.Initialize(holder, item);
            _holder.ItemAttackSpeedMod += ((int)_item.ItemQuality*0.2f);
        }
    }
}
