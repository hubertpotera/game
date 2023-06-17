using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class LightFlicker : MonoBehaviour
    {
        [SerializeField]
        private float _speed = 0.5f;
        [SerializeField]
        private float _range = 1;

        private Light _light;
        private float _startIntensity;

        private void Awake()
        {
            _light = GetComponent<Light>();
            _startIntensity = _light.intensity;
        }

        private void Update()
        {
            _light.intensity = _startIntensity + _range * Mathf.PerlinNoise1D(_speed*Time.time);
        }
    }
}
