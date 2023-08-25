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
            None, Player, AI
        }
        
        [HideInInspector]
        public float ItemHealthMod = 1;
        [HideInInspector]
        public float ItemSpeedMod = 1;
        [HideInInspector]
        public float ItemDashDistMod = 1;
        [HideInInspector]
        public float ItemDashCooldownMod = 1;
        [HideInInspector]
        public float ItemAttackSpeedMod = 1; //TODO implement

        public int Health { get; private set; }
        [Space]
        [SerializeField]
        private int _baseMaxHealth = 10;
        public int MaxHealth => Mathf.CeilToInt(_baseMaxHealth * ItemHealthMod);

        [Header("Movement")]
        [SerializeField]
        private float _baseSpeed = 3f;
        public float Speed => _baseSpeed * ItemSpeedMod;
        public float Acceleration = 10f;
        public float KnockbackRecovery = 3f;

        public Inventory Inventory { get; protected set; }
        public FellaVisuals Visuals { get; protected set; }
        public bool BowEquipped => Inventory.InHands != null && Inventory.InHands.ThreatRange > 5;
        protected CharacterController _characterController;
        private Animator _animator;

        public Vector3 MovementDir { get; protected set; }
        protected Vector3 _additionalVelocities;
        private Vector3 _movementNow;

        protected bool _decidedToAttack;
        protected bool _decidedToLetGoAttack;
        protected bool _decidedToParry;
        protected bool _decidedToSwitchWeapons;
        protected bool _decidedToUseItem1;
        protected bool _decidedToUseItem2;
        protected bool _decidedToUseItem3;
        protected bool _decidedToUseItem4;
        public float LookRot;
        protected float _speedControll = 1f;

        private float _temporarySlow = 0f;
        
        protected bool _canDash = true;
        protected bool _canParry = true;
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
            _animator = GetComponent<Animator>();

            Inventory.ApplyItemEffects();

            AdditionalAwake();
        }
        protected virtual void AdditionalAwake() { }


        private void Update()
        {
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

            _speedControll = Mathf.Clamp01(_speedControll);
            _temporarySlow = Mathf.Lerp(Mathf.Clamp01(_temporarySlow), 0f, KnockbackRecovery * 0.5f * Time.deltaTime);
            float anotherModifier = 1f;
            Vector3 finalMovement = Speed * _speedControll * (1-_temporarySlow) * anotherModifier * _movementNow + _additionalVelocities;
            _characterController.Move(finalMovement * Time.deltaTime);

            _animator.SetBool("Walking", finalMovement.sqrMagnitude > 0.1);

            transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
        }

        protected void CombatActions()
        {
            if(Inventory.WeaponGO != null)
                Inventory.WeaponGO.transform.rotation = Quaternion.Euler(0f, Mathf.LerpAngle(Inventory.WeaponGO.transform.rotation.eulerAngles.y, LookRot, Time.deltaTime*10f), 0f);
            
            if(_decidedToUseItem1 && Inventory.Item1 != null)
            {
                _decidedToUseItem1 = false;
                if(Inventory.Item1Object.Use())
                {
                    Inventory.Item1Object = null;
                    Inventory.Item1 = null;
                }
            }
            if(_decidedToUseItem2 && Inventory.Item2 != null)
            {
                _decidedToUseItem2 = false;
                if(Inventory.Item2Object.Use())
                {
                    Inventory.Item2Object = null;
                    Inventory.Item2 = null;
                }
            }
            if(_decidedToUseItem3 && Inventory.Item3 != null)
            {
                _decidedToUseItem3 = false;
                if(Inventory.Item3Object.Use())
                {
                    Inventory.Item3Object = null;
                    Inventory.Item3 = null;
                }
            }
            if(_decidedToUseItem4 && Inventory.Item4 != null)
            {
                _decidedToUseItem4 = false;
                if(Inventory.Item4Object.Use())
                {
                    Inventory.Item4Object = null;
                    Inventory.Item4 = null;
                }
            }

            if(_decidedToSwitchWeapons)
            {
                _decidedToSwitchWeapons = false;
                SwapWeapons();
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
            if(_decidedToParry && _canParry)
            {
                _decidedToParry = false;
                Vector3 lookDir = Quaternion.Euler(0f,LookRot, 0f) * Vector3.forward;
                if(Inventory.Weapon.Parry(lookDir))
                {
                    SuccessfulParry();
                }
                else
                {
                    StartCoroutine(ParryCooldown(.7f));
                }
            }
        }

        protected virtual void SwapWeapons()
        {
            Inventory.SwapWeapons();
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

        public void StopBleeding()
        {
            if(_bleedingCoroutine != null)
                StopCoroutine(_bleedingCoroutine);
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
            Visuals.ParryEffects(Quaternion.Euler(0f, LookRot, 0f) * Vector3.forward);
        }

        protected virtual void Dash(Vector3 dir)
        {
            if (!_canDash) return;
            _additionalVelocities += 3f * dir.normalized * ItemDashDistMod;
            StartCoroutine(DashCooldown(1f * ItemDashCooldownMod));
        }

        private IEnumerator DashCooldown(float t)
        {
            _canDash = false;
            yield return new WaitForSeconds(t);
            _canDash = true;
        }

        private IEnumerator ParryCooldown(float t)
        {
            _canParry = false;
            yield return new WaitForSeconds(t);
            _canParry = true;
        }

        public virtual void ChangeHealth(int change)
        {
            Health = Mathf.Clamp(Health+change, 0, MaxHealth);
            Visuals.UpdateBlood(Health,MaxHealth);
        }

        public virtual bool TakeAHit(CombatFella attacker, float damage, float armourEffectiveness = 1f)
        {
            if(_invoulnerable) return false;

            float damageModifier = 1f;

            damageModifier *= 1 - armourEffectiveness * Mathf.Sqrt(Inventory.GetArmor()/30f);

            ChangeHealth(-Mathf.CeilToInt(damage * damageModifier));

            Vector3 dir = (transform.position - attacker.transform.position).normalized;

            dir = Vector3.Scale(dir, new Vector3(1f,0f,1f));
            _movementNow = Vector3.zero;
            _additionalVelocities += 2f * dir;
            
            Visuals.HitEffects(dir, Health, MaxHealth);

            if (Health <= 0)
            {
                attacker.Killed(this);
                AllTheFellas.Remove(this);
                Die();
            }
            return true;
        }

        public virtual bool GetParried()
        {
            if (Inventory.Weapon.GetParried())
            {
                DashCooldown(1f);
                _temporarySlow = 1f;
                return true;
            }
            return false;
        }

        public void StepSound()
        {
            SoundManager.Instance.PlayRandomEffect(SoundManager.Instance.AudioEffects.Step, 0.1f);
        }
    }
}
