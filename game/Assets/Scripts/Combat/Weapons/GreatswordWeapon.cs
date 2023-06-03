using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class GreatswordWeapon : Weapon
    {
        [SerializeField]
        private MeshRenderer _damageArea1;
        [SerializeField]
        private MeshRenderer _damageArea2;
        [SerializeField]
        private MeshRenderer _damageArea3;

        private Coroutine _attackingCoroutine = null;

        [SerializeField]
        private WeaponCollider _collider1;
        [SerializeField]
        private WeaponCollider _collider2;
        [SerializeField]
        private WeaponCollider _collider3;

        private float _lastAttackEnd = 0f;
        private int _attacksInRow = 0;

        protected override void AdditionalAwake()
        {
            _damageArea1.material.color = new Color(1f, 1f, 1f, 0f);
            _damageArea2.material.color = new Color(1f, 1f, 1f, 0f);
            _damageArea3.material.color = new Color(1f, 1f, 1f, 0f);
        }

        public override bool StartAttack()
        {
            if (!Attacking && !Recovering)
            {
                Attacking = true;
                if(Time.time - _lastAttackEnd < 0.2f)
                {
                    MeshRenderer damageArea = _damageArea1;
                    WeaponCollider collider = _collider1;
                    if(_attacksInRow == 2)
                    {
                        damageArea = _damageArea2;
                        collider = _collider2;
                    }
                    else if(_attacksInRow >= 3)
                    {
                        damageArea = _damageArea3;
                        collider = _collider3;
                    }
                    
                    _attackingCoroutine = StartCoroutine(DoTheAttack(damageArea, collider.Targets));
                    _attacksInRow += 1;
                }
                else
                {
                    _attackingCoroutine = StartCoroutine(DoTheAttack(_damageArea1, _collider1.Targets));
                    _attacksInRow = 1;
                }
                return true;
            }
            return false;
        }

        public override void AttackLetGo()
        {

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
                _damageArea3.material.color = new Color(1f, 1f, 1f, 0f);
                Attacking = false;
                Parriable = false;
                StopCoroutine(_attackingCoroutine);
                _attackingCoroutine = null;
            }
        }

        private IEnumerator DoTheAttack(MeshRenderer damageArea, List<CombatFella> targets)
        {
            // Charge
            _animator.SetTrigger("attack");
            _audioSource.PlayOneShot(_audio.Choose(_audio.Windup));
            damageArea.material.color = new Color(1f, 1f, 1f, 0.3f);
            yield return new WaitForSeconds(WindupTime);

            // Swing
            damageArea.material.color = new Color(1f, 1f, 1f, 1.0f);
            _audioSource.PlayOneShot(_audio.Choose(_audio.Swing));
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
