using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class DaggerWeapon : BasicMeleeWeapon
    {
        protected override void DealDamage()
        {
            for (int i = 0; i < Targets.Count; i++)
            {
                if(Targets[i] != null)
                {
                    Targets[i].StartBleeding(2, _holder);
                }
            }
            base.DealDamage();
        }
    }
}
