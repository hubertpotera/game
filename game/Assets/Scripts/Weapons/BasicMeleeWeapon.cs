using System.Collections;
using UnityEngine;

namespace Game
{
    public class BasicMeleeWeapon : Weapon
    {
        [SerializeField]
        private MeshRenderer _damageArea;

        private Coroutine _attackingCoroutine = null;


        private void OnTriggerEnter(Collider other)
        {
            CombatFella targetFella = other.gameObject.GetComponent<CombatFella>();
            if (targetFella != null)
            {
                Targets.Add(targetFella);
            }
        }
        private void OnTriggerExit(Collider other)
        {
            CombatFella targetFella = other.gameObject.GetComponent<CombatFella>();
            if (targetFella != null)
            {
                Targets.Remove(targetFella);
            }
        }

        protected override void AdditionalAwake()
        {
            _damageArea.material.color = new Color(1f, 1f, 1f, 0f);
        }

        public override bool StartAttack()
        {
            if (!Attacking && !Recovering)
            {
                Attacking = true;
                _attackingCoroutine = StartCoroutine(DoTheAttack());
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
                _damageArea.material.color = new Color(1f, 1f, 1f, 0f);
                Attacking = false;
                Parriable = false;
                StopCoroutine(_attackingCoroutine);
                _attackingCoroutine = null;
            }
        }

        private IEnumerator DoTheAttack()
        {
            // Charge
            _animator.SetTrigger("attack");
            SoundManager.Instance.PlayRandomEffect(SoundManager.Instance.AudioEffects.Windup, transform.position);
            _damageArea.material.color = new Color(1f, 1f, 1f, 0.3f);
            yield return new WaitForSeconds(WindupTime);

            // Swing
            _damageArea.material.color = new Color(.8f, .4f, .4f, 1.0f);
            SoundManager.Instance.PlayRandomEffect(SoundManager.Instance.AudioEffects.Swing, transform.position);
            Parriable = true;
            yield return new WaitForSeconds(SwingTime);

            // Damage
            _damageArea.material.color = new Color(1f, 1f, 1f, 0f);
            Parriable = false;

            DealDamage();

            Attacking = false;
            StartCoroutine(Recover(AttackRecoveryTime));
        }
    }
}
