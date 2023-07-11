using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Game
{
    public class Shop : MonoBehaviour
    {
        public PlayerController Player;
        [SerializeField]
        private Transform _tokenSpawnPoint;


        [Space]
        [SerializeField]
        private Collider _colliderHead1;
        [SerializeField]
        private Collider _colliderHead2;
        [SerializeField]
        private Collider _colliderBody1;
        [SerializeField]
        private Collider _colliderBody2;

        [Space]
        [SerializeField]
        private TextMeshProUGUI _head1Cost;
        [SerializeField]
        private TextMeshProUGUI _head2Cost;
        [SerializeField]
        private TextMeshProUGUI _body1Cost;
        [SerializeField]
        private TextMeshProUGUI _body2Cost;

        [Space]
        [SerializeField]
        private Collider _colliderItemShop1;
        [SerializeField]
        private Collider _colliderItemShop2;

        [Space]
        [SerializeField]
        private TextMeshProUGUI _item1Cost;
        [SerializeField]
        private TextMeshProUGUI _item2Cost;

        public int BoughtItem = 0;
        [HideInInspector]
        public ItemSO ItemShop1;
        [HideInInspector]
        public ItemSO ItemShop2;


        [Space]
        [SerializeField]
        private Collider _colliderHeal;
        [SerializeField]
        private TextMeshProUGUI _healCost;
        [SerializeField]
        private TextMeshProUGUI _health;

        [Space]
        [SerializeField]
        private GameObject _slainPrefab;

        public event CloseShop OnCloseShop;
        public delegate void CloseShop();

        private List<GameObject> _fellaPile = new List<GameObject>();

        private InventoryProgressionSO _invProgression;

        private InventoryDisplay _inventoryDisplay;


        private void Awake()
        {
            _invProgression = Resources.Load<InventoryProgressionSO>("InventoryProgression");
            _inventoryDisplay = GetComponentInChildren<InventoryDisplay>();
            _inventoryDisplay.OnClose += Close;
            StartCoroutine(Setup());
        }

        private void Update()
        {
            Inputs();
        }

        private void Inputs()
        {
            if (Input.GetMouseButtonDown(0))
                CheckActions();

            if (Input.GetMouseButtonDown(1))
                ConsiderToolTip();
        }

        private IEnumerator Setup()
        {
            yield return null;
            
            _inventoryDisplay.Player = Player;
            _health.text = Player.Health.ToString() + "/" + Player.MaxHealth.ToString();
            UpdateCosts();
            SetItemShop();

            // Drop all slain fellas
            int i = 0;
            foreach(var fella in RunManager.Instance.Killed)
            {
                yield return new WaitForSeconds(0.1f);
                i += 1;
                Vector3 offset = Vector3.back * 0.3f * i + Vector3.right * 0.2f * (2*Random.value-1); 
                GameObject slain = Instantiate(_slainPrefab, _tokenSpawnPoint.position + offset, Quaternion.identity);
                _fellaPile.Add(slain);
                Texture2D tokenTex = new Texture2D(1,1);
                ImageConversion.LoadImage(tokenTex, Misc.StringToBytes(fella));
                SetTokenTexture(slain, tokenTex);
            }
        }

        private void CheckActions()
        {
            // What to do after LMB pressed

            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if(BoughtItem == 0 && _colliderItemShop1.Raycast(mouseRay, out hit, Mathf.Infinity) && SpendTokens(3))
            {
                ItemWasBought(1);
            }
            if(BoughtItem == 0 && _colliderItemShop2.Raycast(mouseRay, out hit, Mathf.Infinity) && SpendTokens(3))
            {
                ItemWasBought(2);
            }

            if(_colliderHeal != null && _colliderHeal.Raycast(mouseRay, out hit, Mathf.Infinity) && SpendTokens(1))
            {
                Player.ChangeHealth(1000);
                Destroy(_colliderHeal.gameObject);
                Destroy(_healCost.gameObject);
                _health.text = Player.Health.ToString() + "/" + Player.MaxHealth.ToString();
            }

            if(_colliderHead1.Raycast(mouseRay, out hit, Mathf.Infinity) && SpendTokens(_invProgression.Head1Upgrades[RunManager.Instance.Head1Level+1].Cost))
            {
                RunManager.Instance.Head1Level += 1;
            }
            
            if(_colliderHead2.Raycast(mouseRay, out hit, Mathf.Infinity) && SpendTokens(_invProgression.Head2Upgrades[RunManager.Instance.Head2Level+1].Cost))
            {
                RunManager.Instance.Head2Level += 1;
            }
            
            if(_colliderBody1.Raycast(mouseRay, out hit, Mathf.Infinity) && SpendTokens(_invProgression.Body1Upgrades[RunManager.Instance.Body1Level+1].Cost))
            {
                RunManager.Instance.Body1Level += 1;
            }
            
            if(_colliderBody2.Raycast(mouseRay, out hit, Mathf.Infinity) && SpendTokens(_invProgression.Body2Upgrades[RunManager.Instance.Body2Level+1].Cost))
            {
                RunManager.Instance.Body2Level += 1;
            }

            _inventoryDisplay.UpdatePlayerToken();
            UpdateCosts();
        }

        public void ItemWasBought(int item)
        {
            BoughtItem = item;
            _item1Cost.text = "";
            _item2Cost.text = "";
            if(item == 1)
            {
                _inventoryDisplay.SetLoot(ItemShop1, _colliderItemShop1.gameObject);
                _inventoryDisplay.UpdateDiplays();

                Destroy(_colliderItemShop2.gameObject);
            }
            else if(item == 2)
            {
                _inventoryDisplay.SetLoot(ItemShop2, _colliderItemShop2.gameObject);
                _inventoryDisplay.UpdateDiplays();

                Destroy(_colliderItemShop1.gameObject);
            }
        }

        private void ConsiderToolTip()
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if(ItemShop1 != null && _colliderItemShop1 != null && _colliderItemShop1.Raycast(mouseRay, out hit, Mathf.Infinity))
            {
                if(ItemShop1.GetType() == typeof(ItemWeaponSO))
                {
                    _inventoryDisplay.ShowWeaponToolTip((ItemWeaponSO)ItemShop1);
                }
                else
                {
                    _inventoryDisplay.ShowToolTip(ItemShop1.Description);
                }
            }
            else if(ItemShop2 != null && _colliderItemShop2 != null && _colliderItemShop2.Raycast(mouseRay, out hit, Mathf.Infinity))
            {
                if(ItemShop1.GetType() == typeof(ItemWeaponSO))
                {
                    _inventoryDisplay.ShowWeaponToolTip((ItemWeaponSO)ItemShop2);
                }
                else
                {
                    _inventoryDisplay.ShowToolTip(ItemShop2.Description);
                }
            } 

            if(_colliderHeal.Raycast(mouseRay, out hit, Mathf.Infinity))
            {
                _inventoryDisplay.ShowToolTip("Heal to full health");
            }

            if(_colliderHead1.Raycast(mouseRay, out hit, Mathf.Infinity))
            {
                _inventoryDisplay.ShowToolTip("Upgrade your coif");
            }
            
            if(_colliderHead2.Raycast(mouseRay, out hit, Mathf.Infinity))
            {
                _inventoryDisplay.ShowToolTip("Upgrade your helmet");
            }
            
            if(_colliderBody1.Raycast(mouseRay, out hit, Mathf.Infinity))
            {
                _inventoryDisplay.ShowToolTip("Upgrade your gamberson");
            }
            
            if(_colliderBody2.Raycast(mouseRay, out hit, Mathf.Infinity))
            {
                _inventoryDisplay.ShowToolTip("Upgrade your armour");
            }
        }

        private bool SpendTokens(int n)
        {
            if(RunManager.Instance.Killed.Count < n) return false;

            SoundManager.Instance.PlayEffect(SoundManager.Instance.AudioEffects.Coins, transform.position, 0.5f);
            
            for (int i = 0; i < n; i++)
            {
                int idx = _fellaPile.Count-1;
                Destroy(_fellaPile[idx]);
                _fellaPile.RemoveAt(idx);
                RunManager.Instance.Killed.RemoveAt(idx);
            }
            return true;
        }

        private void SetTokenTexture(GameObject token, Texture2D tex)
        {
            token.transform.GetChild(0).GetComponent<MeshRenderer>().material.mainTexture = tex;
        }

        private void SetItemShop()
        {
            if(_colliderItemShop1)
                _inventoryDisplay.SetDisplay(_colliderItemShop1.GetComponent<MeshRenderer>(), ItemShop1);
            if(_colliderItemShop2)
                _inventoryDisplay.SetDisplay(_colliderItemShop2.GetComponent<MeshRenderer>(), ItemShop2);
        }

        private void UpdateCosts()
        {
            if(RunManager.Instance.Head1Level+1 < _invProgression.Head1Upgrades.Count)
            {
                _head1Cost.text = _invProgression.Head1Upgrades[RunManager.Instance.Head1Level+1].Cost.ToString();
            }
            else
            {
                _head1Cost.text = "";
            }
            
            if(RunManager.Instance.Head2Level+1 < _invProgression.Head2Upgrades.Count)
            {
                _head2Cost.text = _invProgression.Head2Upgrades[RunManager.Instance.Head2Level+1].Cost.ToString();
            }
            else
            {
                _head2Cost.text = "";
            }
            
            if(RunManager.Instance.Body1Level+1 < _invProgression.Body1Upgrades.Count)
            {
                _body1Cost.text = _invProgression.Body1Upgrades[RunManager.Instance.Body1Level+1].Cost.ToString();
            }
            else
            {
                _body1Cost.text = "";
            }
            
            if(RunManager.Instance.Body2Level+1 < _invProgression.Body2Upgrades.Count)
            {
                _body2Cost.text = _invProgression.Body2Upgrades[RunManager.Instance.Body2Level+1].Cost.ToString();
            }
            else
            {
                _body2Cost.text = "";
            }
        }

        private void Close()
        {
            _inventoryDisplay.OnClose -= Close;

            OnCloseShop?.Invoke();
            foreach(GameObject fella in _fellaPile)
            {
                Destroy(fella);
            }
        }
    }
}
