using Game.Movement;
using UnityEngine;

namespace Game.Input
{
    [DefaultExecutionOrder(-94)]
    public class PlayerLocomotionInputSource : MonoBehaviour, ILocomotionInputSource
    {
        [SerializeField] private LocomotionController _target;

        public Vector2 Move => _inputService.Move;
        public bool JumpPressed => _inputService.JumpPressed;
        public bool SprintHeld => _inputService.Sprint;
        public bool CrouchHeld => _inputService.Crouch;
        public bool JumpHeld => _inputService.JumpHeld;

        private InputService _inputService;

        private void Awake()
        {
            _inputService = InputService.Instance;
        }

        private void Start()
        {
            _target.Initialize(this);
        }
    }
}