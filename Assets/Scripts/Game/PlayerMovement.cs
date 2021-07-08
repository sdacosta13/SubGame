using UnityEngine;

namespace Game
{
    public class PlayerMovement : MonoBehaviour
    {
        private CharacterController _controller;
        private Vector3 _playerVelocity;
        private bool _groundedPlayer;
        [SerializeField]
        private float playerSpeed = 2.0f;
        [SerializeField]
        private float jumpHeight = 1.0f;
        [SerializeField]
        private float gravityValue = -9.81f;

        private void Start()
        {
            _controller = gameObject.AddComponent<CharacterController>();
        }

        void Update()
        {
            _groundedPlayer = _controller.isGrounded;
            if (_groundedPlayer && _playerVelocity.y < 0)
            {
                _playerVelocity.y = 0f;
            }

            var move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            _controller.Move(move * (Time.deltaTime * playerSpeed));

            if (move != Vector3.zero)
            {
                gameObject.transform.forward = move;
            }

            // Changes the height position of the player..
            if (Input.GetButtonDown("Jump") && _groundedPlayer)
            {
                _playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
            }

            _playerVelocity.y += gravityValue * Time.deltaTime;
            _controller.Move(_playerVelocity * Time.deltaTime);
        }
    }
}