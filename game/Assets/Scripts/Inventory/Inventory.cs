using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Inventory : MonoBehaviour
    {
        public ItemWeaponSO InHands;
        public ItemWeaponSO OnSide;
        [Space]

        public int Arrows;  //TODO maybe do away with this?

        public GameObject WeaponGO { get; private set; }
        public Weapon Weapon { get; private set; }

        [Space]
        public ItemArmorSO Head1;
        public ItemArmorSO Head2;
        public ItemArmorSO Body1;
        public ItemArmorSO Body2;
        [Space]

        public ItemEffectSO Item1;
        public ItemEffectSO Item2;
        public ItemEffectSO Item3;
        public ItemEffectSO Item4;

        [HideInInspector]
        public EffectItem Item1Object;
        [HideInInspector]
        public EffectItem Item2Object;
        [HideInInspector]
        public EffectItem Item3Object;
        [HideInInspector]
        public EffectItem Item4Object;

        private Transform _itemHolder;

        public int GetArmor() 
        {
            int sum = 0;
            if(Head1 != null) sum += Head1.ArmorValue;
            if(Head2 != null) sum += Head2.ArmorValue;
            if(Body1 != null) sum += Body1.ArmorValue;
            if(Body2 != null) sum += Body2.ArmorValue;
            return sum;
        } 

        public void ApplyItemEffects()
        {
            if(_itemHolder == null) 
            {
                _itemHolder = new GameObject("item holder").transform;
                _itemHolder.transform.parent = transform;
            }

            CombatFella fella = GetComponent<CombatFella>();
            fella.ItemHealthMod = 1f;
            fella.ItemSpeedMod = 1f;
            fella.ItemDashDistMod = 1f;
            fella.ItemDashCooldownMod = 1f;
            fella.ItemAttackSpeedMod = 1f;
            ClearItems();
            Item1Object = InitializeItem(fella, Item1);
            Item2Object = InitializeItem(fella, Item2);
            Item3Object = InitializeItem(fella, Item3);
            Item4Object = InitializeItem(fella, Item4);
            if(Weapon != null)
                Weapon.UpdateParameters();
            fella.ChangeHealth(0);
        }

        private void ClearItems()
        {
            for (int i = 0; i < _itemHolder.childCount; i++)
            {
                Destroy(_itemHolder.GetChild(i).gameObject);
            }
        }

        private EffectItem InitializeItem(CombatFella holder, ItemEffectSO item)
        {
            if(item == null) return null;
            Debug.Log("init");
            EffectItem o = Instantiate(item.PrefabWithEffect, _itemHolder).GetComponent<EffectItem>();
            o.Initialize(holder, item);
            return o;
        }

        private void Awake()
        {
            EquipWeapon(InHands);
        }

        public void SwapWeapons()
        {
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

        public void DestroyHolders()
        {
            Destroy(WeaponGO); 
            Destroy(_itemHolder.gameObject); 
        }

        public ref ItemWeaponSO GetWeaponByIndex(int idx)
        {
            switch (idx)
            {
                case 0:
                    return ref InHands;
                case 1:
                    return ref OnSide;
            }

            Debug.LogError("wrong item idx");
            return ref InHands;
        }
        
        public ref ItemEffectSO GetItemByIndex(int idx)
        {
            switch (idx)
            {
                case 0:
                    return ref Item1;
                case 1:
                    return ref Item2;
                case 2:
                    return ref Item3;
                case 3:
                    return ref Item4;
            }

            Debug.LogError("wrong item idx");
            return ref Item1;
        }
    }
}
