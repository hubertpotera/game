using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class PauseControl : MonoBehaviour
    {
        [SerializeField]
        private GameObject _pauseUI;

        [HideInInspector]
        public static bool Paused = false;

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape) && !InventoryDisplay.Open && _pauseUI != null)
            {
                if(Paused)
                {
                    UnPause();
                }
                else
                {
                    Pause();
                }
            }
        }

        private void Pause()
        {
            Time.timeScale = 0;
            Paused = true;

            _pauseUI.SetActive(true);
        }
        private void UnPause()
        {
            Time.timeScale = 1;
            Paused = false;

            _pauseUI.SetActive(false);
        }

        public void ToMenu()
        {
            SceneManager.LoadScene("Menu");
            Destroy(RunManager.Instance.gameObject);
            UnPause();
        }

        public void RegenerateLevel()
        {
            Debug.Log(GameObject.Find("Player"));
            GameObject player = GameObject.Find("Player");
            player.transform.position = Vector3.zero;
            player.SetActive(false);
            player.SetActive(true);

            LevelGenerator.Instance.Regenerate();
            
            UnPause();
        }
    }
}
