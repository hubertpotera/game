using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Keeper : MonoBehaviour
    {
        [SerializeField] 
        private GameObject _shopPrefab;

        private PlayerController _player;
        private GameObject _shop;
        private Transform _prevCamTarget;

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

        private void OpenShop()
        {
            _player.BlockInputs = true;
            _shop = Instantiate(_shopPrefab, transform.position + Vector3.back*10f, Quaternion.identity);
            Shop shop = _shop.GetComponent<Shop>();
            shop.OnCloseShop += CloseShop;
            CameraController camController = Camera.main.GetComponent<CameraController>();
            _prevCamTarget = camController.Target;
            camController.Target = shop.CameraTarget;
        }

        private void CloseShop()
        {
            _player.BlockInputs = false;
            Camera.main.GetComponent<CameraController>().Target = _prevCamTarget;
            Destroy(_shop);

            // Update player inventory
            _player.UpdateInventory();
        }
    }
}
