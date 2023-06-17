using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [DefaultExecutionOrder(-10)]
    public class SetupPlayerStart : MonoBehaviour
    {
        [SerializeField]
        private ItemWeaponSO _weapon;
        [SerializeField]
        private ItemWeaponSO.Quality _weaponQuality;
        [SerializeField]
        private ItemWeaponSO.Effect _weaponEffect;


        void Awake()
        {
            ItemWeaponSO weapon = Instantiate(_weapon);
            weapon.WeaponQuality = _weaponQuality;
            weapon.WeaponEffect = _weaponEffect;

            Inventory playerInventory = GetComponent<Inventory>();

            playerInventory.InHands = weapon;

            this.enabled = false;
        }
    }
}
