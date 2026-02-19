using ECM2;
using UnityEngine;
using MovementFSM = Game.Player.Movement.LocomotionStateMachine;

namespace Game.Player.Movement
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private PlayerMovementData _movementData;
        [SerializeField] private CharacterOrientation _orientation;

        private CharacterMovement _controller;

        private PlayerContext _context;
        private PlayerState _state;
        private PlayerInput _input;
        private MovementFSM _machine;

        private void OnGUI()
        {
            // Set text scale to 2
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * 2);
            GUI.color = Color.black;

            GUILayout.Label($"State: {_machine.GetFullPath()}");
            GUILayout.Label($"Grounded: {_context.IsGrounded}");
            GUILayout.Label($"Velocity Vector: {_context.State.Velocity}");

            var horizontal = new Vector2(_context.State.Velocity.x, _context.State.Velocity.z);
            GUILayout.Label($"Speed: {horizontal.magnitude} m/s");
            GUILayout.Label($"Vertical speed: {_context.State.Velocity.y} m/s");
        }

        private void Awake()
        {
            _controller = GetComponent<CharacterMovement>();
            _orientation ??= GetComponent<CharacterOrientation>();

            InitializeModules();
        }

        private void InitializeModules()
        {
            _state = new PlayerState();
            _input = new PlayerInput();

            _context = new PlayerContext(
                _movementData, 
                _controller, 
                _orientation, 
                _input, 
                _state
            );

            _machine = MovementFSM.CreateDefault(_context);
        }

        private void Update()
        {
            _context.Update();
        }

        private void FixedUpdate()
        {
            _machine.Process();
            _controller.Move(_state.Velocity, Time.fixedDeltaTime);
            _state.Velocity = _controller.velocity;
        }

        public static Vector3 Accelerate(Vector3 velocity, Vector3 wishDir, float wishSpeed, float accel, float deltaTime)
        {
            if (wishSpeed <= 0f || wishDir.sqrMagnitude < 0.0001f) return velocity;

            float currentSpeed = Vector3.Dot(velocity, wishDir);
            float addSpeed = wishSpeed - currentSpeed;
            if (addSpeed <= 0f) return velocity;

            float accelSpeed = accel * wishSpeed * deltaTime;
            if (accelSpeed > addSpeed) accelSpeed = addSpeed;

            return velocity + wishDir * accelSpeed;
        }

        public static Vector3 ApplyFriction(Vector3 velocity, float friction, float stopSpeed, float deltaTime)
        {
            float speed = velocity.magnitude;
            if (speed < 0.001f) return Vector3.zero;

            float control = speed < stopSpeed ? stopSpeed : speed;
            float drop = control * friction * deltaTime;
            float newSpeed = Mathf.Max(speed - drop, 0f);

            return velocity * (newSpeed / speed);
        }
    }
    
    public class PlayerState
    {
        public Vector3 Velocity;
    }

    public class PlayerInput
    {
        public Vector2 Move;
        public bool Jump;

        public void Update()
        {
            Move = InputService.Instance.Move;
            Jump = InputService.Instance.Jump;
        }
    }

    public class PlayerContext
    {
        public readonly PlayerMovementData Data;
        public readonly CharacterMovement Controller;
        public readonly CharacterOrientation Orientation;
        public readonly PlayerInput Input;
        public readonly PlayerState State;
        public bool IsGrounded => Controller.isGrounded;
        public Vector3 WishDir => _wishDir;

        private Vector3 _wishDir; // Cached wish dir

        public PlayerContext(
            PlayerMovementData data,
            CharacterMovement controller,
            CharacterOrientation orientaiton,
            PlayerInput input, 
            PlayerState state)
        {
            Data = data;
            Controller = controller;
            Orientation = orientaiton;
            Input = input;
            State = state;
        }

        public void Update()
        {
            Input.Update();
            _wishDir = Orientation.Forward * Input.Move.y + Orientation.Right * Input.Move.x;
            _wishDir = Vector3.ClampMagnitude(_wishDir, 1f);
        }
    }
}