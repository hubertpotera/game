using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class AITutorial : AIBasic
    {
        protected override void MovementAdjustements(Vector3 toPlayerDir)
        {
            _speedControll = 0f;
        }

        protected override void DefensiveActions(float toPlayerDist, float playerDistGoal, Vector3 toPlayerDir)
        {
            if(_timeSinceAttack > _attackWaitTime)
            {
                Stance = FightStance.Offensive;
                _attackWaitTime = 2f;
                _timeSinceAttack = 0f;
                _wantedAttacksInRow = 1;
            }
        }

        protected override bool DetectPlayer(float distance, out bool warnOthers)
        {
            warnOthers = false;
            return true;
        }

        protected override void Parrying(float toPlayerDist)
        {
            return;
        }
        protected override void GotParriedEffect()
        {
            return;
        }
        protected override void CallOthersToAttack()
        {
            return;
        }
    }
}
