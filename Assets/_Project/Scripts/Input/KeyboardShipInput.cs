using UnityEngine;
using UnityEngine.InputSystem;

namespace VARG.Input
{
    public class KeyboardShipInput : IShipInput
    {
        public Vector2 Steer
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

        public bool DescendPressed
        {
            get
            {
                var keyboard = Keyboard.current;
                return keyboard != null && keyboard.eKey.wasPressedThisFrame;
            }
        }

        public bool CalibratePressed => false;
    }
}
