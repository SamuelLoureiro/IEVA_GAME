using UnityEngine;
using VARG.Core;

namespace VARG.Input
{
    public class InputManager : Singleton<InputManager>
    {
        private IPlayerInput _playerInput;
        private IShipInput _shipInput;

        public IPlayerInput Player
        {
            get => _playerInput ??= new KeyboardMouseInput();
            private set => _playerInput = value;
        }

        public IShipInput Ship
        {
            get => _shipInput ??= new KeyboardShipInput();
            private set => _shipInput = value;
        }

        protected override void Awake()
        {
            base.Awake();
            _playerInput ??= new KeyboardMouseInput();
            _shipInput ??= new KeyboardShipInput();
        }

        public void SetPlayerInput(IPlayerInput input)
        {
            Player = input;
        }

        public void SetShipInput(IShipInput input)
        {
            Ship = input;
        }
    }
}
