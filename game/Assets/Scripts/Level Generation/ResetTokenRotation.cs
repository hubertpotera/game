using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ResetTokenRotation : MonoBehaviour
    {
        void Awake()
        {
            StartCoroutine(NextFrame());
        }

        private IEnumerator NextFrame()
        {
            yield return null;
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            this.enabled = false;
        }
    }
}
