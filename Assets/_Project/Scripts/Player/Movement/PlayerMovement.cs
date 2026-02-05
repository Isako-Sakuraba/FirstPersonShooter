using UnityEngine;
using MovementFSM = Game.Player.Movement.PlayerMovementStateMachine;

namespace Game.Player.Movement
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private PlayerMovementData _movementData;

        private CharacterController _controller;

        private PlayerContext _context;
        private PlayerState _state;
        private PlayerInput _input;
        private MovementFSM _machine;

        private Collider[] _groundCache = new Collider[1];

        private void OnGUI()
        {
            GUI.Label(new Rect(0f, 0f, 500f, 20f), $"State: {_machine.GetFullPath()}");
            GUI.Label(new Rect(0f, 20f, 300f, 20f), $"Grounded: {_state.IsGrounded}");
        }

        private void Awake()
        {
            Debug.Assert(_movementData != null, "MovementData is null!");
            _controller = GetComponent<CharacterController>();

            InitializeModules();
        }

        private void InitializeModules()
        {
            _state = new PlayerState();
            _input = new PlayerInput();
            _context = new PlayerContext(_movementData, _controller, _input, _state);
            _machine = MovementFSM.CreateDefault(_context);
        }

        private void Update()
        {
            _input.UpdateInput();
        }

        private void FixedUpdate()
        {
            _state.IsGrounded = CheckGrounded();
            Debug.Log($"Jump is pressed: {_input.Jump}");
            _machine.Process();
            _controller.Move(_state.Velocity * Time.fixedDeltaTime);
        }

        private bool CheckGrounded()
        {
            return Physics.OverlapSphereNonAlloc(transform.position - Vector3.up, 0.1f, _groundCache, _movementData.GroundMask) == 1;
        }
    }
    
    public class PlayerState
    {
        public Vector3 Velocity;
        public bool IsGrounded;
    }

    public class PlayerInput
    {
        public Vector2 Move;
        public Vector3 Orientation;
        public bool Jump;

        public void UpdateInput()
        {
            Move = InputService.Instance.Move;
            Jump = InputService.Instance.Jump;
            Orientation = Vector3.forward;
        }
    }

    public class PlayerContext
    {
        public readonly PlayerMovementData Data;
        public readonly CharacterController Controller;
        public readonly PlayerInput Input;
        public readonly PlayerState State;

        public PlayerContext(
            PlayerMovementData data, 
            CharacterController controller, 
            PlayerInput input, 
            PlayerState state)
        {
            Data = data;
            Controller = controller;
            Input = input;
            State = state;
        }
    }
}