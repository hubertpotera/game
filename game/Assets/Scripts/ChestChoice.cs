using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ChestChoice : MonoBehaviour
    {
        public ItemWeaponSO Looted;
        public PlayerController Player;

        [SerializeField]
        private MeshRenderer _lootedRenderer;
        [SerializeField]
        private MeshRenderer _hand1Renderer;
        [SerializeField]
        private MeshRenderer _hand2Renderer;
        [Space]
        [SerializeField]
        private Collider _hand1Collider;
        [SerializeField]
        private Collider _hand2Collider;


        void Awake()
        {
            StartCoroutine(NextFrame());
        }

        private IEnumerator NextFrame()
        {
            yield return null;
            Player.BlockInputs = true;
            
            UpdateDiplay();
        }

        void Update()
        {
            //TODO make seperate menu inputs???
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                Player.BlockInputs = false;
                Player.Inventory.EquipWeapon(Player.Inventory.InHands);
                Destroy(gameObject);
            }

            if (Input.GetMouseButtonDown(0))
                CheckActions();
        }

        private void UpdateDiplay()
        {
            _lootedRenderer.material.mainTexture = Looted.Texture;
            _hand1Renderer.material.mainTexture = Player.Inventory.InHands.Texture;
            if(Player.Inventory.OnSide != null)
                _hand2Renderer.material.mainTexture = Player.Inventory.OnSide.Texture;
            else
                _hand2Renderer.material.color = new Color(0f, 0f, 0f, 0f);
        }

        private void CheckActions()
        {
            // What to do after LMB pressed

            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if(_hand1Collider.Raycast(mouseRay, out hit, Mathf.Infinity))
            {
                ItemWeaponSO temp = Looted;
                Looted = Player.Inventory.InHands;
                Player.Inventory.InHands = temp;
            }
            
            if(_hand2Collider.Raycast(mouseRay, out hit, Mathf.Infinity) && Player.Inventory.OnSide != null)
            {
                ItemWeaponSO temp = Looted;
                Looted = Player.Inventory.OnSide;
                Player.Inventory.OnSide = temp;
            }

            UpdateDiplay();
        }
    }
}
