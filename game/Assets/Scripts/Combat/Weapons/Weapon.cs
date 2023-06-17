using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        public float DamageMod = 1;
        
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
            _attackLenMod = (1f - 0.1f*2*Mathf.Sqrt((int)ItemStats.WeaponQuality)) * additionalAttackLenMod;
            
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
        public bool GetParried()
        {
            if (Parriable)
            {
                SoundManager.Instance.PlayEffect(SoundManager.Choose(SoundManager.Instance.CombatAudio.Parry), transform.position);
                _animator.SetTrigger("parried");
                GetInterupted(false);
                Attacking = false;
                Parriable = false;
                StartCoroutine(Recover(ParryRecoveryTime));
                return true;
            }
            return false;
        }

        public virtual bool Parry(Vector3 lookDir)
        {
            GetInterupted(true);
            _animator.SetTrigger("parry");
            bool parried = false;
            foreach (var fella in CombatFella.AllTheFellas)
            {
                if (fella == _holder) continue;
                Vector3 fellaDir = fella.transform.position - _holder.transform.position;
                if (Vector3.Angle(lookDir, fellaDir) < 30f)
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

        protected virtual void DealDamage()
        {
            //TODO implement quality
            //TODO implement effects
            if(ItemStats.WeaponQuality == ItemWeaponSO.Quality.Null) 
            {
                Debug.LogError("Null quality");
                return;
            }
            if(ItemStats.WeaponEffect == ItemWeaponSO.Effect.Null) 
            {
                Debug.LogError("Null effect");
                return;
            }

            float damage = ItemStats.BaseDamage * DamageMod;
            damage += 0.2f * ((int)ItemStats.WeaponQuality-2) * damage;

            for (int i = 0; i < Targets.Count; i++)
            {
                if(Targets[i] != null)
                {
                    if(Targets[i].TakeAHit(_holder, damage))
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
