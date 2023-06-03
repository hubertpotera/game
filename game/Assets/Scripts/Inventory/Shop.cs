using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Shop : MonoBehaviour
    {
        public Transform CameraTarget;
        [SerializeField]
        private Transform _tokenSpawnPoint;
        [SerializeField]
        private Inventory _playerRepresentationInventory;

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
        private GameObject _slainPrefab;

        public event CloseShop OnCloseShop;
        public delegate void CloseShop();

        private List<GameObject> _fellaPile = new List<GameObject>();

        private InventoryProgressionSO _invProgression;


        private void Awake()
        {
            _invProgression = Resources.Load<InventoryProgressionSO>("InventoryProgression");
            StartCoroutine(Setup());
            SaveData.Save();
        }

        private void Update()
        {
            Inputs();
        }

        private void Inputs()
        {
            if(Input.GetKeyDown(KeyCode.Escape))
                OnCloseShop?.Invoke();

            if (Input.GetMouseButtonDown(0))
                CheckActions();
                
        }

        private IEnumerator Setup()
        {
            yield return new WaitForEndOfFrame();
            // Set player representation
            UpdatePlayerToken();
            // Drop all slain fellas
            int i = 0;
            foreach(var fella in SaveData.Progress.Killed)
            {
                Debug.Log("uu");
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

            if(_colliderHead1.Raycast(mouseRay, out hit, Mathf.Infinity) && SpendTokens(_invProgression.Head1Upgrades[SaveData.Progress.Head1Level+1].Cost))
            {
                SaveData.Progress.Head1Level += 1;
            }
            
            if(_colliderHead2.Raycast(mouseRay, out hit, Mathf.Infinity) && SpendTokens(_invProgression.Head2Upgrades[SaveData.Progress.Head2Level+1].Cost))
            {
                SaveData.Progress.Head2Level += 1;
            }
            
            if(_colliderBody1.Raycast(mouseRay, out hit, Mathf.Infinity) && SpendTokens(_invProgression.Body1Upgrades[SaveData.Progress.Body1Level+1].Cost))
            {
                SaveData.Progress.Body1Level += 1;
            }
            
            if(_colliderBody2.Raycast(mouseRay, out hit, Mathf.Infinity) && SpendTokens(_invProgression.Body2Upgrades[SaveData.Progress.Body2Level+1].Cost))
            {
                SaveData.Progress.Body2Level += 1;
            }

            UpdatePlayerToken();
            SaveData.Save();
        }

        private bool SpendTokens(int n)
        {
            if(SaveData.Progress.Killed.Count < n) return false;
            
            for (int i = 0; i < n; i++)
            {
                int idx = _fellaPile.Count-1-i;
                Destroy(_fellaPile[idx]);
                _fellaPile.RemoveAt(idx);
                SaveData.Progress.Killed.RemoveAt(idx);
            }
            return true;
        }

        private void UpdatePlayerToken()
        {
            if(SaveData.Progress.Head1Level >= 0)
                _playerRepresentationInventory.Head1 = _invProgression.Head1Upgrades[SaveData.Progress.Head1Level].Item;
            if(SaveData.Progress.Head2Level >= 0)
                _playerRepresentationInventory.Head2 = _invProgression.Head2Upgrades[SaveData.Progress.Head2Level].Item;
            if(SaveData.Progress.Body1Level >= 0)
                _playerRepresentationInventory.Body1 = _invProgression.Body1Upgrades[SaveData.Progress.Body1Level].Item;
            if(SaveData.Progress.Body2Level >= 0)
                _playerRepresentationInventory.Body2 = _invProgression.Body2Upgrades[SaveData.Progress.Body2Level].Item;
            FellaVisuals visuals = _playerRepresentationInventory.GetComponent<FellaVisuals>();
            visuals.UpdateDisplays();
        }

        private void SetTokenTexture(GameObject token, Texture2D tex)
        {
            token.transform.GetChild(0).GetComponent<MeshRenderer>().material.mainTexture = tex;
        }

        private void OnDestroy()
        {
            foreach(GameObject fella in _fellaPile)
            {
                Destroy(fella);
            }
        }
    }
}
