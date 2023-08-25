using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class Menu : MonoBehaviour
    {
        public void PlayTutorial()
        {
            SceneManager.LoadScene("Tutorial", LoadSceneMode.Single);
        }

        
        public void PlayGame()
        {
            SceneManager.LoadScene("Level1", LoadSceneMode.Single);
        }

        public void Exit()
        {
            Application.Quit();
        }
    }
}
