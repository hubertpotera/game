using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class PassiveDashDistItem : EffectItem
    {
        public override void Initialize(CombatFella holder, ItemEffectSO item)
        {
            base.Initialize(holder, item);
            _holder.ItemDashDistMod += ((int)_item.ItemQuality*0.1f);
        }
    }
}
