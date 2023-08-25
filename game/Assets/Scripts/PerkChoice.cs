using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Game
{
    public class PerkChoice : MonoBehaviour
    {
        [SerializeField]
        private PlayerController _player;

        [SerializeField]
        private TextMeshProUGUI _howManyChoices;

        private int _choicesMade = 0;

        private void Awake()
        {
            SaveData.Load();
            if(SaveData.Progress.maxBossKilled == 0 || RunManager.Instance.PerksChosen) 
            {
                Continue();
                return;
            }

            _player.BlockInputs = true;
            RunManager.Instance.UsingDashInvulnerability = false;
            RunManager.Instance.UsingRiposte = false;
            RunManager.Instance.UsingBloodRage = false;
            RunManager.Instance.UsingRampage = false;

            _howManyChoices.text = ("Choose " + SaveData.Progress.maxBossKilled);
        }

        public void DashInvulnerabilityToggled(bool val)
        {
            ChoiceMade(val);
            RunManager.Instance.UsingDashInvulnerability = val;
        }
        public void RiposteToggled(bool val)
        {
            ChoiceMade(val);
            RunManager.Instance.UsingRiposte = val;
        }
        public void BloodRageToggled(bool val)
        {
            ChoiceMade(val);
            RunManager.Instance.UsingBloodRage = val;
        }
        public void RampageToggled(bool val)
        {
            ChoiceMade(val);
            RunManager.Instance.UsingRampage = val;
        }

        private void ChoiceMade(bool val)
        {
            if(val) _choicesMade += 1;
            else _choicesMade -= 1;

            if(_choicesMade == SaveData.Progress.maxBossKilled) Continue();
        }

        public void Continue()
        {
            _player.BlockInputs = false;
            RunManager.Instance.PerksChosen = true;

            Destroy(gameObject);
        }
    }
}
