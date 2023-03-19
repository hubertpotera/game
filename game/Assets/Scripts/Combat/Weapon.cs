using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Game
{
    public abstract class Weapon : MonoBehaviour
    {
        // This is a base class
        // Children classes are responsible for setting the targets in their own way

        public WeaponStatsSO WeaponStats;

        public List<CombatFella> Targets { get; protected set; } = new List<CombatFella>();
        private CombatFella _holder;
        public bool Attacking { get; private set; } = false;
        public bool Recovering { get; private set; } = false;
        public bool Parriable { get; private set; } = false;

        public float SwingTime => WeaponStats.BaseSwingTime;
        public float AttackRecoveryTime => WeaponStats.BaseAttackRecoveryTime;
        public float ParryRecoveryTime => WeaponStats.BaseParryRecoveryTime;

        private Coroutine _attackingCoroutine = null;

        private void Awake()
        {
            _holder = transform.parent.GetComponent<CombatFella>();
            transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;//TEMP
        }

        public void StartAttack()
        {
            if(!Attacking && !Recovering)
            {
                Attacking = true;
                _attackingCoroutine = StartCoroutine(DoTheAttack());
                transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;//TEMP
            }
        }

        // <summary> returns true if actualy parried anything </summary>
        public bool GetParried()
        {
            if (Parriable)
            {
                Attacking = false;
                Parriable = false;
                StopCoroutine(_attackingCoroutine);
                _attackingCoroutine = null;
                transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;//TEMP
                transform.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;//TEMP
                StartCoroutine(Recover(ParryRecoveryTime));
                return true;
            }
            return false;
        }

        public void GetInterupted()
        {
            if (Attacking)
            {
                Attacking = false;
                StopCoroutine(_attackingCoroutine);
                _attackingCoroutine = null;
                transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;//TEMP
            }
        }

        private IEnumerator DoTheAttack()
        {
            yield return new WaitForSeconds(0.6f*SwingTime);
            Parriable = true;
            transform.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.red;//TEMP
            yield return new WaitForSeconds(0.4f * SwingTime);
            Parriable = false;
            transform.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;//TEMP

            for (int i = 0; i < Targets.Count; i++)
            {
                Targets[i].TakeAHit(_holder, WeaponStats);
                if (Targets[i].Health <= 0)
                {
                    Targets.Remove(Targets[i]);
                    i--;
                }
            }

            Attacking = false;
            StartCoroutine(Recover(AttackRecoveryTime));
            transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;//TEMP
        }

        private IEnumerator Recover(float time)
        {
            Recovering = true;
            yield return new WaitForSeconds(time);
            Recovering = false;
        }
    }
}
