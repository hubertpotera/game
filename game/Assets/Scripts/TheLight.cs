using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class TheLight : MonoBehaviour
    {
        public static TheLight Instance;

        [HideInInspector]
        public Light Source;

        [SerializeField]
        private float _flickerSpeed = 0.5f;
        [SerializeField]
        private float _flickerRange = 1;

        private float _startIntensity;
        
        void Awake()
        {
            if(Instance != null)
            {
                Debug.LogError("replacing the light");
                Destroy(Instance.gameObject);
            }
            Instance = this;
            Source = GetComponent<Light>();
            _startIntensity = Source.intensity;
        }

        private void Update()
        {
            Source.intensity = _startIntensity + _flickerRange * Mathf.PerlinNoise1D(_flickerSpeed*Time.time + 123.24f);
        }
    }
}
