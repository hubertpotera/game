using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

namespace Game
{
    public class BasicMeleeWeapon : Weapon
    {
        private void OnTriggerEnter(Collider other)
        {
            CombatFella targetFella = other.gameObject.GetComponent<CombatFella>();
            if (targetFella != null)
            {
                Targets.Add(targetFella);
            }
        }
        private void OnTriggerExit(Collider other)
        {
            CombatFella targetFella = other.gameObject.GetComponent<CombatFella>();
            if (targetFella != null)
            {
                Targets.Remove(targetFella);
            }
        }
    }
}
