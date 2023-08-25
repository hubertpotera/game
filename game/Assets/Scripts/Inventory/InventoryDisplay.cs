using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Game
{
    public class InventoryDisplay : MonoBehaviour
    {
        public static bool Open = false;

        public PlayerController Player;

        public ItemSO LootedItem;
        public GameObject LootedItemGO;
        private Collider _lootedCollider;
        private MeshRenderer _lootedRenderer;
        [SerializeField]
        public TextMeshProUGUI HealthDisplay;
        
        [SerializeField]
        private GameObject _toolTipPrefab;
        private GameObject _toolTipGo;
        
        [SerializeField]
        private Transform _cameraTarget;
        private Transform _prevCameraTarget;

        public delegate void Close();
        public event Close OnClose;

        [SerializeField]
        private Inventory _playerRepresentationInventory;
        [SerializeField]
        private GameObject _hand1;
        private Collider _hand1Collider;
        private MeshRenderer _hand1Renderer;
        [SerializeField]
        private GameObject _hand2;
        private Collider _hand2Collider;
        private MeshRenderer _hand2Renderer;
        [SerializeField]
        private GameObject _item1;
        private Collider _item1Collider;
        private MeshRenderer _item1Renderer;
        [SerializeField]
        private GameObject _item2;
        private Collider _item2Collider;
        private MeshRenderer _item2Renderer;
        [SerializeField]
        private GameObject _item3;
        private Collider _item3Collider;
        private MeshRenderer _item3Renderer;
        [SerializeField]
        private GameObject _item4;
        private Collider _item4Collider;
        private MeshRenderer _item4Renderer;

        [Header("quality indicators")]
        [SerializeField]
        private Texture2D[] _tiers;
        [SerializeField]
        private Texture2D _sharp;
        [SerializeField]
        private Texture2D _light;
        [SerializeField]
        private Texture2D _heavy;

        private InventoryProgressionSO _invProgression;

        private Vector3 _draggedStartPosition;
        private GameObject _dragged;
        private int _draggedItemIdx;
        private bool _isWeapon;
        private bool _isLooted;


        private void Awake()
        {
            Open = true;

            _prevCameraTarget = Camera.main.GetComponent<CameraController>().Target;
            Camera.main.GetComponent<CameraController>().Target = _cameraTarget;
            SoundManager.Instance.PlayEffect(SoundManager.Instance.AudioEffects.OpenInventory, 0.2f);

            _invProgression = Resources.Load<InventoryProgressionSO>("InventoryProgression");

            _hand1Collider = _hand1.GetComponent<Collider>();
            _hand1Renderer = _hand1.GetComponent<MeshRenderer>();
            _hand2Collider = _hand2.GetComponent<Collider>();
            _hand2Renderer = _hand2.GetComponent<MeshRenderer>();
            _item1Collider = _item1.GetComponent<Collider>();
            _item1Renderer = _item1.GetComponent<MeshRenderer>();
            _item2Collider = _item2.GetComponent<Collider>();
            _item2Renderer = _item2.GetComponent<MeshRenderer>();
            _item3Collider = _item3.GetComponent<Collider>();
            _item3Renderer = _item3.GetComponent<MeshRenderer>();
            _item4Collider = _item4.GetComponent<Collider>();
            _item4Renderer = _item4.GetComponent<MeshRenderer>();

            StartCoroutine(NextFrame());
        }

        private IEnumerator NextFrame()
        {
            yield return null;

            Player.BlockInputs = true;
            if(LootedItemGO != null)
            {
                _lootedCollider = LootedItemGO.GetComponent<Collider>();
                _lootedRenderer = LootedItemGO.GetComponent<MeshRenderer>();
            }
            
            UpdateDiplays();
            UpdatePlayerToken();
        }

        public void SetLoot(ItemSO item, GameObject go)
        {
            LootedItem = item;
            LootedItemGO = go;
            _lootedCollider = LootedItemGO.GetComponent<Collider>();
            _lootedRenderer = LootedItemGO.GetComponent<MeshRenderer>();
        }

        void Update()
        {
            //TODO make seperate menu inputs???
            if(Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.E))
            {
                OnClose?.Invoke();
                Player.BlockInputs = false;
                Player.Inventory.ApplyItemEffects();
                Player.UpdateInventory();
                Player.Inventory.EquipWeapon(Player.Inventory.InHands);
                Destroy(gameObject, 0);
            }
            
            if (Input.GetMouseButtonDown(1))
                ConsiderToolTip();

            if (Input.GetMouseButtonDown(0))
                Drag();
                
            if (Input.GetMouseButtonUp(0))
                Drop();

            if(_dragged != null)
            {
                Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 worldMousePos = Vector3.zero;
                if (new Plane(Vector3.up, transform.position).Raycast(cameraRay, out float d))
                {
                    worldMousePos = cameraRay.GetPoint(d);
                }

                _dragged.transform.position = worldMousePos + 0.1f * Vector3.up;
            }
        }

        public void UpdateDiplays()
        {
            if(LootedItemGO != null)
            {
                SetDisplay(_lootedRenderer, LootedItem);
            }
            SetDisplay(_hand1Renderer, Player.Inventory.InHands);
            SetDisplay(_hand2Renderer, Player.Inventory.OnSide);
            SetDisplay(_item1Renderer, Player.Inventory.Item1);
            SetDisplay(_item2Renderer, Player.Inventory.Item2);
            SetDisplay(_item3Renderer, Player.Inventory.Item3);
            SetDisplay(_item4Renderer, Player.Inventory.Item4);
            // Update health
            HealthDisplay.text = Player.Health.ToString() + "/" + Player.MaxHealth.ToString();
        }

        public void SetDisplay(MeshRenderer meshRenderer, ItemSO item)
        {
            meshRenderer.gameObject.SetActive(true);
            if(item != null)
            {
                Texture2D texture = CombineTextures(item.Texture, _tiers[-1+(int)item.ItemQuality]);
                if(item.GetType() == typeof(ItemWeaponSO))
                {
                    if(((ItemWeaponSO)item).WeaponEffect == ItemWeaponSO.Effect.Sharp)
                    {
                        texture = CombineTextures(texture, _sharp);
                    }
                    else if(((ItemWeaponSO)item).WeaponEffect == ItemWeaponSO.Effect.Light)
                    {
                        texture = CombineTextures(texture, _light);
                    }
                    else if(((ItemWeaponSO)item).WeaponEffect == ItemWeaponSO.Effect.Heavy)
                    {
                        texture = CombineTextures(texture, _heavy);
                    }
                }
                meshRenderer.material.mainTexture = texture;
                meshRenderer.material.color = Color.white;
            }
            else
                meshRenderer.material.color = new Color(0f, 0f, 0f, 0f);
        }

        private Texture2D CombineTextures(Texture2D one, Texture2D two)
        {
            Texture2D result = new(one.width, one.height);
            
            result.LoadRawTextureData(one.GetRawTextureData());

            for (int y = 0; y < result.height; y++)
            {
                for (int x = 0; x < result.width; x++)
                {
                    if(two.GetPixel(x,y).a > 0)
                    {
                        result.SetPixel(x,y, two.GetPixel(x,y));
                    }
                }
            }

            result.Apply();
            return result;
        }
        
        public void UpdatePlayerToken()
        {
            if(RunManager.Instance.Head1Level >= 0)
                _playerRepresentationInventory.Head1 = _invProgression.Head1Upgrades[RunManager.Instance.Head1Level].Item;
            if(RunManager.Instance.Head2Level >= 0)
                _playerRepresentationInventory.Head2 = _invProgression.Head2Upgrades[RunManager.Instance.Head2Level].Item;
            if(RunManager.Instance.Body1Level >= 0)
                _playerRepresentationInventory.Body1 = _invProgression.Body1Upgrades[RunManager.Instance.Body1Level].Item;
            if(RunManager.Instance.Body2Level >= 0)
                _playerRepresentationInventory.Body2 = _invProgression.Body2Upgrades[RunManager.Instance.Body2Level].Item;
            FellaVisuals visuals = _playerRepresentationInventory.GetComponent<FellaVisuals>();
            visuals.UpdateDisplays();
        }

        private void ConsiderToolTip()
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if(LootedItemGO != null && LootedItem != null && _lootedCollider.Raycast(mouseRay, out hit, Mathf.Infinity))
            {
                if(LootedItem.GetType() == typeof(ItemWeaponSO))
                    ShowWeaponToolTip((ItemWeaponSO)LootedItem);
                else
                    ShowToolTip(LootedItem.Description);
            }
            else if(Player.Inventory.InHands != null && _hand1Collider.Raycast(mouseRay, out hit, Mathf.Infinity))
            {
                ShowWeaponToolTip((ItemWeaponSO)Player.Inventory.InHands);
            } 
            else if(Player.Inventory.OnSide != null && _hand2Collider.Raycast(mouseRay, out hit, Mathf.Infinity))
            {
                ShowWeaponToolTip((ItemWeaponSO)Player.Inventory.OnSide);
            } 
            else if(Player.Inventory.Item1 != null && _item1Collider.Raycast(mouseRay, out hit, Mathf.Infinity))
            {
                ShowToolTip(Player.Inventory.Item1.Description);
            } 
            else if(Player.Inventory.Item2 != null && _item2Collider.Raycast(mouseRay, out hit, Mathf.Infinity))
            {
                ShowToolTip(Player.Inventory.Item2.Description);
            } 
            else if(Player.Inventory.Item3 != null && _item3Collider.Raycast(mouseRay, out hit, Mathf.Infinity))
            {
                ShowToolTip(Player.Inventory.Item3.Description);
            } 
            else if(Player.Inventory.Item4 != null && _item4Collider.Raycast(mouseRay, out hit, Mathf.Infinity))
            {
                ShowToolTip(Player.Inventory.Item4.Description);
            }
            else return;
        }

        public void ShowWeaponToolTip(ItemWeaponSO weapon)
        {
            string quality = "";
            switch (weapon.ItemQuality)
            {
                case(ItemSO.Quality.Old):
                    quality = "Old ";
                    break;
                case(ItemSO.Quality.Decent):
                    quality = "Decent ";
                    break;
                case(ItemSO.Quality.Normal):
                    quality = "Normal ";
                    break;
                case(ItemSO.Quality.Quality):
                    quality = "Quality ";
                    break;
                case(ItemSO.Quality.Pristine):
                    quality = "Pristine ";
                    break;
            }

            string effect = "";
            switch (weapon.WeaponEffect)
            {
                case(ItemWeaponSO.Effect.Heavy):
                    effect = "Heavy ";
                    break;
                case(ItemWeaponSO.Effect.Light):
                    effect = "Light ";
                    break;
                case(ItemWeaponSO.Effect.Sharp):
                    effect = "Sharp ";
                    break;
            }

            ShowToolTip(quality + effect + weapon.Description);
        }

        public void ShowToolTip(string str)
        {
            if(_toolTipGo == null)
            {
                _toolTipGo = Instantiate(_toolTipPrefab, transform);
                _toolTipGo.transform.parent = Camera.main.transform;
                _toolTipGo.transform.localPosition = 3.22f*Vector3.right + -1.37f*Vector3.up + 4.11f*Vector3.forward;
            }
            SoundManager.Instance.PlayEffect(SoundManager.Instance.AudioEffects.OpenInventory, 0.1f);
            _toolTipGo.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = str;
        }

        private void Drag()
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if(LootedItemGO != null && LootedItem != null && _lootedCollider.Raycast(mouseRay, out hit, Mathf.Infinity))
            {
                _draggedStartPosition = LootedItemGO.transform.position;
                _dragged = LootedItemGO;
                _draggedItemIdx = 0;
                _isWeapon = (LootedItem.GetType() == typeof(ItemWeaponSO));
                _isLooted = true;
            }
            else if(_hand1Collider.Raycast(mouseRay, out hit, Mathf.Infinity))
            {
                _draggedStartPosition = _hand1.transform.position;
                _dragged = _hand1;
                _draggedItemIdx = 0;
                _isWeapon = true;
                _isLooted = false;
            } 
            else if(_hand2Collider.Raycast(mouseRay, out hit, Mathf.Infinity))
            {
                _draggedStartPosition = _hand2.transform.position;
                _dragged = _hand2;
                _draggedItemIdx = 1;
                _isWeapon = true;
                _isLooted = false;
            } 
            else if(_item1Collider.Raycast(mouseRay, out hit, Mathf.Infinity))
            {
                _draggedStartPosition = _item1.transform.position;
                _dragged = _item1;
                _draggedItemIdx = 0;
                _isWeapon = false;
                _isLooted = false;
            } 
            else if(_item2Collider.Raycast(mouseRay, out hit, Mathf.Infinity))
            {
                _draggedStartPosition = _item2.transform.position;
                _dragged = _item2;
                _draggedItemIdx = 1;
                _isWeapon = false;
                _isLooted = false;
            } 
            else if(_item3Collider.Raycast(mouseRay, out hit, Mathf.Infinity))
            {
                _draggedStartPosition = _item3.transform.position;
                _dragged = _item3;
                _draggedItemIdx = 2;
                _isWeapon = false;
                _isLooted = false;
            } 
            else if(_item4Collider.Raycast(mouseRay, out hit, Mathf.Infinity))
            {
                _draggedStartPosition = _item4.transform.position;
                _dragged = _item4;
                _draggedItemIdx = 3;
                _isWeapon = false;
                _isLooted = false;
            }
            else return;

            SoundManager.Instance.PlayEffect(SoundManager.Instance.AudioEffects.Parry[0], 0.05f);
        }

        private void Drop()
        {
            if(_dragged == null) return;

            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            _dragged.GetComponent<Collider>().enabled = false;

            if(_hand1Collider.Raycast(mouseRay, out hit, Mathf.Infinity) && _isWeapon)
            {
                if(!_isLooted)
                {
                    ItemWeaponSO temp = Player.Inventory.GetWeaponByIndex(_draggedItemIdx);
                    Player.Inventory.GetWeaponByIndex(_draggedItemIdx) = Player.Inventory.InHands;
                    Player.Inventory.InHands = temp;
                }
                else
                {
                    ItemWeaponSO temp = (ItemWeaponSO)LootedItem;
                    LootedItem = Player.Inventory.InHands;
                    Player.Inventory.InHands = temp;
                }
            }
            else if(_hand2Collider.Raycast(mouseRay, out hit, Mathf.Infinity) && _isWeapon)
            {
                if(!_isLooted)
                {
                    ItemWeaponSO temp = Player.Inventory.GetWeaponByIndex(_draggedItemIdx);
                    Player.Inventory.GetWeaponByIndex(_draggedItemIdx) = Player.Inventory.OnSide;
                    Player.Inventory.OnSide = temp;
                }
                else
                {
                    ItemWeaponSO temp = (ItemWeaponSO)LootedItem;
                    LootedItem = Player.Inventory.OnSide;
                    Player.Inventory.OnSide = temp;
                }
            }
            else if(_item1Collider.Raycast(mouseRay, out hit, Mathf.Infinity) && !_isWeapon)
            {
                if(!_isLooted)
                {
                    ItemEffectSO temp = Player.Inventory.GetItemByIndex(_draggedItemIdx);
                    Player.Inventory.GetItemByIndex(_draggedItemIdx) = Player.Inventory.Item1;
                    Player.Inventory.Item1 = temp;
                }
                else
                {
                    ItemEffectSO temp = (ItemEffectSO)LootedItem;
                    LootedItem = Player.Inventory.Item1;
                    Player.Inventory.Item1 = temp;
                }
            }
            else if(_item2Collider.Raycast(mouseRay, out hit, Mathf.Infinity) && !_isWeapon)
            {
                if(!_isLooted)
                {
                    ItemEffectSO temp = Player.Inventory.GetItemByIndex(_draggedItemIdx);
                    Player.Inventory.GetItemByIndex(_draggedItemIdx) = Player.Inventory.Item2;
                    Player.Inventory.Item2 = temp;
                }
                else
                {
                    ItemEffectSO temp = (ItemEffectSO)LootedItem;
                    LootedItem = Player.Inventory.Item2;
                    Player.Inventory.Item2 = temp;
                }
            }
            else if(_item3Collider.Raycast(mouseRay, out hit, Mathf.Infinity) && !_isWeapon)
            {
                if(!_isLooted)
                {
                    ItemEffectSO temp = Player.Inventory.GetItemByIndex(_draggedItemIdx);
                    Player.Inventory.GetItemByIndex(_draggedItemIdx) = Player.Inventory.Item3;
                    Player.Inventory.Item3 = temp;
                }
                else
                {
                    ItemEffectSO temp = (ItemEffectSO)LootedItem;
                    LootedItem = Player.Inventory.Item3;
                    Player.Inventory.Item3 = temp;
                }
            }
            else if(_item4Collider.Raycast(mouseRay, out hit, Mathf.Infinity) && !_isWeapon)
            {
                if(!_isLooted)
                {
                    ItemEffectSO temp = Player.Inventory.GetItemByIndex(_draggedItemIdx);
                    Player.Inventory.GetItemByIndex(_draggedItemIdx) = Player.Inventory.Item4;
                    Player.Inventory.Item4 = temp;
                }
                else
                {
                    ItemEffectSO temp = (ItemEffectSO)LootedItem;
                    LootedItem = Player.Inventory.Item4;
                    Player.Inventory.Item4 = temp;
                }
            }
            
            if(_dragged != null)
                SoundManager.Instance.PlayEffect(SoundManager.Instance.AudioEffects.Parry[1], 0.05f);
            
            _dragged.GetComponent<Collider>().enabled = true;
            _dragged.transform.position = _draggedStartPosition;
            _dragged = null;

            UpdateDiplays();
        }

        void OnDestroy()
        {
            Open = false;
            Camera.main.GetComponent<CameraController>().Target = _prevCameraTarget;
            Destroy(_toolTipGo);
        }
    }
}
