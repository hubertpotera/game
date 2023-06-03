using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public abstract class CombatFella : MonoBehaviour
    {
        // This is a base class
        // Children classes are responsible for setting fella type and deciding for the entity

        public static List<CombatFella> AllTheFellas { get; protected set; } = new List<CombatFella>();
        public FellaType Type { get; protected set; }
        public enum FellaType
        {
            Player, Test
        }


        public int Health { get; private set; }
        [Space]
        public int MaxHealth = 10;

        private int _combatExpertice;
        public int GetCombatLevel()
        {
            if(_combatExpertice > 50) return 1;
            if(_combatExpertice > 150) return 2;
            if(_combatExpertice > 300) return 3;
            if(_combatExpertice > 500) return 4;
            return 0;
        }

        [Header("Movement")]
        public float Speed = 3f;
        public float Acceleration = 10f;
        public float KnockbackRecovery = 3f;

        [HideInInspector]
        public Inventory Inventory { get; protected set; }
        public FellaVisuals Visuals { get; protected set; }
        public bool BowEquipped => Inventory.InHands.ThreatRange > 5;
        protected CharacterController _characterController;

        public Vector3 MovementDir { get; protected set; }
        protected Vector3 _additionalVelocities;
        private Vector3 _movementNow;

        protected bool _decidedToAttack;
        protected bool _decidedToLetGoAttack;
        protected bool _decidedToParry;
        protected bool _decidedToSwitchWeapons;
        protected float _lookRot;
        protected float _speedModifier = 1f;

        private float _temporarySlow = 0f;
        private float _dashCooldown = 0f;
        
        private bool _invoulnerable = false;
        private Coroutine _iFramesCoroutine;
        private Coroutine _bleedingCoroutine;


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

            _characterController = GetComponent<CharacterController>();
            Inventory = GetComponent<Inventory>();
            Visuals = GetComponent<FellaVisuals>();

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
        }
        protected abstract void Decide();
        protected abstract void Die();
        protected virtual void Killed(CombatFella killedOne) {}
        protected virtual void Attack() {}

        protected void Movement()
        {
            MovementDir = Vector3.Scale(MovementDir, new Vector3(1f,0f,1f));

            _movementNow = Vector3.Lerp(_movementNow, MovementDir.normalized, Acceleration * Time.deltaTime);
            _additionalVelocities = Vector3.Lerp(_additionalVelocities, Vector3.zero, KnockbackRecovery * Time.deltaTime);

            _speedModifier = Mathf.Clamp01(_speedModifier);
            _temporarySlow = Mathf.Lerp(Mathf.Clamp01(_temporarySlow), 0f, KnockbackRecovery * 0.5f * Time.deltaTime);
            float anotherModifier = 1f;
            _characterController.Move((Speed * _speedModifier * (1-_temporarySlow) * anotherModifier * _movementNow + _additionalVelocities) * Time.deltaTime);
        }

        protected void CombatActions()
        {
            Inventory.WeaponGO.transform.rotation = Quaternion.Euler(0f, Mathf.LerpAngle(Inventory.WeaponGO.transform.rotation.eulerAngles.y, _lookRot, Time.deltaTime*10f), 0f);
            
            if(_decidedToSwitchWeapons)
            {
                _decidedToSwitchWeapons = false;
                Inventory.SwapWeapons();
            }
            if (_decidedToAttack)
            {
                if(!Inventory.Weapon.Recovering)
                {
                    _decidedToAttack = false;
                }
                    
                if(Inventory.Weapon.StartAttack())
                {
                    Attack();
                }
            }
            if(_decidedToLetGoAttack)
            {
                _decidedToLetGoAttack = false;
                Inventory.Weapon.AttackLetGo();
            }
            if(_decidedToParry)
            {
                _decidedToParry = false;
                Vector3 lookDir = Quaternion.Euler(0f,_lookRot, 0f) * Vector3.forward;
                if(Inventory.Weapon.Parry(lookDir))
                {
                    SuccessfulParry();
                }
            }
        }

        protected void StartInvulnerability(float t)
        {
            //TODO indicate this
            if(_iFramesCoroutine != null)
                StopCoroutine(_iFramesCoroutine);
            _iFramesCoroutine = StartCoroutine(InvulnerabilityCoroutine(t));
        }

        private IEnumerator InvulnerabilityCoroutine(float time)
        {
            _invoulnerable = true;
            yield return new WaitForSeconds(time);
            _invoulnerable = false;
        }

        public void StartBleeding(int ammount, CombatFella attacker)
        {
            //TODO indicate this
            if(_bleedingCoroutine != null)
                StopCoroutine(_bleedingCoroutine);
            _bleedingCoroutine = StartCoroutine(BleedCoroutine(ammount, attacker));
        }
        
        private IEnumerator BleedCoroutine(int ammount, CombatFella attacker)
        {
            for (int i = 0; i < ammount; i++)
            {
                yield return new WaitForSeconds(1f);
                TakeAHit(attacker, 1, 0);
            }
        }

        protected virtual void SuccessfulParry()
        {
            StartInvulnerability(0.2f);
        }

        protected virtual void Dash(Vector3 dir)
        {
            if (_dashCooldown != 0f) return;
            _additionalVelocities += 3f * dir.normalized;
            _dashCooldown = 1f;
        }

        public virtual void ChangeHealth(int change)
        {
            Health = Mathf.Clamp(Health+change, 0, MaxHealth);
            Visuals.UpdateBlood(Health,MaxHealth);
        }

        public bool TakeAHit(CombatFella attacker, int damage, float armourEffectiveness = 1f)
        {
            if(_invoulnerable) return false;

            float damageModifier = 1f;

            damageModifier *= 1 - armourEffectiveness * Mathf.Sqrt(Inventory.GetArmor()/30f);

            ChangeHealth(-Mathf.Max(1, Mathf.FloorToInt(damage * damageModifier))); // i dont know if min 1 is a good solution

            Vector3 dir = (transform.position - attacker.transform.position).normalized;

            dir = Vector3.Scale(dir, new Vector3(1f,0f,1f));
            _movementNow = Vector3.zero;
            _additionalVelocities += 2f * dir;
            
            StartCoroutine(Visuals.HitEffects(dir, Health, MaxHealth));

            if (Health <= 0)
            {
                attacker.Killed(this);
                AllTheFellas.Remove(this);
                Die();
            }
            return true;
        }

        public void GainExpertice(int ammount)
        {
            _combatExpertice += ammount;
            Inventory.Weapon.UpdateParameters();
        }

        public virtual bool GetParried()
        {
            if (Inventory.Weapon.GetParried())
            {
                _dashCooldown = 1f;
                _temporarySlow = 1f;
                return true;
            }
            return false;
        }
    }
}
