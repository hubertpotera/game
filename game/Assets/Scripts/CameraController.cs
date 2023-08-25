using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class CameraController : MonoBehaviour
    {
        public Transform Target;

        private const float SHAKE_FREQUENCY = 3.56f;
        private float _stress = 0;

        private void Update()
        {
            if(Target == null) return;

            GetShake(out Vector3 shakePos, out Vector3 shakeRot);

            transform.position = Vector3.Lerp(transform.position, Target.position, Time.deltaTime*5f) + 0.01f * shakePos;
            transform.rotation = Quaternion.Lerp(transform.rotation, Target.rotation, Time.deltaTime*5f) * Quaternion.Euler(0.001f * shakeRot);
        }

        private void GetShake(out Vector3 shakePos, out Vector3 shakeRot)
        {
            _stress -= 2f*Time.deltaTime;
            _stress = Mathf.Clamp01(_stress);

            float intensity = (_stress*_stress);

            shakePos = intensity * new Vector3(
                Mathf.PerlinNoise(0, SHAKE_FREQUENCY*Time.time) * 2 - 1,
                0,
                Mathf.PerlinNoise(2, SHAKE_FREQUENCY*Time.time) * 2 - 1
            );

            shakeRot = intensity * new Vector3(
                Mathf.PerlinNoise(0, 1f+SHAKE_FREQUENCY*Time.time) * 2 - 1,
                Mathf.PerlinNoise(1, 1f+SHAKE_FREQUENCY*Time.time) * 2 - 1,
                Mathf.PerlinNoise(2, 1f+SHAKE_FREQUENCY*Time.time) * 2 - 1
            );
        }

        public void InduceShake(float stress)
        {
            _stress = Mathf.Clamp01(stress);
        }
    }
}
