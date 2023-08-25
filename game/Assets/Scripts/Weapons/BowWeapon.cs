using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class BowWeapon : Weapon
    {
        [SerializeField]
        private GameObject _arrow;

        private float _drawStartTime = -1f;

        public override bool Parry(Vector3 lookDir)
        {
            GetInterupted(true);
            return false;
        }

        public override bool StartAttack()
        {
            if(!Recovering && transform.parent.GetComponent<Inventory>().Arrows > 0)
            {
                SoundManager.Instance.PlayRandomEffect(SoundManager.Instance.AudioEffects.BowDraw);
                _animator.SetBool("drawing", true);
                _drawStartTime = Time.time;
                Attacking= true;
                return true;
            }
            return false;
        }

        public override void AttackLetGo()
        {
            SoundManager.Instance.StopEffects();
            if(_drawStartTime > 0f && Time.time-_drawStartTime >= SwingTime)
            {
                SoundManager.Instance.PlayRandomEffect(SoundManager.Instance.AudioEffects.BowRelease);
                _animator.SetTrigger("shoot");
                StartCoroutine(Recover(AttackRecoveryTime));
                
                BowArrow arrow = GameObject.Instantiate(_arrow, transform.position, Quaternion.identity).GetComponent<BowArrow>();
                arrow.Direction = transform.forward;
                arrow.WeaponStats = ItemStats;
                arrow.Holder = _holder;
                _holder.GetComponent<Inventory>().Arrows --;
            }
            _animator.SetBool("drawing", false);
            Attacking= false;
            _drawStartTime = 0f;
        }

        public override void GetInterupted(bool interuptAnimation)
        {
            SoundManager.Instance.StopEffects();
            _drawStartTime = 0f;
            Attacking= false;
        }
    }
}
