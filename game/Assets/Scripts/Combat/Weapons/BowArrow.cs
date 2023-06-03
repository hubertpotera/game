using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class BowArrow : MonoBehaviour
    {
        [SerializeField]
        private GameObject _arrowPrefab;

        public CombatFella Holder;
        public Vector3 Direction;
        public ItemWeaponSO WeaponStats;
        public CombatAudioSO Audio;

        private float _shootTimeStart;
        private float _travelTime = 2f;
        private bool _going = true;
        private bool _setup = false;

        private void LateUpdate()
        {
            if(!_setup)
            {
                Direction = Direction.normalized;
                transform.position = new Vector3(transform.position.x, 0.2f, transform.position.z);
                transform.forward = Direction;
                transform.rotation *= Quaternion.Euler(90f,-45f,0);
                _shootTimeStart = Time.time;
                transform.GetChild(0).transform.localPosition = Vector3.zero;
                transform.GetChild(0).transform.localRotation = Quaternion.identity;
            }
            if(_going)
            {
                if(Time.time - _shootTimeStart > _travelTime)
                {
                    Destroy(gameObject);
                }
                Vector3 travel = Direction * Time.deltaTime * 30f;
                Debug.DrawRay(Vector3.up*0.3f+transform.position, travel, Color.green, 100f);
                if (Physics.Raycast(Vector3.up*0.3f+transform.position, travel, out RaycastHit hit, travel.magnitude))
                {
                    _going = false;
                    CombatFella hitFella = hit.collider.gameObject.GetComponent<CombatFella>();
                    if (hitFella != null)
                    {
                        hitFella.GetComponent<AudioSource>().PlayOneShot(Audio.Choose(Audio.BowHit));
                        transform.SetParent(hit.transform, true);
                        hitFella.TakeAHit(Holder, WeaponStats.BaseDamage);
                    }
                    else
                    {
                        StartCoroutine(DestroyAfterDelay(10f));
                    }
                    return;
                }
                transform.position += travel;
            }
        }

        public IEnumerator DestroyAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Destroy(gameObject);
        }
    }
}
