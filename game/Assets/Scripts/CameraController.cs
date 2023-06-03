using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class CameraController : MonoBehaviour
    {
        public Transform Target;

        private void Update()
        {
            transform.position = Vector3.Lerp(transform.position, Target.position, Time.deltaTime*5f);
            transform.rotation = Quaternion.Lerp(transform.rotation, Target.rotation, Time.deltaTime*5f);
        }
    }
}
