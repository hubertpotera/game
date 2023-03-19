using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Inventory : MonoBehaviour
    {
        public ItemWeaponSO InHands;
        public ItemWeaponSO OnSide;


        public GameObject WeaponGO { get; private set; }
        public Weapon Weapon { get; private set; }

        private void Awake()
        {
            WeaponGO = Instantiate(InHands.WeaponPrefab, transform);
            Weapon = WeaponGO.GetComponent<Weapon>();
        }
    }
}
