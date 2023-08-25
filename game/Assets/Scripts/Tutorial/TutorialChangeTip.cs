using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Game
{
    public class TutorialChangeTip : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _tipDisplay;

        [SerializeField]
        private string _newTip;

        private void OnTriggerEnter(Collider other)
        {
            if(other.gameObject.TryGetComponent<PlayerController>(out PlayerController _) && _tipDisplay.text != _newTip)
            {
                SoundManager.Instance.PlayEffect(SoundManager.Instance.AudioEffects.OpenInventory);
                _tipDisplay.text = _newTip;
            }
        }
    }
}
