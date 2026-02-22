using ECM2;
using UnityEditor;
using UnityEngine;
using MovementFSM = Game.Player.Movement.LocomotionStateMachine;

namespace Game.Player.Movement
{
    public enum Stance
    {
        Standing,
        Crouched
    }

    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private PlayerMovementData _movementData;
        [SerializeField] private PlayerBodyData _bodyData;
        [SerializeField] private CharacterOrientation _orientation;

        private CharacterMovement _controller;

        private PlayerContext _context;
        private PlayerState _state;
        private PlayerInput _input;
        private BodyController _body;
        private LocomotionSensors _sensors;
        private MovementFSM _machine;

        public PlayerContext Conext => _context;

        private void OnGUI()
        {
            // Set text scale to 2
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * 2);
            GUI.color = Color.black;

            GUILayout.Label($"Movement State: {_machine.GetFullPath()}");
            GUILayout.Label($"Grounded: {_context.IsGrounded}");
            GUILayout.Label($"Stance: {_body.Stance}");
            GUILayout.Label($"Input: [Move {_input.Move}] [Jump: {_input.Jump}] [Crouch: {_input.Crouch}]");
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
            _body = new BodyController(_controller, _bodyData);
            _sensors = new LocomotionSensors(_controller);

            _context = new PlayerContext(
                _movementData, 
                _controller, 
                _orientation, 
                _body,
                _sensors,
                _input, 
                _state
            );

            _machine = MovementFSM.CreateDefault(_context);
        }

        private void Update()
        {
            _input.Update();
            _context.Update(); // WishDir recalculations
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;

            _sensors.Update();
            _state.UpdateTimers(dt);
            _body.Update(_input.Crouch ? Stance.Crouched : Stance.Standing);
            _machine.Process();
            _controller.Move(_state.Velocity, dt);
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
        public Stance Stance;

        // Just flags, modified from states, only for getting current state
        public bool IsSliding;
        public bool IsWallrunning;

        // Wallrun data
        public float WallRunBeginTimer;
        public bool WallrunBeginTimerDepleted => WallRunBeginTimer <= 0f;
        public bool WallrunEndTimerEnded;
        public Vector3 LastWallNormal;
        public bool WallJump; // To distinct walljumps from normal jumps
        public float WallJumpCurrentHeight;

        // Jump timers
        public float JumpBufferTimer;
        public float CoyoteTimer;

        public void UpdateTimers(float delta)
        {
            WallRunBeginTimer -= delta;
            JumpBufferTimer -= delta;
            CoyoteTimer -= delta;
        }
    }

    public class PlayerInput
    {
        public Vector2 Move;
        public bool Jump;
        public bool Sprint;
        public bool Crouch;

        public void Update()
        {
            Move = InputService.Instance.Move;
            Jump = InputService.Instance.Jump;
            Sprint = InputService.Instance.Sprint;
            Crouch = InputService.Instance.Crouch;
        }
    }

    public class PlayerContext
    {
        public readonly PlayerMovementData Data;
        public readonly CharacterMovement Controller;
        public readonly CharacterOrientation Orientation;
        public readonly BodyController Body;
        public readonly LocomotionSensors Sensors;
        public readonly PlayerInput Input;
        public readonly PlayerState State;
        public bool IsGrounded => Controller.isGrounded;
        public Vector3 WishDir => _wishDir;

        private Vector3 _wishDir; // Cached wish dir

        public PlayerContext(
            PlayerMovementData data,
            CharacterMovement controller,
            CharacterOrientation orientaiton,
            BodyController body,
            LocomotionSensors sensors,
            PlayerInput input,
            PlayerState state)
        {
            Data = data;
            Controller = controller;
            Orientation = orientaiton;
            Body = body;
            Sensors = sensors;
            Input = input;
            State = state;
        }

        public void Update()
        {
            // TODO: move from here?
            _wishDir = Orientation.Forward * Input.Move.y + Orientation.Right * Input.Move.x;
            _wishDir = Vector3.ClampMagnitude(_wishDir, 1f);

            // TODO: if you won't remove it from here I'll kill you
            if (Input.Jump)
                State.JumpBufferTimer = Data.JumpBuffer;
        }
    }
}