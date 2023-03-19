using System.Collections;
using System.Collections.Generic;
using UnityEditor.Compilation;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Game
{
    public abstract class CombatFella : MonoBehaviour
    {
        // This is a base class
        // Children classes are responsible for moving and generaly controlling the entity

        public static List<CombatFella> AllTheFellas { get; protected set; } = new List<CombatFella>();
        public FellaType Type { get; protected set; }
        public enum FellaType
        { 
            Player, Test
        }



        public int Health { get; private set; }
        private int _stamina; //TODO implement

        public int MaxHealth = 10;
        public int MaxStamina = 10;

        [Header("Movement")]
        public float Speed = 3f;
        public float Acceleration = 5f;
        public float KnockbackRecovery = 3f;



        protected CharacterController _characterController;
        protected Inventory _inventory;

        protected Vector3 _movementDir;
        protected Vector3 _additionalVelocities;
        private Vector3 _movementNow;

        protected bool _decidedToAttack;
        protected bool _decidedToParry;
        protected bool _decidedToBlock;
        protected float _lookRot;
        protected float _speedModifier = 1f;
        protected float _temporarySlow = 0f;

        private float _dashCooldown = 0f;

        private void OnEnable()
        {
            AllTheFellas.Add(this);
        }

        private void OnDisable()
        {
            AllTheFellas.Remove(this);
        }

        private void Awake()
        {
            Health = MaxHealth;
            _stamina = MaxStamina;

            _characterController = GetComponent<CharacterController>();
            _inventory = GetComponent<Inventory>();

            AdditionalAwake();
        }
        protected virtual void AdditionalAwake() { }


        private void Update()
        {
            _dashCooldown -= Time.deltaTime;
            if (_dashCooldown < 0f) _dashCooldown = 0f; 

            Decide();
            Movement();
            CombatActions();

            AdditionalUpdate();
        }
        protected abstract void Decide();
        protected virtual void AdditionalUpdate() { }
        protected void Movement()
        {
            _movementNow = Vector3.Lerp(_movementNow, _movementDir.normalized, Acceleration * Time.deltaTime);
            _additionalVelocities = Vector3.Lerp(_additionalVelocities, Vector3.zero, KnockbackRecovery * Time.deltaTime);

            _speedModifier = Mathf.Clamp01(_speedModifier);
            _temporarySlow = Mathf.Lerp(Mathf.Clamp01(_temporarySlow), 0f, KnockbackRecovery * 0.5f * Time.deltaTime);
            float anotherModifier = 1f;
            if (_decidedToBlock) anotherModifier *= 0.3f;
            _characterController.Move((Speed * _speedModifier * (1-_temporarySlow) * anotherModifier * _movementNow + _additionalVelocities) * Time.deltaTime);
        }
        protected void CombatActions()
        {
            _inventory.WeaponGO.transform.rotation = Quaternion.Euler(0f, _lookRot, 0f);
            if (_decidedToAttack)
            {
                _decidedToAttack = false;
                _decidedToBlock = false;
                _inventory.Weapon.StartAttack();
            }
            if(_decidedToParry)
            {
                _inventory.Weapon.GetInterupted();
                _decidedToParry = false;
                Vector3 lookDir = Quaternion.Euler(0f,_lookRot, 0f) * Vector3.forward;
                foreach (var fella in AllTheFellas)
                {
                    if (fella == this) continue;
                    Vector3 fellaDir = fella.transform.position - transform.position;
                    if (Vector3.Angle(lookDir, fellaDir) < 30f)
                        fella.GetParried();
                }
            }
            if(_decidedToBlock)
            {
                _inventory.Weapon.GetInterupted();
            }
        }

        protected void Dash(Vector3 dir)
        {
            if (_dashCooldown != 0f) return;
            _additionalVelocities += 3f * dir.normalized;
            _dashCooldown = 1f;
        }
        public void TakeAHit(CombatFella attacker, WeaponStatsSO weaponStats)
        {
            float damageModifier = 1f;

            if (_decidedToBlock)
                damageModifier *= 0.3f;

            Health -= Mathf.FloorToInt(weaponStats.BaseDamage * damageModifier);

            Debug.Log(Health);

            if (Health <= 0)
            {
                Destroy(gameObject);
            }

            Vector3 dir = (transform.position - attacker.transform.position).normalized;
            _movementNow = Vector3.zero;
            _additionalVelocities += 2f * dir;
        }

        public virtual bool GetParried()
        {
            if (_inventory.Weapon.GetParried())
            {
                _dashCooldown = 1f;
                _temporarySlow = 1f;
                return true;
            }
            return false;
        }
    }
}
