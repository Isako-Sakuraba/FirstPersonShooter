using UnityEngine;

// TODO: change the namespace to something more appropriate
namespace Game.Movement.Player
{
    // TODO: unnecessary abstraction layer, switch to just InputService when it is complete
    public class PlayerInputSource : ILocomotionInputSource
    {
        public Vector2 Move => _move;
        public bool JumpPressed => _jumpPressed;
        public bool SprintHeld => _sprintHeld;
        public bool CrouchHeld => _crouchHeld;

        private Vector2 _move;
        private bool _jumpPressed;
        private bool _sprintHeld;
        private bool _crouchHeld;

        private InputService _input;

        public PlayerInputSource()
        {
            _input = InputService.Instance;
        }

        public PlayerInputSource(InputService inputService)
        {
            _input = inputService;
        }

        public void UpdateFromService()
        {
            _move = _input.Move;
            _jumpPressed = _input.Jump;
            _sprintHeld = _input.Sprint;
            _crouchHeld = _input.Crouch;
        }
    }
}