using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [DefaultExecutionOrder(-10)]
    public class SetupFellaInventory : MonoBehaviour
    {
        [SerializeField]
        private ItemWeaponSO _mainWeapon;
        [SerializeField]
        private ItemWeaponSO _sideWeapon;
        [SerializeField]
        private ItemWeaponSO.Quality _weaponQuality;
        [SerializeField]
        private ItemWeaponSO.Effect _weaponEffect;


        void Awake()
        {
            Inventory fellaInventory = GetComponent<Inventory>();

            ItemWeaponSO mainWeapon = Instantiate(_mainWeapon);
            mainWeapon.ItemQuality = _weaponQuality;
            mainWeapon.WeaponEffect = _weaponEffect;
            fellaInventory.InHands = mainWeapon;

            if(_sideWeapon != null)
            {
                ItemWeaponSO sideWeapon = Instantiate(_sideWeapon);
                sideWeapon.ItemQuality = _weaponQuality;
                sideWeapon.WeaponEffect = _weaponEffect;
                fellaInventory.OnSide = sideWeapon;
            }

            Destroy(this);
        }
    }
}
