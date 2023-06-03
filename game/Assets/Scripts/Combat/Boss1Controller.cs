using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [DefaultExecutionOrder(5)]
    public class Boss1Controller : CombatFella
    {

        [Space]
        [SerializeField]
        private float _parryChance = 0.6f;
        [SerializeField]
        private float _chanceToFollowOthers = 0.3f;

        private Transform _playerTransform;
        private CombatFella _playerFella;
        private Inventory _playerInventory;

        private int _attacksInRow;
        private int _wantedAttacksInRow;
        private float _timeSinceAttack;
        private float _attackWaitTime = 0f;
        private bool _defensiveReactionDecided;
        private bool _willParry;
        private bool _canDoActions = true;

        private Coroutine _actionBlockCoroutine;

        private FightStance _stance = FightStance.Defensive;

        private LayerMask _terrainLayer;

        private enum FightStance
        {
            Idle, Defensive, Offensive
        }

        private bool _phase1 = true;
        private bool _shooting = false;



        override protected void AdditionalAwake()
        {
            Inventory.Arrows = 100000;
            _parryChance = 1f;

            Type = FellaType.Test;
            _terrainLayer = LayerMask.NameToLayer("Terrain");
            
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
                return;
                
            Vector3 toPlayer = _playerTransform.position - transform.position;
            float toPlayerDist = toPlayer.magnitude;
            Vector3 toPlayerDir = toPlayer.normalized;
            _speedModifier = 1f;

            // Movement ---------------------------------------------------------
            // Set desired distance from player 
            float playerDistGoal = 0f;
            if (_stance == FightStance.Offensive || _playerFella.BowEquipped || BowEquipped)
            {
                playerDistGoal = Inventory.Weapon.ItemStats.ThreatRange - 0.25f;
            }
            else if (_stance == FightStance.Defensive)
            {
                playerDistGoal = _playerInventory.Weapon.ItemStats.ThreatRange + 0.4f * Inventory.Weapon.ItemStats.ThreatRange;
            }

            // Move dir and look rot 
            MovementDir = Vector3.ClampMagnitude((toPlayer - playerDistGoal * toPlayerDir), 1);
            if(!Inventory.Weapon.Attacking)
                _lookRot = Mathf.Rad2Deg * Mathf.Atan2(toPlayer.x, toPlayer.z);
            else if(_phase1)
            {
                Vector3 toPlayerPredict = toPlayer + toPlayerDist*_playerFella.MovementDir*0.1f;
                _lookRot = Mathf.Rad2Deg * Mathf.Atan2(toPlayerPredict.x, toPlayerPredict.z);
            }
            else
            {
                _lookRot = Mathf.LerpAngle(_lookRot, Mathf.Rad2Deg * Mathf.Atan2(toPlayer.x, toPlayer.z), Time.deltaTime*4f);
            }

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

            _speedModifier = MovementDir.sqrMagnitude * 10f;
            if(_phase1) _speedModifier = 0;

            // Combat -----------------------------------------------------------
            
            if(_timeSinceAttack > _attackWaitTime || toPlayerDist < 0.5f*Inventory.Weapon.ItemStats.ThreatRange)
            {
                // Random attack chance and if too close
                _stance = FightStance.Offensive;
                CallOthersToAttack();
                _attackWaitTime = 1f + Random.value * 3f;
                _timeSinceAttack = 0f;
                _wantedAttacksInRow = (int)(1+Random.value*3.5f);
            }
            _timeSinceAttack += Time.deltaTime;

            // Parrying
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
                    if(_phase1)
                    {
                        _phase1 = false;
                        _parryChance = 0.4f;
                        _decidedToSwitchWeapons = true;
                        _decidedToParry = true;
                    }
                    else
                    {
                        _willParry = false;
                        StartCoroutine(ParryAfterDelay(0.1f+0.5f*Random.value*_playerInventory.Weapon.ItemStats.BaseSwingTime));
                    }

                }
            }
            else _defensiveReactionDecided = false;

            if(!_canDoActions)
            {
                // :)
            }
            if(_phase1)
            {
                if(!_shooting) 
                    StartShooting();
            }
            else if(_stance == FightStance.Offensive)
            {
                // Offensive actions

                if(toPlayerDist > playerDistGoal+0.5f)
                {
                    // Dash towards player / disengage if too far
                    if(_timeSinceAttack > 2f)
                    {
                        _stance = FightStance.Defensive;
                    }
                    StartCoroutine(DashAfterDelay(0.1f, toPlayerDir));
                }
                else if (!Inventory.Weapon.Attacking && !Inventory.Weapon.Recovering)
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
                        _stance = FightStance.Defensive;
                    }
                }
            }
            else if (_stance == FightStance.Defensive && _playerInventory.Weapon.Attacking)
            {
                // Defensive actions

                // Dash away from player
                if (toPlayerDist < 0.8f*playerDistGoal)
                    StartCoroutine(DashAfterDelay(0.1f, -toPlayer));
            }
        }

        // This was necessary, as the fella would interupt his own attack
        private IEnumerator SwitchStanceNextFrame()
        {
            yield return 0;
            _stance = _stance == FightStance.Defensive ? FightStance.Offensive : FightStance.Defensive;
            if (_stance == FightStance.Offensive) CallOthersToAttack();
        }

        public override bool GetParried()
        {
            if(base.GetParried())
            {
                _stance = FightStance.Defensive;
                if(_actionBlockCoroutine != null) 
                    StopCoroutine(_actionBlockCoroutine);
                _actionBlockCoroutine = StartCoroutine(BlockActions(1f));
                return true;
            }
            return false;
        }

        private IEnumerator BlockActions(float time)
        {
            _canDoActions = false;
            yield return new WaitForSeconds(0.5f*time);
            _canDoActions = true;
        }

        public IEnumerator CalledToAttack()
        {
            if(BowEquipped) 
                yield break;
            _timeSinceAttack = 0f;
            if (Random.value < _chanceToFollowOthers)
            {
                yield return new WaitForSeconds(0.3f + Random.value * 0.5f);
                _stance = FightStance.Offensive;
            }
        }

        private void CallOthersToAttack()
        {
            foreach (var fella in AllTheFellas)
            {
                if(fella.Type != FellaType.Player && fella != this)
                {
                    StartCoroutine(((BasicAIController)fella).CalledToAttack());
                }
            }
        }

        private void StartShooting()
        {
            StartCoroutine(DrawBow());
            _shooting = true;
        }

        private void StopShooting()
        {
            _decidedToLetGoAttack = true;
            StopCoroutine("DrawBow");
            _shooting = false;
        }

        private IEnumerator DrawBow()
        {
            yield return null;
            _decidedToAttack = true;
            yield return new WaitForSeconds(Inventory.InHands.BaseSwingTime + 0.1f);
            _decidedToLetGoAttack = true;
            yield return new WaitForSeconds(Inventory.Weapon.AttackRecoveryTime + 0.1f);
            StartCoroutine(DrawBow());
        }

        private IEnumerator DashAfterDelay(float delay, Vector3 dir)
        {
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
