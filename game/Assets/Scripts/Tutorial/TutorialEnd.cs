using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class TutorialEnd : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if(other.gameObject.TryGetComponent<PlayerController>(out PlayerController player))
            {
                player.BlockInputs = true;
                StartCoroutine(End());
            }
        }

        private IEnumerator End()
        {
            TheLight.Instance.Source.intensity = 0;
            Destroy(LevelGenerator.Instance.gameObject);

            yield return new WaitForSeconds(1);

            //TODO change scene or smthn idk
            //and remember [] you will fall
            
            SceneManager.LoadScene("Level1", LoadSceneMode.Single);
        }
    }
}
