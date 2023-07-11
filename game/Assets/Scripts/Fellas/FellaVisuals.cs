using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class FellaVisuals : MonoBehaviour
    {
        public MeshRenderer LadRenderer;

        private GameObject _hitParticles;

        private Inventory _inventory;
        
        private Material _head1Material;
        private Material _head2Material;
        private Material _body1Material;
        private Material _body2Material;

        private void Awake()
        {
            _hitParticles = Resources.Load<GameObject>("FX/Damage/Hit Particles");
            UpdateBlood(1,1);
            
            _inventory = GetComponent<Inventory>();

            CrateAndSetArmorDisplays();
        }

        public void HitEffects(Vector3 dir, int health, int maxHealth)
        {
            UpdateBlood(health, maxHealth);

            // Blood particles
            float angle = Mathf.Atan2(dir.x,dir.z) * Mathf.Rad2Deg;
            GameObject go = Instantiate(_hitParticles, transform.position, Quaternion.Euler(0f, angle, 0f));
            Destroy(go, 2);
        }

        public void UpdateBlood(int health, int maxHealth)
        {
            // Damage display
            float hpPercent = ((float)health)/maxHealth;
            
            transform.GetChild(0).GetChild(0).gameObject.SetActive(hpPercent<0.9f);
            transform.GetChild(0).GetChild(1).gameObject.SetActive(hpPercent<0.7f);
            transform.GetChild(0).GetChild(2).gameObject.SetActive(hpPercent<0.4f);
            transform.GetChild(0).GetChild(3).gameObject.SetActive(hpPercent<0.2f);
        }

        

        public void UpdateDisplays()
        {
            if(_inventory.Head1 != null)
            {
                _head1Material.mainTexture = _inventory.Head1.Texture;
                _head1Material.color = new Color(1,1,1,1);
            }
            else _head1Material.color = Color.clear;
            if(_inventory.Head2 != null)
            {
                _head2Material.mainTexture = _inventory.Head2.Texture;
                _head2Material.color = new Color(1,1,1,1);
            }
            else _head2Material.color = Color.clear;
            if(_inventory.Body1 != null)
            {
                _body1Material.mainTexture = _inventory.Body1.Texture;
                _body1Material.color = new Color(1,1,1,1);
            }
            else _body1Material.color = Color.clear;
            if(_inventory.Body2 != null)
            {
                _body2Material.mainTexture = _inventory.Body2.Texture;
                _body2Material.color = new Color(1,1,1,1);
            }
            else _body2Material.color = Color.clear;
        }

        private void CrateAndSetArmorDisplays()
        {
            _head1Material = CreateArmorDisplay(.03f, "head1", out GameObject go1);
            _head2Material = CreateArmorDisplay(.04f, "head2", out GameObject go2);
            _body1Material = CreateArmorDisplay(.01f, "body1", out GameObject go3);
            _body2Material = CreateArmorDisplay(.02f, "body2", out GameObject go4);
            go1.transform.parent = LadRenderer.transform;
            go2.transform.parent = LadRenderer.transform;
            go3.transform.parent = LadRenderer.transform;
            go4.transform.parent = LadRenderer.transform;
            UpdateDisplays();
            return;
        }

        private Material CreateArmorDisplay(float height, string name, out GameObject createdDisplay)
        {
            createdDisplay = Instantiate(LadRenderer.gameObject, LadRenderer.transform.position + Vector3.up * height, Quaternion.Euler(90f, 0f, 0f), transform);
            createdDisplay.name = name;
            Material mat = createdDisplay.GetComponent<MeshRenderer>().material;

            return mat;
        }
    }
}
