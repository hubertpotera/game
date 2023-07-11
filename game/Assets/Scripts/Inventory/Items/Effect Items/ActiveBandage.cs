using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ActiveBandage : EffectItem
    {
        public override bool Use()
        {
            _holder.ChangeHealth(Mathf.CeilToInt(0.5f * _holder.MaxHealth));
            _holder.StopBleeding();

            Destroy(gameObject);
            
            return true;
        }
    }
}
