using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ActivePocketSand : EffectItem
    {
        [SerializeField]
        private GameObject _particlesGO;

        private List<AIBasic> _targets = new List<AIBasic>();

        private void OnTriggerEnter(Collider other)
        {
            AIBasic targetFella = other.gameObject.GetComponent<AIBasic>();
            if (targetFella != null && !_targets.Contains(targetFella))
            {
                _targets.Add(targetFella);
            }
        }
        private void OnTriggerExit(Collider other)
        {
            AIBasic targetFella = other.gameObject.GetComponent<AIBasic>();
            if (targetFella != null)
            {
                _targets.Remove(targetFella);
            }
        }

        public void Update()
        {
            transform.rotation = Quaternion.Euler(0f, _holder.LookRot, 0f);
        }

        public override bool Use()
        {
            _particlesGO.SetActive(true);
            SoundManager.Instance.PlayEffect(SoundManager.Instance.AudioEffects.Step[0]);

            foreach (var target in _targets)
            {
                float time = 2f + 0.5f*(int)_item.ItemQuality;
                time /= target.MaxHealth/15f;
                target.BlockActionsForTime(time);
            }

            Destroy(gameObject, 1f);

            return true;
        }
    }
}
