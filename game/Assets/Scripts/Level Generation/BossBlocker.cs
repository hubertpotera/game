using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class BossBlocker : MonoBehaviour
    {
        [SerializeField]
        private Transform _trees;

        void OnTriggerEnter(Collider other)
        {
            if(other.TryGetComponent<PlayerController>(out PlayerController _))
                StartCoroutine(ComeUp());
        }

        private IEnumerator ComeUp()
        {
            foreach (var fella in CombatFella.AllTheFellas)
            {
                if(fella.TryGetComponent<AIBasic>(out AIBasic ai))
                {
                    ai.Stance = AIBasic.FightStance.Defensive;
                }
            }

            float elapsed = 0f;
            float time = 1f;
            float startHeight = _trees.position.y;

            SoundManager.Instance.PlayEffect(SoundManager.Instance.AudioEffects.GateRumble, 0.4f);

            while(elapsed < time)
            {
                Camera.main.GetComponent<CameraController>().InduceShake(2f);
                elapsed += Time.deltaTime;
                _trees.position = new Vector3(_trees.position.x, Mathf.Lerp(startHeight, 0f, elapsed/time), _trees.position.z);
                yield return null;
            }
            SoundManager.Instance.PlayEffect(SoundManager.Instance.AudioEffects.GateRumble);

            SoundManager.Instance.StopEffects();

            _trees.position = new Vector3(_trees.position.x, 0f, _trees.position.z);

            Destroy(this);
        }
    }
}
