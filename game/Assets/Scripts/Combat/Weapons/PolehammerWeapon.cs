using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class PolehammerWeapon : BasicMeleeWeapon
    {
        protected override void DealDamage()
        {
            for (int i = 0; i < Targets.Count; i++)
            {
                if(Targets[i] != null)
                {
                    if(Targets[i].TakeAHit(_holder, Mathf.CeilToInt(ItemStats.BaseDamage * DamageMod), 0.2f))
                    {
                        _holder.GainExpertice(5);
                        _audioSource.PlayOneShot(_audio.Choose(_audio.Hit));
                    }
                }
                if (Targets[i].Health <= 0)
                {
                    _holder.GainExpertice(10);
                    Targets.Remove(Targets[i]);
                    i--;
                }
            }
        }
    }
}
