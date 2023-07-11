using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Game
{
    public abstract class Weapon : MonoBehaviour
    {
        // This is a base class
        // Children classes are responsible for setting the targets in their own way

        [HideInInspector]
        public ItemWeaponSO ItemStats;

        public List<CombatFella> Targets { get; protected set; } = new List<CombatFella>();
        public bool Attacking { get; protected set; } = false;
        public bool Recovering { get; protected set; } = false;
        public bool Parriable { get; protected set; } = false;

        public float WindupTime => 0.6f * ItemStats.BaseSwingTime * _attackLenMod;
        public float SwingTime => 0.4f * ItemStats.BaseSwingTime * _attackLenMod;
        public float AttackRecoveryTime => ItemStats.BaseAttackRecoveryTime * _attackLenMod;
        public float ParryRecoveryTime => ItemStats.BaseParryRecoveryTime * _attackLenMod;
        private float _attackLenMod = 1;
        public float SkillDamageMod = 1;
        
        protected Animator _animator;
        protected CombatFella _holder;


        private void Awake()
        {
            _holder = transform.parent.GetComponent<CombatFella>();
            _animator = GetComponent<Animator>();
            StartCoroutine(UpdateParametersNextFrame());
            
            AdditionalAwake();
        }

        private IEnumerator UpdateParametersNextFrame()
        {
            yield return null;
            UpdateParameters();
        }

        public void UpdateParameters(float additionalAttackLenMod = 1f)
        {
            // Change animation speed based on the weapon qualities
            _attackLenMod = 1f + 0.04f*(2 - (int)ItemStats.ItemQuality);
            switch (ItemStats.WeaponEffect)
            {
                case(ItemWeaponSO.Effect.Light):
                {
                    _attackLenMod *= 0.8f;
                    break;
                }
                case(ItemWeaponSO.Effect.Heavy):
                {
                    _attackLenMod *= 1.2f;
                    break;
                }
            }
            // Additional modifiers from skills, items
            _attackLenMod *= additionalAttackLenMod;
            _attackLenMod /= _holder.ItemAttackSpeedMod;
            
            _animator.SetFloat("windupLen", 1f/(WindupTime));
            _animator.SetFloat("swingLen", 1f/(SwingTime));
            _animator.SetFloat("recoveryAttackLen", 1f/(AttackRecoveryTime));
            _animator.SetFloat("recoveryParriedLen", 1f/(ParryRecoveryTime));
        }

        public IEnumerator BoostNextAttackCoroutine(float attackLengthMod, float timeFrame)
        {
            UpdateParameters(attackLengthMod);
            yield return new WaitForSeconds(timeFrame);
            while(Attacking) 
            {
                yield return null;
            }
            UpdateParameters();
        }

        protected virtual void AdditionalAwake() { }

        public abstract bool StartAttack();
        public abstract void AttackLetGo();
        public abstract void GetInterupted(bool interuptAnimation);

        // <summary> returns true if actualy parried anything </summary>
        public virtual bool GetParried()
        {
            if (Parriable)
            {
                SoundManager.Instance.PlayRandomEffect(SoundManager.Instance.AudioEffects.Parry, transform.position);
                _animator.SetTrigger("parried");
                GetInterupted(false);
                Attacking = false;
                Parriable = false;
                StartCoroutine(Recover(ParryRecoveryTime));
                return true;
            }
            return false;
        }

        public virtual bool Parry(Vector3 lookDir) //TODO if this method stays remove lookdir
        {
            GetInterupted(true);
            _animator.SetTrigger("parry");
            bool parried = false;
            foreach (var fella in CombatFella.AllTheFellas)
            {
                if (fella == _holder) continue;
                Vector3 fellaDir = fella.transform.position - _holder.transform.position;
                // if (Vector3.Angle(lookDir, fellaDir) < 30f)
                // {
                //     parried = parried || fella.GetParried();
                // }
                if(fellaDir.magnitude < fella.Inventory.InHands.ThreatRange+0.5f)
                {
                    parried = parried || fella.GetParried();
                }
            }
            return parried;
        }

        protected IEnumerator Recover(float time)
        {
            Recovering = true;
            yield return new WaitForSeconds(time);
            Recovering = false;
        }

        protected void DealDamage()
        {
            //TODO implement effects
            if(ItemStats.ItemQuality == ItemWeaponSO.Quality.Null) 
            {
                Debug.LogError("Null quality");
                return;
            }
            if(ItemStats.WeaponEffect == ItemWeaponSO.Effect.Null) 
            {
                Debug.LogError("Null effect");
                return;
            }

            // Add damage mod from weapon quality
            float damage = ItemStats.BaseDamage * SkillDamageMod;
            damage += 0.2f * ((int)ItemStats.ItemQuality-2) * damage;

            float armourIgnore = ItemStats.ArmourPenetration;

            switch (ItemStats.WeaponEffect)
            {
                case(ItemWeaponSO.Effect.Light):
                {
                    damage *= 0.8f;
                    armourIgnore *= 0.7f;
                    break;
                }
                case(ItemWeaponSO.Effect.Heavy):
                {
                    damage *= 1.1f;
                    armourIgnore *= 1.3f;
                    break;
                }
            }

            List<CombatFella> targets = Targets.Distinct().ToList();
            for (int i = 0; i < targets.Count; i++)
            {
                if(targets[i] != null)
                {
                    armourIgnore = Mathf.Clamp01(armourIgnore);
                    if(ItemStats.WeaponEffect == ItemWeaponSO.Effect.Sharp)
                        targets[i].StartBleeding(3, _holder);
                    if(targets[i].TakeAHit(_holder, damage, 1-armourIgnore))
                    {
                        SoundManager.Instance.PlayRandomEffect(SoundManager.Instance.AudioEffects.Hit, targets[i].transform.position);
                    }
                }
                if (targets[i].Health <= 0)
                {
                    targets.Remove(targets[i]);
                    i--;
                }
            }
        }

        public static void DealDamage(ItemWeaponSO ItemStats, float SkillDamageMod, CombatFella holder, CombatFella target)
        {
            //TODO implement effects
            if(ItemStats.ItemQuality == ItemWeaponSO.Quality.Null) 
            {
                Debug.LogError("Null quality");
                return;
            }
            if(ItemStats.WeaponEffect == ItemWeaponSO.Effect.Null) 
            {
                Debug.LogError("Null effect");
                return;
            }

            // Add damage mod from weapon quality
            float damage = ItemStats.BaseDamage * SkillDamageMod;
            damage += 0.2f * ((int)ItemStats.ItemQuality-2) * damage;

            float armourIgnore = ItemStats.ArmourPenetration;

            switch (ItemStats.WeaponEffect)
            {
                case(ItemWeaponSO.Effect.Light):
                {
                    damage *= 0.8f;
                    armourIgnore *= 0.7f;
                    break;
                }
                case(ItemWeaponSO.Effect.Heavy):
                {
                    damage *= 1.1f;
                    armourIgnore *= 1.3f;
                    break;
                }
            }

            if(target != null)
            {
                armourIgnore = Mathf.Clamp01(armourIgnore);
                if(ItemStats.WeaponEffect == ItemWeaponSO.Effect.Sharp)
                    target.StartBleeding(3, holder);
                if(target.TakeAHit(holder, damage, 1-armourIgnore))
                {
                    SoundManager.Instance.PlayRandomEffect(SoundManager.Instance.AudioEffects.Hit, target.transform.position);
                }
            }
        }
    }
}
