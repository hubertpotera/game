using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class AIBoss1 : AIArcherSkilled
    {
        private bool _phase1 = true;

        protected override bool DetectPlayer(float distance, out bool warnOthers)
        {
            warnOthers = false;
            if(LevelGenerator.Instance == null) return true;
            return distance < LevelGenerator.Instance.SeeingRange;
        }

        protected override void AfterActions(float toPlayerDist)
        {
            if(_phase1) _speedControll = 0;
        }

        protected override void Parrying(float toPlayerDist)
        {
            base.Parrying(toPlayerDist);
            if(_phase1 && _defensiveReactionDecided)
            {
                _phase1 = false;
                _decidedToSwitchWeapons = true;
                _defensiveReactionDecided = true;
                _willParry = true;
                Stance = FightStance.Defensive;
            }
        }

        protected override void DefensiveActions(float toPlayerDist, float playerDistGoal, Vector3 toPlayerDir)
        {
            if(_phase1)
            {
                if(Stance == FightStance.Defensive && !_drawing)
                {
                    Stance = FightStance.Offensive;
                }
            }
            else
            {
                base.DefensiveActions(toPlayerDist, playerDistGoal, toPlayerDir);
            }
        }

        protected override void GotParriedEffect()
        {
            if(_attacksInRow > 1)
            {
                base.GotParriedEffect();
            }
        }

        public override void ChangeHealth(int change)
        {
            base.ChangeHealth(change);
            if(_phase1 && change < 0)
            {
                _phase1 = false;
                _decidedToSwitchWeapons = true;
                Stance = FightStance.Defensive;
            }
        }
    }
}
