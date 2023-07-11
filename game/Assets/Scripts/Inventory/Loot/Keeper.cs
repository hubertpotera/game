using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Keeper : MonoBehaviour
    {
        [SerializeField] 
        private GameObject _shopPrefab;

        [Space]
        [SerializeField]
        private GameObject _chestPrefabForLoot;

        private PlayerController _player;
        private GameObject _shop;
        private Transform _prevCamTarget;

        private ItemSO _item1;
        private ItemSO _item2;
        private int _boughtItem;

        private void OnTriggerEnter(Collider other)
        {
            if(other.gameObject.TryGetComponent<PlayerController>(out var player))
            {
                _player = player;
                _player.OnInteraction += OpenShop;
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if(other.gameObject.TryGetComponent<PlayerController>(out var player))
            {
                _player.OnInteraction -= OpenShop;
                _player = null;
            }
        }

        private void Awake()
        {
            _item1 = _chestPrefabForLoot.GetComponentInChildren<Chest>().GenerateLoot();
            _item2 = _chestPrefabForLoot.GetComponentInChildren<Chest>().GenerateLoot();
            
        }

        private void OpenShop()
        {
            _player.BlockInputs = true;
            _shop = Instantiate(_shopPrefab, transform.position + 1.5f*Vector3.up, Quaternion.identity);
            Shop shop = _shop.GetComponent<Shop>();
            InventoryDisplay inventoryDisplay = _shop.GetComponent<InventoryDisplay>();

            shop.OnCloseShop += CloseShop;
            shop.Player = _player;
            inventoryDisplay.Player = _player;
            if(_boughtItem == 0)
            {
                shop.ItemShop1 = _item1;
                shop.ItemShop2 = _item2;
            }
            else if(_boughtItem == 1)
            {
                shop.ItemShop1 = _item1;
                shop.ItemShop2 = _item2;
                shop.ItemWasBought(1);
            }
            else if(_boughtItem == 2)
            {
                shop.ItemShop1 = _item2;
                shop.ItemShop2 = _item1;
                shop.ItemWasBought(2);
            }

            CameraController camController = Camera.main.GetComponent<CameraController>();
            _prevCamTarget = camController.Target;
        }

        private void CloseShop()
        {
            // Update player inventory
            _player.Inventory.ApplyItemEffects();
            _player.UpdateInventory();
            
            Shop shop = _shop.GetComponent<Shop>();
            InventoryDisplay inventoryDisplay = _shop.GetComponent<InventoryDisplay>();

            _item1 = shop.ItemShop1;
            _item2 = shop.ItemShop2;
            _boughtItem = shop.BoughtItem;

            if(_boughtItem > 0 )
            {
                _item2 = null;
                _item1 = inventoryDisplay.LootedItem;
            }
        }
    }
}
