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
                _audioSource.PlayOneShot(_audio.Choose(_audio.BowDraw));
                _animator.SetBool("drawing", true);
                _drawStartTime = Time.time;
                Attacking= true;
                return true;
            }
            return false;
        }

        public override void AttackLetGo()
        {
            if(_drawStartTime > 0f && Time.time-_drawStartTime >= SwingTime)
            {
                _audioSource.Stop();
                _audioSource.PlayOneShot(_audio.Choose(_audio.BowRelease));
                _animator.SetTrigger("shoot");
                StartCoroutine(Recover(AttackRecoveryTime));
                
                BowArrow arrow = GameObject.Instantiate(_arrow, transform.position, Quaternion.identity).GetComponent<BowArrow>();
                arrow.Direction = transform.forward;
                arrow.WeaponStats = ItemStats;
                arrow.Audio = _audio;
                arrow.Holder = _holder;
                _holder.GetComponent<Inventory>().Arrows --;
            }
            _animator.SetBool("drawing", false);
            Attacking= false;
            _drawStartTime = 0f;
        }

        public override void GetInterupted(bool interuptAnimation)
        {
            _drawStartTime = 0f;
            Attacking= false;
        }
    }
}
