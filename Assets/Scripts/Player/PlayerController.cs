using UnityEngine;
using ShadowFire.Core;
using ShadowFire.Audio;
using ShadowFire.Managers;

namespace ShadowFire.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        [Header("Movement Speeds")]
        [SerializeField] private float walkSpeed = 6.0f;
        [SerializeField] private float sprintSpeed = 10.5f;
        [SerializeField] private float crouchSpeed = 3.2f;
        [SerializeField] private float jumpHeight = 1.6f;
        [SerializeField] private float gravity = -22f;

        [Header("Look Settings")]
        public float MouseSensitivity = 1.8f;
        [SerializeField] private float lookXLimit = 85.0f;

        [Header("Camera & Head Bob")]
        [SerializeField] private Transform playerCameraTransform;
        [SerializeField] private float standingHeight = 2.0f;
        [SerializeField] private float crouchHeight = 1.1f;
        [SerializeField] private float crouchTransitionSpeed = 10f;
        [SerializeField] private float bobFrequency = 12f;
        [SerializeField] private float bobAmount = 0.05f;

        private CharacterController _characterController;
        private PlayerStats _stats;
        private PlayerInputHandler _input;

        private Vector3 _velocity;
        private float _rotationX = 0f;
        private float _targetHeight;
        private Vector3 _camInitialLocalPos;
        private float _bobTimer = 0f;
        private float _footstepTimer = 0f;
        [SerializeField] private float footstepInterval = 0.45f;

        public Transform CameraTransform => playerCameraTransform;
        public bool IsGrounded => _characterController.isGrounded;
        public bool IsMoving => _input != null && _input.MoveInput.sqrMagnitude > 0.01f;
        public bool IsSprinting { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);

            _characterController = GetComponent<CharacterController>();
            _stats = GetComponent<PlayerStats>();
            _targetHeight = standingHeight;
        }

        private void Start()
        {
            _input = PlayerInputHandler.Instance;
            if (playerCameraTransform != null)
            {
                _camInitialLocalPos = playerCameraTransform.localPosition;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (_stats != null && !_stats.IsAlive) return;
            if (_input == null) return;
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.InGame && GameManager.Instance.State != GameState.WaveCountdown) return;

            HandleLook();
            HandleMovement();
            HandleCrouch();
            HandleHeadBob();
        }

        private void HandleLook()
        {
            if (playerCameraTransform == null) return;

            Vector2 look = _input.LookInput * MouseSensitivity * 0.1f;

            _rotationX -= look.y;
            _rotationX = Mathf.Clamp(_rotationX, -lookXLimit, lookXLimit);
            playerCameraTransform.localRotation = Quaternion.Euler(_rotationX, 0, 0);

            transform.Rotate(Vector3.up * look.x);
        }

        private void HandleMovement()
        {
            bool grounded = _characterController.isGrounded;
            if (grounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }

            // Determine speed
            float targetSpeed = walkSpeed;
            IsSprinting = false;

            if (_input.IsCrouching)
            {
                targetSpeed = crouchSpeed;
            }
            else if (_input.IsSprinting && _input.MoveInput.y > 0)
            {
                float staminaCost = 15f * Time.deltaTime;
                if (_stats != null && _stats.ConsumeStamina(staminaCost))
                {
                    targetSpeed = sprintSpeed * (_stats != null ? _stats.SprintMultiplier : 1.0f);
                    IsSprinting = true;
                }
            }

            Vector3 move = transform.right * _input.MoveInput.x + transform.forward * _input.MoveInput.y;
            _characterController.Move(move * (targetSpeed * Time.deltaTime));

            // Jump
            if (_input.JumpTriggered && grounded && !_input.IsCrouching)
            {
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                if (AudioManager.Instance != null) AudioManager.Instance.PlayJump();
            }

            // Gravity
            _velocity.y += gravity * Time.deltaTime;
            _characterController.Move(_velocity * Time.deltaTime);

            // Footstep sounds
            if (grounded && move.sqrMagnitude > 0.05f)
            {
                float stepSpeed = IsSprinting ? footstepInterval * 0.65f : footstepInterval;
                _footstepTimer += Time.deltaTime;
                if (_footstepTimer >= stepSpeed)
                {
                    _footstepTimer = 0f;
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayFootstep();
                }
            }
        }

        private void HandleCrouch()
        {
            _targetHeight = _input.IsCrouching ? crouchHeight : standingHeight;
            _characterController.height = Mathf.Lerp(_characterController.height, _targetHeight, Time.deltaTime * crouchTransitionSpeed);
            _characterController.center = new Vector3(0, _characterController.height / 2f, 0);
        }

        private void HandleHeadBob()
        {
            if (playerCameraTransform == null) return;

            if (IsGrounded && IsMoving)
            {
                float speedMultiplier = IsSprinting ? 1.5f : 1.0f;
                _bobTimer += Time.deltaTime * bobFrequency * speedMultiplier;
                float newY = _camInitialLocalPos.y + Mathf.Sin(_bobTimer) * bobAmount;
                float newX = _camInitialLocalPos.x + Mathf.Cos(_bobTimer * 0.5f) * bobAmount * 0.5f;
                playerCameraTransform.localPosition = new Vector3(newX, newY, _camInitialLocalPos.z);
            }
            else
            {
                _bobTimer = 0;
                playerCameraTransform.localPosition = Vector3.Lerp(playerCameraTransform.localPosition, _camInitialLocalPos, Time.deltaTime * 8f);
            }
        }

        public void SetCameraTransform(Transform cam)
        {
            playerCameraTransform = cam;
            if (cam != null) _camInitialLocalPos = cam.localPosition;
        }
    }
}
