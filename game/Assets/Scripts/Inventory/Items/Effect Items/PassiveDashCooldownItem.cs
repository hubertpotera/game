using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class PassiveDashCooldownItem : EffectItem
    {
        public override void Initialize(CombatFella holder, ItemEffectSO item)
        {
            base.Initialize(holder, item);
            _holder.ItemDashCooldownMod -= ((int)_item.ItemQuality*0.1f);
        }
    }
}
