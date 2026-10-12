using UnityEngine;

namespace VARG.Input
{
    public interface IPlayerInput
    {
        Vector2 Move { get; }
        Vector2 Look { get; }
        bool FirePressed { get; }
        bool GrabHeld { get; }
        bool ThrowPressed { get; }
        float HoldDistanceDelta { get; }
        bool InteractPressed { get; }
        bool PausePressed { get; }
    }
}
