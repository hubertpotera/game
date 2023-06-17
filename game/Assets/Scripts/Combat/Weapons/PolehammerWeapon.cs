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
                        SoundManager.Instance.PlayEffect(SoundManager.Choose(SoundManager.Instance.CombatAudio.Hit), transform.position);
                    }
                }
                if (Targets[i].Health <= 0)
                {
                    Targets.Remove(Targets[i]);
                    i--;
                }
            }
        }
    }
}
