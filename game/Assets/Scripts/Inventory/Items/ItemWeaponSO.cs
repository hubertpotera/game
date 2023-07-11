using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Items/ItemWeapon")]
    public class ItemWeaponSO : ItemSO
    {
        [Space]
        public GameObject WeaponPrefab;
        [Space]
        public float ThreatRange = 1.3f;
        [Range(0,1)]
        public float ArmourPenetration = 0f;
        public int BaseDamage = 7;
        [Space]
        public float BaseSwingTime = 0.5f;
        public float BaseAttackRecoveryTime = 0.5f;
        public float BaseParryRecoveryTime = 0.5f;


        public enum Effect
        {
            Null, None, Sharp, Light, Heavy
        }

        public Effect WeaponEffect = Effect.Null;
    }
}
