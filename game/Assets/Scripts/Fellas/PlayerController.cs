using System.Collections;
using System;
using UnityEngine;

namespace Game
{
    public class PlayerController : CombatFella
    {
        private Camera _camera;

        private float _lastAttackTime;
        private int _attacksInRow;
        private float _rmbDownTime;

        public bool BlockInputs = false;

        public event Interaction OnInteraction;
        public delegate void Interaction();
        
        [SerializeField]
        public Transform CameraTarget;

        [SerializeField]
        private GameObject _inventoryPrefab;
        private GameObject _spawnedInventory;


        protected override void AdditionalAwake()
        {
            Type = FellaType.Player;

            _camera = Camera.main;
            _camera.GetComponent<CameraController>().Target = CameraTarget;

            Inventory.Arrows = 10000; //TEMP 

            UpdateInventory();
        }

        public void UpdateInventory()
        {
            InventoryProgressionSO invProg = Resources.Load<InventoryProgressionSO>("InventoryProgression");
            if(RunManager.Instance.Head1Level >= 0)
                Inventory.Head1 = invProg.Head1Upgrades[RunManager.Instance.Head1Level].Item;
            if(RunManager.Instance.Head2Level >= 0)
                Inventory.Head2 = invProg.Head2Upgrades[RunManager.Instance.Head2Level].Item;
            if(RunManager.Instance.Body1Level >= 0)
                Inventory.Body1 = invProg.Body1Upgrades[RunManager.Instance.Body1Level].Item;
            if(RunManager.Instance.Body2Level >= 0)
                Inventory.Body2 = invProg.Body2Upgrades[RunManager.Instance.Body2Level].Item;
            Visuals.Invoke("UpdateDisplays", 0);    // Invoking because in build the inventory would be null at start
        }


        protected override void Decide()
        {
            if(BlockInputs || PauseControl.Paused) 
            {
                MovementDir = Vector3.zero;
                return;
            }

            MovementDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

            if(Input.GetKeyDown(KeyCode.Alpha1))
                _decidedToUseItem1 = true;
            if(Input.GetKeyDown(KeyCode.Alpha2))
                _decidedToUseItem2 = true;
            if(Input.GetKeyDown(KeyCode.Alpha3))
                _decidedToUseItem3 = true;
            if(Input.GetKeyDown(KeyCode.Alpha4))
                _decidedToUseItem4 = true;

            if(Input.GetMouseButtonDown(0))
                _decidedToAttack = true;
            if(Input.GetMouseButtonUp(0))
                _decidedToLetGoAttack = true;

            if (Input.GetMouseButtonDown(1))
                _decidedToParry = true;

            if (Input.GetKeyDown(KeyCode.Space)) 
                Dash(MovementDir);

            if(Input.GetKeyDown(KeyCode.Q))
                _decidedToSwitchWeapons = true;

            if(Input.GetKeyDown(KeyCode.E))
                OnInteraction?.Invoke();

            if(Input.GetKeyDown(KeyCode.Tab))
                OpenInventory();

            Ray cameraRay = _camera.ScreenPointToRay(Input.mousePosition);
            Vector3 worldMousePos = Vector3.zero;
            if (new Plane(Vector3.up, transform.position.y).Raycast(cameraRay, out float d))
            {
                worldMousePos = cameraRay.GetPoint(d);
            }
            Vector3 mouseDir = worldMousePos - transform.position;
            LookRot = Mathf.Rad2Deg * Mathf.Atan2(mouseDir.x, mouseDir.z);
        }

        private void OpenInventory()
        {
            _spawnedInventory = Instantiate(_inventoryPrefab, transform.position + 1.5f*Vector3.up, Quaternion.identity);
            _spawnedInventory.GetComponent<InventoryDisplay>().Player = this;
        }


        protected override void Killed(CombatFella killedOne)
        {
            RunManager.Instance.AddKilled(killedOne);
        }

        public override bool TakeAHit(CombatFella attacker, float damage, float armourEffectiveness = 1f)
        {
            _camera.GetComponent<CameraController>().InduceShake(1);
            return base.TakeAHit(attacker, damage, armourEffectiveness);
        }

        protected override void Die()
        {
            RunManager.Instance.PlayerDeath();
        }

        protected override void Dash(Vector3 dir)
        {
            base.Dash(dir);
            _camera.GetComponent<CameraController>().InduceShake(.4f);
            if(RunManager.Instance.UsingDashInvulnerability)
            {
                StartInvulnerability(0.3f);
            }
        }

        protected override void SuccessfulParry()
        {
            base.SuccessfulParry();
            _camera.GetComponent<CameraController>().InduceShake(.1f);
            if(RunManager.Instance.UsingRiposte)
            {
                StartCoroutine(Inventory.Weapon.BoostNextAttackCoroutine(0.5f, 0.2f));
            }
        }

        public override bool GetParried()
        {
            _camera.GetComponent<CameraController>().InduceShake(.6f);
            return base.GetParried();
        }

        public override void ChangeHealth(int change)
        {
            base.ChangeHealth(change);
            if(RunManager.Instance.UsingBloodRage && Inventory.Weapon != null)
            {
                Inventory.Weapon.BloodRageDamageMod = 2f - Health/MaxHealth;
            }
        }

        protected override void SwapWeapons()
        {
            base.SwapWeapons();
            if(RunManager.Instance.UsingBloodRage)
            {
                Inventory.Weapon.BloodRageDamageMod = 2f - Health/MaxHealth;
            }
        }

        protected override void Attack()
        {
            if(RunManager.Instance.UsingRampage)
            {
                if(Time.time - _lastAttackTime < 1.2f*(Inventory.Weapon.SwingTime+Inventory.Weapon.WindupTime+Inventory.Weapon.AttackRecoveryTime))
                {
                    StartCoroutine(Inventory.Weapon.BoostNextAttackCoroutine(Mathf.Pow(0.9f, _attacksInRow), 0.3f));
                    Inventory.Weapon.RampageDamageMod = Mathf.Pow(1.05f, _attacksInRow);
                    _attacksInRow = Mathf.Min(_attacksInRow+1, 4);
                }
                else
                {
                    _attacksInRow = 0;
                    Inventory.Weapon.RampageDamageMod = 1f;
                }
                _lastAttackTime = Time.time;
            }

            base.Attack();
        }
    }
}
