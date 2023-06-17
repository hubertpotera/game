using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class RandomizeTree : MonoBehaviour
    {
        [SerializeField]
        private List<Transform> _layers;

        void Awake()
        {
            Vector3 offset = new Vector3((Random.value*2-1)*0.1f, 0f, (Random.value*2-1)*0.1f);
            foreach (var layer in _layers)
            {
                layer.transform.rotation = Quaternion.Euler(90f, Random.value*360f, 0f);
                layer.transform.position += offset;
            }
        }
    }
}
