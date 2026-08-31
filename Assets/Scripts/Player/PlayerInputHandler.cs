using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ShadowFire.Player
{
    public class PlayerInputHandler : MonoBehaviour
    {
        public static PlayerInputHandler Instance { get; private set; }

        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool JumpTriggered { get; private set; }
        public bool IsCrouching { get; private set; }
        public bool IsFiring { get; private set; }
        public bool FireTriggered { get; private set; }
        public bool ReloadTriggered { get; private set; }
        public bool ScopeHeld { get; private set; }
        public int WeaponSlotRequested { get; private set; } = -1;
        public float ScrollDelta { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Update()
        {
            // Reset frame triggers
            JumpTriggered = false;
            FireTriggered = false;
            ReloadTriggered = false;
            WeaponSlotRequested = -1;

            ReadInputs();
        }

        private void ReadInputs()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            var mouse = Mouse.current;

            if (kb != null)
            {
                float x = 0;
                float y = 0;
                if (kb.wKey.isPressed) y += 1;
                if (kb.sKey.isPressed) y -= 1;
                if (kb.dKey.isPressed) x += 1;
                if (kb.aKey.isPressed) x -= 1;
                MoveInput = new Vector2(x, y).normalized;

                IsSprinting = kb.leftShiftKey.isPressed;
                if (kb.spaceKey.wasPressedThisFrame) JumpTriggered = true;
                IsCrouching = kb.leftCtrlKey.isPressed || kb.cKey.isPressed;
                if (kb.rKey.wasPressedThisFrame) ReloadTriggered = true;

                if (kb.digit1Key.wasPressedThisFrame) WeaponSlotRequested = 0;
                else if (kb.digit2Key.wasPressedThisFrame) WeaponSlotRequested = 1;
                else if (kb.digit3Key.wasPressedThisFrame) WeaponSlotRequested = 2;
                else if (kb.digit4Key.wasPressedThisFrame) WeaponSlotRequested = 3;
                else if (kb.digit5Key.wasPressedThisFrame) WeaponSlotRequested = 4;
            }

            if (mouse != null)
            {
                LookInput = mouse.delta.ReadValue();
                IsFiring = mouse.leftButton.isPressed;
                if (mouse.leftButton.wasPressedThisFrame) FireTriggered = true;
                ScopeHeld = mouse.rightButton.isPressed;
                ScrollDelta = mouse.scroll.ReadValue().y;
            }
#else
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            MoveInput = new Vector2(x, y).normalized;

            LookInput = new Vector2(Input.GetAxisRaw("Mouse X") * 10f, Input.GetAxisRaw("Mouse Y") * 10f);
            IsSprinting = Input.GetKey(KeyCode.LeftShift);
            if (Input.GetKeyDown(KeyCode.Space)) JumpTriggered = true;
            IsCrouching = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);
            if (Input.GetKeyDown(KeyCode.R)) ReloadTriggered = true;

            IsFiring = Input.GetMouseButton(0);
            if (Input.GetMouseButtonDown(0)) FireTriggered = true;
            ScopeHeld = Input.GetMouseButton(1);
            ScrollDelta = Input.mouseScrollDelta.y;

            if (Input.GetKeyDown(KeyCode.Alpha1)) WeaponSlotRequested = 0;
            else if (Input.GetKeyDown(KeyCode.Alpha2)) WeaponSlotRequested = 1;
            else if (Input.GetKeyDown(KeyCode.Alpha3)) WeaponSlotRequested = 2;
            else if (Input.GetKeyDown(KeyCode.Alpha4)) WeaponSlotRequested = 3;
            else if (Input.GetKeyDown(KeyCode.Alpha5)) WeaponSlotRequested = 4;
#endif
        }
    }
}
