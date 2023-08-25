using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class AIBoss2 : AIPersistant
    {
        private bool _phase1 = true;

        protected override void AfterActions(float toPlayerDist)
        {
            base.AfterActions(toPlayerDist);
            _phase1 = AllTheFellas.Count != 2;
        }

        protected override void OffensiveActions(bool clearLineToPlayer, float toPlayerDist, float playerDistGoal, Vector3 toPlayerDir)
        {
            if(_phase1)
            {
                Stance = FightStance.Defensive;
                return;
            }
            base.OffensiveActions(clearLineToPlayer, toPlayerDist, playerDistGoal, toPlayerDir);
        }

        protected override void Die()
        {
            base.Die();

            SaveData.Progress.maxBossKilled = Mathf.Max(SaveData.Progress.maxBossKilled, 2);
            SaveData.Save();

            RunManager.Instance.BossKilled = true;
        }

        protected override float GetDesiredPlayerDist()
        {
            return 1.3f*base.GetDesiredPlayerDist();
        }
    }
}
