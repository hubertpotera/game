using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Spawner : MonoBehaviour
    {
        
        [System.Serializable]
        private struct Choice
        {
            public GameObject Prefab;
            public int Weight;
        }
        
        [SerializeField]
        private List<Choice> _choices = new List<Choice>();

        private GameObject spawned;

        private void Awake()
        {
            StartCoroutine(NextFrame());
        }

        private IEnumerator NextFrame()
        {
            yield return null;
            GameObject go = Choose(_choices);
            if(go == null) yield break;
            spawned = Instantiate(go, transform.position, Quaternion.identity);
        }

        private GameObject Choose(List<Choice> choices)
        {
            if(choices.Count == 0) return null;

            float sumWeight = 0;
            foreach(var choice in choices)
            {
                sumWeight += choice.Weight;
            }
            float roll = Random.value;
            float seen = 0;
            foreach(var choice in choices)
            {
                seen += (choice.Weight/sumWeight);
                if(seen >= roll) 
                {
                    return choice.Prefab;
                }
            }
            Debug.LogError("It shouldn't come to this");
            return choices[0].Prefab;
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawSphere(transform.position, .5f);
        }

        void OnDestroy()
        {
            Destroy(spawned);
        }
    }
}
