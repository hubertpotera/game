using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ActiveSpiceBuff : EffectItem
    {
        [SerializeField]
        private ParticleSystem _particles;

        public override bool Use()
        {
            _holder.ItemAttackSpeedMod *= 1f + ((int)_item.ItemQuality*0.1f);
            _holder.ItemDashCooldownMod *= 1f - ((int)_item.ItemQuality*0.1f);
            _holder.ItemDashDistMod *= 1f + ((int)_item.ItemQuality*0.1f);
            _holder.ItemSpeedMod *= 1f + ((int)_item.ItemQuality*0.1f);
            _holder.Inventory.Weapon.UpdateParameters();
            _particles.Play();

            StartCoroutine(DestroyAfterDelay());

            return true;
        }

        private IEnumerator DestroyAfterDelay()
        {
            yield return new WaitForSeconds(2f + 2f*(int)_item.ItemQuality);
            _holder.Inventory.ApplyItemEffects();
            Debug.Log("edn");
            Destroy(gameObject);
        }
    }
}
