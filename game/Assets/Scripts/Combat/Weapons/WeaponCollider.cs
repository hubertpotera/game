using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class WeaponCollider : MonoBehaviour
    {
        public List<CombatFella> Targets = new List<CombatFella>();

        private void OnTriggerEnter(Collider other)
        {
            CombatFella targetFella = other.gameObject.GetComponent<CombatFella>();
            if (targetFella != null && !Targets.Contains(targetFella))
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
