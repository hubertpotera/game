using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(menuName = "Scriptable Objects/WeaponStats")]
    public class WeaponStatsSO : ScriptableObject
    {
        public float ThreatRange = 1.3f;
        [Space]
        public int BaseDamage = 7;
        public DamageType TypeOfDamage = DamageType.slashing;
        [Space]
        public float BaseSwingTime = 0.5f;
        public float BaseAttackRecoveryTime = 0.5f;
        public float BaseParryRecoveryTime = 0.5f;

        public enum DamageType { blunt, piercing, slashing }
    }
}
