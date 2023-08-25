using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class LevelGate : MonoBehaviour
    {
        [SerializeField]
        private string _destinationLevel;
        [SerializeField]
        private int _lightTemp;

        [SerializeField]
        private ParticleSystem _particles;   

        private bool _spawned = false;
        private bool _readyToEnter = false;

        private void OnTriggerEnter(Collider other)
        {
            if(_readyToEnter && other.TryGetComponent<PlayerController>(out PlayerController _))
                Entered(other.gameObject);
        }

        private void Update()
        {
            if(RunManager.Instance.BossKilled && !_spawned)
            {
                _spawned = true;
                StartCoroutine(Spawn());
            }
        }

        private IEnumerator Spawn()
        {
            float elapsed = 0f;
            float time = 2f;
            float startHeight = transform.position.y;

            _particles.Play();
            SoundManager.Instance.PlayEffect(SoundManager.Instance.AudioEffects.GateRumble);

            while(elapsed < time)
            {
                Camera.main.GetComponent<CameraController>().InduceShake(5f);
                elapsed += Time.deltaTime;
                transform.position = new Vector3(transform.position.x, Mathf.Lerp(startHeight, 0f, elapsed/time), transform.position.z);
                yield return null;
            }
            SoundManager.Instance.PlayEffect(SoundManager.Instance.AudioEffects.GateRumble);

            _particles.Stop();
            SoundManager.Instance.StopEffects();

            transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
            _readyToEnter = true;
        }

        private void Entered(GameObject player)
        {
            RunManager.Instance.BossKilled = false;
            RunManager.Instance.NextLevel(player, _destinationLevel, _lightTemp);
            Destroy(this);
        }
    }
}
