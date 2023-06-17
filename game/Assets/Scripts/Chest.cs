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
        private List<ItemWeaponSO> _possibleContents;
        [Space]

        [SerializeField]
        private int _minWeaponQuality=1;
        [SerializeField]
        private int _maxWeaponQuality=3;
        [SerializeField]
        private List<ItemWeaponSO.Effect> _effects;

        private PlayerController _player;

        [SerializeField]
        private ItemWeaponSO _loot;

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
            _loot = Instantiate(_possibleContents[Random.Range(0, _possibleContents.Count)]);
            _loot.WeaponQuality = (ItemWeaponSO.Quality)Random.Range(_minWeaponQuality,_maxWeaponQuality);
            _loot.WeaponEffect = (ItemWeaponSO.Effect)_effects[Random.Range(0,_effects.Count)];
        }

        private void OpenChest()
        {
            ChestChoice chestChoice = Instantiate(_chestChoicePrefab, transform.position + Vector3.up, Quaternion.identity).GetComponent<ChestChoice>();
            chestChoice.Player = _player;
            chestChoice.Looted = _loot;
        }
    }
}
