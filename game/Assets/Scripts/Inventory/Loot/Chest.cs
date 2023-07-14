using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Chest : MonoBehaviour
    {
        [SerializeField]
        private GameObject _chestChoicePrefab;
        [Space]

        [SerializeField]
        private List<ItemSO> _possibleContents;
        [Space]

        [SerializeField]
        private List<int> _itemQualities;
        [SerializeField]
        private List<ItemWeaponSO.Effect> _weaponEffects;

        private PlayerController _player;
        private InventoryDisplay _invDisplay;

        [SerializeField]
        private ItemSO _loot;

        private void OnTriggerEnter(Collider other)
        {
            if(other.gameObject.TryGetComponent<PlayerController>(out var player))
            {
                _player = player;
                _player.OnInteraction += OpenChest;
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if(other.gameObject.TryGetComponent<PlayerController>(out var player))
            {
                _player.OnInteraction -= OpenChest;
                _player = null;
            }
        }

        void Awake()
        {
            GenerateLoot();
        }

        public ItemSO GenerateLoot()
        {
            _loot = Instantiate(_possibleContents[Random.Range(0, _possibleContents.Count)]);
            _loot.ItemQuality = (ItemWeaponSO.Quality)_itemQualities[Random.Range(0,_itemQualities.Count)];
            
            if(_loot.GetType() == typeof(ItemWeaponSO))
            {
                ItemWeaponSO loot = (ItemWeaponSO)_loot; 
                loot.WeaponEffect = (ItemWeaponSO.Effect)_weaponEffects[Random.Range(0,_weaponEffects.Count)];
                _loot = loot;
            }

            return _loot;
        }

        private void OpenChest()
        {
            _invDisplay = Instantiate(_chestChoicePrefab, _player.transform.position + 1.5f*Vector3.up, Quaternion.identity).GetComponentInChildren<InventoryDisplay>();
            _invDisplay.OnClose += CopyLootedFromInventory;
            _invDisplay.Player = _player;
            _invDisplay.LootedItem = _loot;
        }

        private void CopyLootedFromInventory()
        {
            _loot = _invDisplay.LootedItem;
            _invDisplay = null;
        }
    }
}
