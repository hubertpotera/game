using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Inventory : MonoBehaviour
    {
        public ItemWeaponSO InHands;
        public ItemWeaponSO OnSide;

        public int Arrows;

        public GameObject WeaponGO { get; private set; }
        public Weapon Weapon { get; private set; }

        public ItemArmorSO Head1;
        public ItemArmorSO Head2;
        public ItemArmorSO Body1;
        public ItemArmorSO Body2;

        public int GetArmor() 
        {
            int sum = 0;
            if(Head1 != null) sum += Head1.ArmorValue;
            if(Head2 != null) sum += Head2.ArmorValue;
            if(Body1 != null) sum += Body1.ArmorValue;
            if(Body2 != null) sum += Body2.ArmorValue;
            return sum;
        } 

        private void Awake()
        {
            EquipWeapon(InHands);
        }

        public void SwapWeapons()
        {
            if(InHands == null || OnSide == null) return;
            
            ItemWeaponSO old = InHands;
            InHands = OnSide;
            OnSide = old;
            EquipWeapon(InHands);
        }

        public void EquipWeapon(ItemWeaponSO weapon)
        {
            if(weapon == null)
            {
                Destroy(WeaponGO);
                return;
            }

            Quaternion rot = Quaternion.identity;
            if(WeaponGO != null)
            {
                rot = WeaponGO.transform.rotation;
                Destroy(WeaponGO);
            }

            WeaponGO = Instantiate(weapon.WeaponPrefab, transform);
            WeaponGO.transform.position += 0.1f * Vector3.up;
            WeaponGO.transform.rotation = rot;
            Weapon = WeaponGO.GetComponent<Weapon>();
            Weapon.ItemStats = weapon;
        }
    }
}
