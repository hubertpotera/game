using UnityEngine;

namespace Game
{
    public class PlayerController : CombatFella
    {
        private Camera _camera;

        private float _rmbDownTime;



        override protected void AdditionalAwake()
        {
            Type = FellaType.Player;

            _camera = Camera.main;
        }



        protected override void Decide()
        {
            _movementDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

            _decidedToAttack = Input.GetMouseButtonDown(0);
            if(Input.GetMouseButtonDown(1)) 
                _rmbDownTime= Time.time;

            if (Time.time - _rmbDownTime > 0.3f && Input.GetMouseButton(1))
                _decidedToBlock = true;
            else 
                _decidedToBlock = false;

            if (Time.time - _rmbDownTime < 0.3f && Input.GetMouseButtonUp(1))
                _decidedToParry = true;

            if (Input.GetKeyDown(KeyCode.Space)) Dash(_movementDir);

            Ray cameraRay = _camera.ScreenPointToRay(Input.mousePosition);
            Vector3 worldMousePos = Vector3.zero;
            if (new Plane(Vector3.up, transform.position.y).Raycast(cameraRay, out float d))
            {
                worldMousePos = cameraRay.GetPoint(d);
            }
            Vector3 mouseDir = worldMousePos - transform.position;
            _lookRot = Mathf.Rad2Deg * Mathf.Atan2(mouseDir.x, mouseDir.z);
        }
    }
}
