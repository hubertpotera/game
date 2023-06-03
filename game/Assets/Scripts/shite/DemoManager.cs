using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class DemoManager : MonoBehaviour
    {
        public GameObject Player;
        public List<GameObject> Enemies = new List<GameObject>();

        public Slider EnemySlider;

        public GameObject EnemyPrefab;

        public void Awake()
        {
            Restart();
        }

        public void Restart()
        {
            Player.SetActive(true);
            Player.transform.position = new Vector3(0f, 0f, -8.5f);
            Player.GetComponent<CombatFella>().ChangeHealth(10000);

            foreach (var enemy in Enemies)
            {
                if(enemy != null)
                    Destroy(enemy);
            }
            Enemies.Clear();
            for (int i = 0; i < EnemySlider.value; i++)
            {
                Enemies.Add(Instantiate(EnemyPrefab, new Vector3(4*(Random.value*2-1), 0f, 4*Random.value), Quaternion.identity));
            }

        }
    }
}
