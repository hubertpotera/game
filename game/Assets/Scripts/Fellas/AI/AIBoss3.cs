using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class AIBoss3 : AIPersistant
    {
        protected override void DefensiveActions(float toPlayerDist, float playerDistGoal, Vector3 toPlayerDir)
        {
            _wantedAttacksInRow = _parriesNeededToStop;
            base.DefensiveActions(toPlayerDist, playerDistGoal, toPlayerDir);
        }
        
        protected override void Die()
        {
            base.Die();

            SaveData.Progress.maxBossKilled = Mathf.Max(SaveData.Progress.maxBossKilled, 3);
            SaveData.Save();

            RunManager.Instance.BossKilled = true;
        }
    }
}
