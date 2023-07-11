using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [DefaultExecutionOrder(-5)]
    public class RandomiseFellasInventory : MonoBehaviour
    {
        [System.Serializable]
        private struct Choice
        {
            public ItemSO Item;
            public int Weight;
        }

        [SerializeField]
        private int _minWeaponQuality=1;
        [SerializeField]
        private int _maxWeaponQuality=3;
        [SerializeField]
        private List<ItemWeaponSO.Effect> _effects;
        [Space]
        
        [SerializeField]
        private List<Choice> _mainWeaponChoices = new List<Choice>();
        [SerializeField]
        private List<Choice> _secondaryWeaponChoices = new List<Choice>();
        [Space]
        [SerializeField]
        private List<Choice> _head1Choices = new List<Choice>();
        [SerializeField]
        private List<Choice> _head2Choices = new List<Choice>();
        [Space]
        [SerializeField]
        private List<Choice> _body1Choices = new List<Choice>();
        [SerializeField]
        private List<Choice> _body2Choices = new List<Choice>();

        private void Awake()
        {
            Inventory inventory = GetComponent<Inventory>();
            
            inventory.InHands = Instantiate((ItemWeaponSO)Choose(_mainWeaponChoices));
            inventory.InHands.ItemQuality = (ItemWeaponSO.Quality)Random.Range(_minWeaponQuality,_maxWeaponQuality+1);
            inventory.InHands.WeaponEffect = (ItemWeaponSO.Effect)_effects[Random.Range(0,_effects.Count)];


            if(_secondaryWeaponChoices.Count > 0)
            {
                inventory.OnSide = Instantiate((ItemWeaponSO)Choose(_secondaryWeaponChoices));
                inventory.OnSide.ItemQuality = (ItemWeaponSO.Quality)Random.Range(_minWeaponQuality,_maxWeaponQuality+1);
                inventory.OnSide.WeaponEffect = (ItemWeaponSO.Effect)_effects[Random.Range(0,_effects.Count)];
            }

            inventory.Head1 = (ItemArmorSO)Choose(_head1Choices);
            inventory.Head2 = (ItemArmorSO)Choose(_head2Choices);
            inventory.Body1 = (ItemArmorSO)Choose(_body1Choices);
            inventory.Body2 = (ItemArmorSO)Choose(_body2Choices);
        }

        private ItemSO Choose(List<Choice> choices)
        {
            if(choices.Count == 0) return null;

            float sumWeight = 0;
            foreach(var choice in choices)
            {
                sumWeight += choice.Weight;
            }
            float roll = Random.value;
            float seen = 0;
            foreach(var choice in choices)
            {
                seen += (choice.Weight/sumWeight);
                if(seen >= roll) 
                {
                    return choice.Item;
                }
            }
            Debug.LogError("It shouldn't come to this");
            return choices[0].Item;
        }
    }
}
