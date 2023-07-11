using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class AIArcherSkilled : AIArcherBasic
    {
        protected override float GetLookRot(Vector3 toPlayer, float toPlayerDist, CombatFella playerFella)
        {
            if(BowEquipped)
            {
                Vector3 toPlayerPredict = toPlayer + toPlayerDist*playerFella.MovementDir*0.1f;
                LookRot = Mathf.Rad2Deg * Mathf.Atan2(toPlayerPredict.x, toPlayerPredict.z);
            }
            return base.GetLookRot(toPlayer, toPlayerDist, playerFella);
        }
    }
}
