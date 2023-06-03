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


        protected override void AdditionalAwake()
        {
            Type = FellaType.Player;

            _camera = Camera.main;

            Inventory.Arrows = 10000; //TEMP 

            UpdateInventory();
            SaveData.Save();
        }

        public void UpdateInventory()
        {
            InventoryProgressionSO invProg = Resources.Load<InventoryProgressionSO>("InventoryProgression"); 
            if(SaveData.Progress.Head1Level >= 0)
                Inventory.Head1 = invProg.Head1Upgrades[SaveData.Progress.Head1Level].Item;
            if(SaveData.Progress.Head2Level >= 0)
                Inventory.Head2 = invProg.Head2Upgrades[SaveData.Progress.Head2Level].Item;
            if(SaveData.Progress.Body1Level >= 0)
                Inventory.Body1 = invProg.Body1Upgrades[SaveData.Progress.Body1Level].Item;
            if(SaveData.Progress.Body2Level >= 0)
                Inventory.Body2 = invProg.Body2Upgrades[SaveData.Progress.Body2Level].Item;
            Visuals.UpdateDisplays();
            // TODO
            // _inventory.InHands = InHands;
            // _inventory.OnSide = OnSide;
        }


        protected override void Decide()
        {
            if(BlockInputs) return;

            MovementDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

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

            Ray cameraRay = _camera.ScreenPointToRay(Input.mousePosition);
            Vector3 worldMousePos = Vector3.zero;
            if (new Plane(Vector3.up, transform.position.y).Raycast(cameraRay, out float d))
            {
                worldMousePos = cameraRay.GetPoint(d);
            }
            Vector3 mouseDir = worldMousePos - transform.position;
            _lookRot = Mathf.Rad2Deg * Mathf.Atan2(mouseDir.x, mouseDir.z);
        }


        protected override void Killed(CombatFella killedOne)
        {
            SaveData.Progress.AddKilled(killedOne);
        }

        protected override void Die()
        {
            gameObject.SetActive(false);
        }


        protected override void Dash(Vector3 dir)
        {
            base.Dash(dir);
            if(SaveData.Progress.UsingDashInvulnerability)
            {
                StartInvulnerability(0.3f);
            }
        }

        protected override void SuccessfulParry()
        {
            base.SuccessfulParry();
            if(SaveData.Progress.UsingRiposte)
            {
                StartCoroutine(Inventory.Weapon.BoostNextAttackCoroutine(0.5f, 0.2f));
            }
        }

        public override void ChangeHealth(int change)
        {
            base.ChangeHealth(change);
            if(SaveData.Progress.UsingBloodRage)
            {
                Inventory.Weapon.DamageMod = 1.5f - 0.5f*Health/MaxHealth;
            }
        }

        protected override void Attack()
        {
            if(SaveData.Progress.UsingRampage)
            {
                if(Time.time - _lastAttackTime < 1.2f*(Inventory.Weapon.SwingTime+Inventory.Weapon.WindupTime+Inventory.Weapon.AttackRecoveryTime))
                {
                    StartCoroutine(Inventory.Weapon.BoostNextAttackCoroutine(Mathf.Pow(0.9f, _attacksInRow), 0.3f));
                    Inventory.Weapon.DamageMod = Mathf.Pow(1.05f, _attacksInRow);
                    _attacksInRow = Mathf.Min(_attacksInRow+1, 4);
                }
                else
                {
                    _attacksInRow = 0;
                    Inventory.Weapon.DamageMod = 1f;
                }
                _lastAttackTime = Time.time;
            }

            base.Attack();
        }
    }
}
