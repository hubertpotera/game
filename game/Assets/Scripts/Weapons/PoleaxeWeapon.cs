using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class PoleaxeWeapon : Weapon
    {
        [SerializeField]
        private MeshRenderer _damageArea1;
        [SerializeField]
        private MeshRenderer _damageArea2;

        private Coroutine _attackingCoroutine = null;

        [SerializeField]
        private WeaponCollider _collider1;
        [SerializeField]
        private WeaponCollider _collider2;

        private float _lastAttackEnd = 0f;

        protected override void AdditionalAwake()
        {
            _damageArea1.material.color = new Color(1f, 1f, 1f, 0f);
            _damageArea2.material.color = new Color(1f, 1f, 1f, 0f);
        }

        public override bool StartAttack()
        {
            if (!Attacking && !Recovering)
            {
                Attacking = true;
                if(Time.time - _lastAttackEnd < 0.5f)
                {
                    _attackingCoroutine = StartCoroutine(DoTheAttack(_damageArea2, _collider2.Targets, "attack2"));
                }
                else
                {
                    _attackingCoroutine = StartCoroutine(DoTheAttack(_damageArea1, _collider1.Targets, "attack1"));
                }
                return true;
            }
            return false;
        }

        public override void AttackLetGo()
        {

        }

        public override bool GetParried()
        {
            if(base.GetParried())
            {
                _lastAttackEnd = Time.time + ParryRecoveryTime;
                return true;
            }
            return false;
        }

        public override void GetInterupted(bool interuptAnimation)
        {
            if (Attacking)
            {
                if(interuptAnimation)
                {
                    _animator.SetTrigger("interupted");
                }
                _damageArea1.material.color = new Color(1f, 1f, 1f, 0f);
                _damageArea2.material.color = new Color(1f, 1f, 1f, 0f);
                Attacking = false;
                Parriable = false;
                StopCoroutine(_attackingCoroutine);
                _attackingCoroutine = null;
            }
        }

        private IEnumerator DoTheAttack(MeshRenderer damageArea, List<CombatFella> targets, string animationTrigger)
        {
            // Charge
            _animator.SetTrigger(animationTrigger);
            SoundManager.Instance.PlayRandomEffect(SoundManager.Instance.AudioEffects.Windup, transform.position);
            damageArea.material.color = new Color(1f, 1f, 1f, 0.3f);
            yield return new WaitForSeconds(WindupTime);

            // Swing
            damageArea.material.color = new Color(.8f, .4f, .4f, 1.0f);
            SoundManager.Instance.PlayRandomEffect(SoundManager.Instance.AudioEffects.Swing, transform.position);
            Parriable = true;
            yield return new WaitForSeconds(SwingTime);

            // Damage
            damageArea.material.color = new Color(1f, 1f, 1f, 0f);
            Parriable = false;

            
            Targets = new List<CombatFella>(targets);
            DealDamage();
            Targets.Clear();

            Attacking = false;
            StartCoroutine(Recover(AttackRecoveryTime));

            _lastAttackEnd = Time.time + AttackRecoveryTime;
        }
    }
}
