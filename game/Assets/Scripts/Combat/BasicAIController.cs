using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Game
{
    public class BasicAIController : CombatFella
    {
        [SerializeField]
        private float _parryChance = 0.6f;
        [SerializeField]
        private float _chanceToFollowOthers = 0.3f;

        private Transform _playerTransform;
        private Inventory _playerInventory;

        private int _attacksInRow;
        private float _timeSinceAttack;
        private float _attackWaitTime;
        private bool _defensiveReactionDecided;
        private bool _willParry;

        private FightStance _stance = FightStance.Defensive;

        private enum FightStance
        {
            Idle, Defensive, Offensive
        }



        override protected void AdditionalAwake()
        {
            Type = FellaType.Test;
            _attackWaitTime = 2f + Random.value * 5f;

            StartCoroutine(SetPlayer());
        }

        private IEnumerator SetPlayer()
        {
            yield return new WaitForEndOfFrame();
            foreach (CombatFella fella in AllTheFellas)
            {
                if (fella.Type == FellaType.Player)
                {
                    _playerTransform = fella.transform;
                    _playerInventory = fella.GetComponentInChildren<Inventory>();
                    break;
                }
            }
        }



        protected override void Decide()
        {
            // Movement -------------------------------------------
            if (_playerTransform == null || _playerInventory.Weapon == null) return;

            Vector3 toPlayer = _playerTransform.position - transform.position;
            float toPlayerDist = toPlayer.magnitude;
            Vector3 toPlayerDir = toPlayer.normalized;
            _speedModifier = 1f;

            float playerDistGoal = 0f;
            if (_stance == FightStance.Defensive)
                playerDistGoal = _playerInventory.Weapon.WeaponStats.ThreatRange + 0.5f * _inventory.Weapon.WeaponStats.ThreatRange;
            if (_stance == FightStance.Offensive)
                playerDistGoal = _inventory.Weapon.WeaponStats.ThreatRange - 0.25f;

            _movementDir = Vector3.ClampMagnitude((toPlayer - playerDistGoal * toPlayerDir), 1);

            _lookRot = Mathf.Rad2Deg * Mathf.Atan2(toPlayer.x, toPlayer.z);

            List<CombatFella> friends = new List<CombatFella>();
            foreach (CombatFella fella in AllTheFellas)
            {
                if (fella != this && fella.Type == FellaType.Test)
                    friends.Add(fella);
            }

            if (friends.Count > 0)
            {
                foreach (CombatFella friend in friends)
                {
                    Vector3 friendToHere = transform.position - friend.transform.position;
                    _movementDir += friendToHere.normalized * Mathf.Max(0, (9 - friendToHere.sqrMagnitude) / 9);
                }
            }

            _speedModifier = _movementDir.sqrMagnitude * 10f;

            // Combat
            if(_timeSinceAttack > _attackWaitTime)
            {
                _stance = FightStance.Offensive;
                CallOthersToAttack();
                _attackWaitTime = 4f + Random.value * 3f;
                _timeSinceAttack = 0f;
            }
            _timeSinceAttack += Time.deltaTime;

            if (_playerInventory.Weapon.Attacking == true && toPlayerDist < _playerInventory.Weapon.WeaponStats.ThreatRange+0.6f)
            {
                if (!_defensiveReactionDecided)
                {
                    _defensiveReactionDecided = true;
                    if (Random.value < _parryChance) _willParry = true;
                    else if (Random.value < _parryChance)
                    {
                        _decidedToBlock = true;
                        StopBlockingAfterDelay(_playerInventory.Weapon.SwingTime);
                    }
                }
                if (_willParry && _inventory.Weapon.Parriable)
                {
                    _decidedToParry = true;
                    SwitchStanceNextFrame();
                }
            }
            else _defensiveReactionDecided = false;

            if(_stance == FightStance.Offensive)
            {
                if(toPlayerDist > playerDistGoal+1f)
                {
                    if(_timeSinceAttack > _attackWaitTime-3)
                    {
                        _stance = FightStance.Defensive;
                    }
                    DashAfterDelay(0.1f, toPlayerDir);
                }
                else if (!_inventory.Weapon.Attacking && !_inventory.Weapon.Recovering)
                {
                    if (_attacksInRow < 1+Random.value*3.5f)
                    {
                        if (toPlayerDist > playerDistGoal+0.5f)
                            StartCoroutine(DashAfterDelay(0.3f * _inventory.Weapon.SwingTime, toPlayerDir));
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
            else if (_stance == FightStance.Defensive)
            {
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
                return true;
            }
            return false;
        }

        public IEnumerator CalledToAttack()
        {
            _timeSinceAttack = 0f;
            if (Random.value < _chanceToFollowOthers)
            {
                yield return new WaitForSeconds(0.7f + 1.2f*Random.value);
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

        private IEnumerator DashAfterDelay(float delay, Vector3 dir)
        {
            yield return new WaitForSeconds(delay);
            Dash(dir);
        }

        private IEnumerator StopBlockingAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            _decidedToBlock= false;
            _defensiveReactionDecided= false;
        }
    }
}
