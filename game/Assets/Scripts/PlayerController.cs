using UnityEngine;

namespace Game
{
    public class PlayerController : MonoBehaviour
    {
        // ---------- VARIABLES ----------
        #region variables

        [SerializeField]
        private float _speed = 5f;
        [SerializeField]
        private float _acceleraction = 5f;

        private CharacterController _characterController;

        private Vector3 _movementInput;
        private Vector3 _movementReal;

        #endregion





        // ------------ INIT -------------
        #region init

        private void Awake() 
        {
            _characterController = GetComponent<CharacterController>();
        }

        #endregion





        // ------------ LOOPS ------------
        #region loops

        private void Update() 
        {
            UpdateInputs();
            Movement();
        }

        #endregion





        // ----------- METHODS -----------
        #region  methods

        private void UpdateInputs()
        {
            _movementInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;
        }

        private void Movement()
        {
            _movementReal = Vector3.Lerp(_movementReal, _movementInput, _acceleraction * Time.deltaTime);
            _characterController.Move(_speed * _movementReal * Time.deltaTime);
        }

        #endregion
    }
}
