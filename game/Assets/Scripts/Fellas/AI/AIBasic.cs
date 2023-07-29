using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class AIBasic : CombatFella
    {
        [Header("AI")]
        [SerializeField]
        private float _parryChance = 0.2f;
        [SerializeField]
        private float _chanceToFollowOthers = 0.2f;
        [SerializeField]
        private float _attackWaitMod = 1f;

        private float _weaveTimeOffset;

        private Transform _playerTransform;
        private CombatFella _playerFella;
        private Inventory _playerInventory;

        protected int _attacksInRow;
        protected int _wantedAttacksInRow;
        protected float _timeSinceAttack;
        protected float _attackWaitTime = 0f;
        protected bool _defensiveReactionDecided;
        protected bool _willParry;
        private bool _canDoActions = true;

        private Coroutine _actionBlockCoroutine;
        
        [HideInInspector]
        public bool SpawnDestroyed = false;

        [HideInInspector]
        public FightStance Stance = FightStance.Idle;

        public enum FightStance
        {
            Idle, Defensive, Offensive
        }



        override protected void AdditionalAwake()
        {
            _weaveTimeOffset = Random.value*10;

            Type = FellaType.AI;
            
            SetPlayerReferences();
        }

        private void SetPlayerReferences()
        {
            foreach (CombatFella fella in AllTheFellas)
            {
                if (fella.Type == FellaType.Player)
                {
                    _playerTransform = fella.transform;
                    _playerFella = fella.GetComponent<CombatFella>();
                    _playerInventory = fella.GetComponent<Inventory>();
                    break;
                }
            }
        }



        protected override void Decide()
        {
            if(_playerFella == null)
            {
                SetPlayerReferences();
                return;
            }

            Vector3 toPlayer = _playerTransform.position - transform.position;
            float toPlayerDist = toPlayer.magnitude;
            Vector3 toPlayerDir = toPlayer.normalized;
            _speedControll = 1f;

            if(SpawnDestroyed && toPlayerDist > LevelGenerator.Instance.SeeingRange)
            {
                Destroy(gameObject);
            }

            // Player detection
            if(Stance == FightStance.Idle)
            {
                if(DetectPlayer(toPlayerDist, out bool warnOthers) || Health != MaxHealth)
                {
                    Stance = FightStance.Defensive;

                    // Warn others
                    if(warnOthers)
                    {
                        foreach (var fella in AllTheFellas)
                        {
                            if(fella.Type != FellaType.Player && fella != this 
                                && (fella.transform.position-transform.position).magnitude < LevelGenerator.Instance.SeeingRange)
                            {
                                ((AIBasic)fella).Stance = FightStance.Defensive;
                            }
                        }
                    }
                }
                return;
            }

            // Movement ---------------------------------------------------------
            float playerDistGoal = GetDesiredPlayerDist();

            // Move dir and look rot 
            LookRot = GetLookRot(toPlayer, toPlayerDist, _playerFella);

            MovementDir = Vector3.ClampMagnitude((toPlayer - playerDistGoal * toPlayerDir), 1);
            MovementDir += Inventory.WeaponGO.transform.right * (Mathf.PerlinNoise1D(Time.time*Speed*0.05f*_weaveTimeOffset + _weaveTimeOffset)*2-1);   // Weaving

            // Keep distance from other fellas
            foreach (CombatFella fella in AllTheFellas)
            {
                if (fella != this && !fella.BowEquipped && fella.Type == FellaType.AI)
                {
                    Vector3 friendToHere = transform.position - fella.transform.position;
                    if(friendToHere.sqrMagnitude < 8)
                        MovementDir += friendToHere.normalized * Mathf.Max(-1, (5 - friendToHere.sqrMagnitude) / 5);
                }
            }

            // Check line of sight to player
            bool clearLineToPlayer = true;
            if(Physics.SphereCast(0.5f*Vector3.up+transform.position, 0.3f, toPlayerDir, out RaycastHit hit, toPlayerDist-1.2f))
            {
                clearLineToPlayer = false;
            }

            MovementAdjustements(toPlayerDir);

            // Obstacle avoidance
            if(Physics.Raycast(0.5f*Vector3.up+transform.position, MovementDir, out RaycastHit hit_, 5f))
            {
                if(hit_.transform.GetComponent<CombatFella>() == null)
                {
                    Debug.DrawRay(0.5f*Vector3.up+transform.position, 5f* MovementDir.normalized);
                    for(float angle = 20f; angle < 120f; angle += 20f)
                    {
                        if(Physics.Raycast(transform.position, Quaternion.Euler(0f,angle,0f)*MovementDir, 5f))
                        {
                            Debug.DrawRay(transform.position, Quaternion.Euler(0f,angle,0f)*MovementDir.normalized*5f);
                            if(!Physics.Raycast(transform.position, Quaternion.Euler(0f,-angle,0f)*MovementDir, 5f))
                            {
                                MovementDir = Quaternion.Euler(0f,-angle-10,0f) * MovementDir;
                                break;
                            }
                            Debug.DrawRay(transform.position, Quaternion.Euler(0f,-angle,0f)*MovementDir.normalized*5f);
                        }
                        else
                        {
                            MovementDir = Quaternion.Euler(0f,angle+10,0f) * MovementDir;
                            break;
                        }
                    }
                }
            }

            _speedControll = MovementDir.sqrMagnitude * 10f;
            if(!_canDoActions) _speedControll = 0.2f;

            // Combat -----------------------------------------------------------

            _timeSinceAttack += Time.deltaTime;

            if(!_canDoActions) return;
            
            Parrying(toPlayerDist);

            switch (Stance)
            {
                case(FightStance.Offensive):
                    OffensiveActions(clearLineToPlayer, toPlayerDist, playerDistGoal, toPlayerDir);
                    break;
                case(FightStance.Defensive):
                    DefensiveActions(toPlayerDist, playerDistGoal, toPlayerDir);
                    break;
            }

            AfterActions(toPlayerDist);
        }

        protected virtual bool DetectPlayer(float distance, out bool warnOthers)
        {
            warnOthers = true;
            if(LevelGenerator.Instance == null) return true;
            return distance < 0.5f*LevelGenerator.Instance.SeeingRange;
        }

        protected virtual float GetDesiredPlayerDist()
        {
            if (Stance == FightStance.Offensive || _playerFella.BowEquipped)
            {
                return Inventory.Weapon.ItemStats.ThreatRange - 0.25f;
            }
            else if (Stance == FightStance.Defensive)
            {
                return _playerInventory.Weapon.ItemStats.ThreatRange + 0.2f * Inventory.Weapon.ItemStats.ThreatRange;
            }
            Debug.LogError("didn't account for this");
            return 0f;
        }

        protected virtual void MovementAdjustements(Vector3 toPlayerDir) {}
        protected virtual void AfterActions(float toPlayerDist) {}

        protected virtual float GetLookRot(Vector3 toPlayer, float toPlayerDist, CombatFella playerFella)
        {
            if(!_canDoActions)
            {
                return LookRot = Mathf.LerpAngle(LookRot, Mathf.Rad2Deg * Mathf.Atan2(toPlayer.x, toPlayer.z), Time.deltaTime*0.1f);
            }
            if(Inventory.Weapon.Attacking)
            {
                return LookRot = Mathf.LerpAngle(LookRot, Mathf.Rad2Deg * Mathf.Atan2(toPlayer.x, toPlayer.z), Time.deltaTime*4f);
            }
            return LookRot = Mathf.Rad2Deg * Mathf.Atan2(toPlayer.x, toPlayer.z);
        }

        protected virtual void DefensiveActions(float toPlayerDist, float playerDistGoal, Vector3 toPlayerDir)
        {
            // Random attack chance and if too close
            if(_timeSinceAttack > _attackWaitTime || toPlayerDist < 0.5f*Inventory.Weapon.ItemStats.ThreatRange)
            {
                Stance = FightStance.Offensive;
                CallOthersToAttack();
                _attackWaitTime = _attackWaitMod * 1f + Random.value * 3f;
                _timeSinceAttack = 0f;
                _wantedAttacksInRow = (int)(1+Random.value*3.5f);
            }
            
            // Dash away from player
            if (_canDoActions && toPlayerDist < 0.8f*playerDistGoal)
                StartCoroutine(DashAfterDelay(0.1f, -toPlayerDir));
        }

        protected virtual void Parrying(float toPlayerDist)
        {
            if (_canDoActions && _playerInventory.Weapon.Attacking == true && toPlayerDist < _playerInventory.Weapon.ItemStats.ThreatRange+0.6f)
            {
                if (!_defensiveReactionDecided)
                {
                    float roll = Random.value;
                    _defensiveReactionDecided = true;
                    if (roll < _parryChance) 
                    {
                        _willParry = true;
                    }
                }
                if (_willParry && _playerInventory.Weapon.Parriable)
                {
                    _willParry = false;
                    StartCoroutine(ParryAfterDelay(0.5f*Random.value*_playerInventory.Weapon.ItemStats.BaseSwingTime));
                }
            }
            else _defensiveReactionDecided = false;

        }

        protected virtual void OffensiveActions(bool clearLineToPlayer, float toPlayerDist, float playerDistGoal, Vector3 toPlayerDir)
        {
            if(toPlayerDist > playerDistGoal+0.5f)
            {
                // Player is too far away
                // Disengage
                if(_timeSinceAttack > 2f)
                {
                    Stance = FightStance.Defensive;
                }
                // Dash towards player
                StartCoroutine(DashAfterDelay(0.1f, toPlayerDir));
            }
            else if (!Inventory.Weapon.Attacking && !Inventory.Weapon.Recovering && clearLineToPlayer)
            {
                // Attacks
                if (_attacksInRow < _wantedAttacksInRow )
                {
                    _decidedToAttack = true;
                    _timeSinceAttack = 0f;
                    _attacksInRow++;
                }
                else
                {
                    _attacksInRow = 0;
                    Stance = FightStance.Defensive;
                }
            }
        }

        // This was necessary, as the fella would interupt his own attack
        private IEnumerator SwitchStanceNextFrame()
        {
            yield return 0;
            Stance = Stance == FightStance.Defensive ? FightStance.Offensive : FightStance.Defensive;
            if (Stance == FightStance.Offensive) CallOthersToAttack();
        }

        public override bool GetParried()
        {
            if(base.GetParried())
            {
                GotParriedEffect();
                return true;
            }
            return false;
        }

        protected virtual void GotParriedEffect()
        {
            _attacksInRow = 0;
            Stance = FightStance.Defensive;
            BlockActionsForTime(1f);
        }

        public void BlockActionsForTime(float time)
        {
            Inventory.Weapon.GetInterupted(true);
            if(_actionBlockCoroutine != null) 
                StopCoroutine(_actionBlockCoroutine);
            _actionBlockCoroutine = StartCoroutine(BlockingActions(time));
        }

        private IEnumerator BlockingActions(float time)
        {
            _canDoActions = false;
            yield return new WaitForSeconds(time);
            _canDoActions = true;
        }

        public virtual IEnumerator CalledToAttack(Vector3 callerPos)
        {
            _timeSinceAttack = 0f;
            if (Random.value < _chanceToFollowOthers)
            {
                yield return new WaitForSeconds(0.3f + Random.value * 0.5f);
                Stance = FightStance.Offensive;
            }
        }

        protected virtual void CallOthersToAttack()
        {
            foreach (var fella in AllTheFellas)
            {
                if(fella.Type != FellaType.Player && fella != this)
                {
                    StartCoroutine(((AIBasic)fella).CalledToAttack(transform.position));
                }
            }
        }

        private IEnumerator DashAfterDelay(float delay, Vector3 dir)
        {
            if(!_canDash) yield break;
            yield return new WaitForSeconds(delay);
            Dash(dir);
        }
        
        private IEnumerator ParryAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            _decidedToParry = true;
            StartCoroutine(SwitchStanceNextFrame());
        }

        protected override void Die()
        {
            Destroy(gameObject);
        }
    }
}
