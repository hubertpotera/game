using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class AIArcherBasic : AIBasic
    {
        protected bool _drawing = false;

        protected override void AdditionalAwake()
        {
            base.AdditionalAwake();

            Inventory.Arrows = 5;
        }

        protected override void MovementAdjustements(Vector3 toPlayerDir)
        {
            // Strafe to avoid things when equipped with bow
            if(BowEquipped)
            {
                float turnDir = Mathf.Sign(Mathf.PerlinNoise1D(Time.time * 0.01f)*2-1);
                MovementDir = Quaternion.Euler(0f, turnDir*60f, 0f) * toPlayerDir;
                Debug.DrawRay(transform.position, 2*MovementDir.normalized, Color.red);
            }
        }

        protected override float GetDesiredPlayerDist()
        {
            if (BowEquipped)
            {
                return Inventory.Weapon.ItemStats.ThreatRange - 0.25f;
            }
            return base.GetDesiredPlayerDist();
        }

        protected override void AfterActions(float toPlayerDist)
        {
            // Dequip bow if out of arrows or the player is close
            if(BowEquipped && (Inventory.Arrows == 0 || toPlayerDist < 0.5f*Inventory.InHands.ThreatRange))
            {
                _decidedToSwitchWeapons = true;
            }
        }

        protected override void OffensiveActions(bool clearLineToPlayer, float toPlayerDist, float playerDistGoal, Vector3 toPlayerDir)
        {
            if(BowEquipped)
            {
                if(!_drawing)
                {
                    StartCoroutine(DrawBow());
                }
                Stance = FightStance.Defensive;
            }
            else
            {
                base.OffensiveActions(clearLineToPlayer, toPlayerDist, playerDistGoal, toPlayerDir);
            }
        }

        private IEnumerator DrawBow()
        {
            _decidedToAttack = true;
            _drawing = true;
            yield return new WaitForSeconds(Inventory.Weapon.SwingTime + 0.1f);
            _decidedToLetGoAttack = true;
            yield return new WaitForSeconds(Inventory.Weapon.AttackRecoveryTime + 0.1f);
            _drawing = false;
        }

        public override IEnumerator CalledToAttack(Vector3 callerPos)
        {
            if(BowEquipped) 
                yield break;
            base.CalledToAttack(callerPos);
        }
    }
}
