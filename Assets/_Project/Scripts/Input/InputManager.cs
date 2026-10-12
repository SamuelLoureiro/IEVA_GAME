using UnityEngine;
using VARG.Core;

namespace VARG.Input
{
    public class InputManager : Singleton<InputManager>
    {
        public IPlayerInput Player { get; protected set; }
        public IShipInput Ship { get; protected set; }
    }
}
