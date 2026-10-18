using UnityEngine;
using UnityEngine.InputSystem;

namespace VARG.Input
{
    public class KeyboardMouseInput : IPlayerInput
    {
        public Vector2 Move
        {
            get
            {
                var keyboard = Keyboard.current;
                if (keyboard == null) return Vector2.zero;

                var direction = Vector2.zero;
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) direction.y += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) direction.y -= 1f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) direction.x -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) direction.x += 1f;

                return direction.sqrMagnitude > 1f ? direction.normalized : direction;
            }
        }

        public Vector2 Look
        {
            get
            {
                var mouse = Mouse.current;
                return mouse != null ? mouse.delta.ReadValue() : Vector2.zero;
            }
        }

        public bool FirePressed
        {
            get
            {
                var mouse = Mouse.current;
                return mouse != null && mouse.leftButton.wasPressedThisFrame;
            }
        }

        public bool GrabHeld
        {
            get
            {
                var mouse = Mouse.current;
                return mouse != null && mouse.leftButton.isPressed;
            }
        }

        public bool ThrowPressed
        {
            get
            {
                var mouse = Mouse.current;
                return mouse != null && mouse.rightButton.wasPressedThisFrame;
            }
        }

        public float HoldDistanceDelta
        {
            get
            {
                var mouse = Mouse.current;
                return mouse != null ? mouse.scroll.ReadValue().y : 0f;
            }
        }

        public bool InteractPressed
        {
            get
            {
                var keyboard = Keyboard.current;
                return keyboard != null && (keyboard.eKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame);
            }
        }

        public bool PausePressed
        {
            get
            {
                var keyboard = Keyboard.current;
                return keyboard != null && keyboard.escapeKey.wasPressedThisFrame;
            }
        }
    }
}
