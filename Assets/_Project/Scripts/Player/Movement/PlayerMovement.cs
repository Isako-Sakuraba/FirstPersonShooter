using ECM2;
using Game.Movement;
using ImprovedTimers;
using UnityEngine;
//using MovementFSM = Game.Player.Movement.LocomotionStateMachine;
using UnityEngine.Splines;

namespace Game.Player.Movement
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private PlayerMovementData _movementData;
        //[SerializeField] private BodyConfig _bodyData;
        [SerializeField] private CharacterOrientation _orientation;

        private CharacterMovement _controller;

        private PlayerContext _context;
        private PlayerState _state;
        private PlayerInput _input;
        //private BodyController _body;
        private LocomotionSensors _sensors;
        //private MovementFSM _machine;

        public PlayerContext Conext => _context;

        private void OnGUI()
        {
            // Set text scale to 2
            //GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * 2);
            //GUI.color = Color.black;

            //GUILayout.Label($"Movement State: {_machine.GetFullPath()}");
            //GUILayout.Label($"Grounded: {_attachable.IsGrounded}");
            //GUILayout.Label($"Stance: {_body.Stance}");
            //GUILayout.Label($"Jumps left: {_state.JumpsLeft}");
            //GUILayout.Label($"JB: {_input.JumpBufferTimer.CurrentTime} | CT: {_state.CoyoteTimer.CurrentTime} | WBT: {!_state.WallrunBeginTimer.IsRunning}/{_state.WallrunBeginTimer.CurrentTime}");
            //GUILayout.Label($"Input: [Move {_input.Move}] [Jump: {_input.Jump}] [Crouch: {_input.Crouch}]");
            //GUILayout.Label($"Velocity Vector: {_attachable.State.Velocity}");

            //var horizontal = new Vector2(_attachable.State.Velocity.x, _attachable.State.Velocity.z);
            //GUILayout.Label($"Speed: {horizontal.magnitude} m/s");
            //GUILayout.Label($"Vertical speed: {_attachable.State.Velocity.y} m/s");
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
            //_body = new BodyController(_controller, _bodyData);
            _sensors = new LocomotionSensors(_controller);

            _context = new PlayerContext(
                _movementData,
                _controller,
                _orientation,
                //_body,
                _sensors,
                _input,
                _state
            );

            //_machine = MovementFSM.CreateDefault(_attachable);
        }

        private void Update()
        {
            //_input.Update();
            //_attachable.Update(); // WishDir recalculations
        }

        private void FixedUpdate()
        {
            //float dt = Time.fixedDeltaTime;

            //_sensors.Update();
            ////_body.Update(_input.Crouch ? Stance.Crouched : Stance.Standing);
            //var velocity = _state.Velocity;
            //_machine.Process();
            //_controller.Move(_state.Velocity, dt);
            //_state.Velocity = _controller.velocity;
            //_state.PreviousVelocity = velocity;
        }
    }

    public class PlayerState
    {
        public Vector3 Velocity;
        public Vector3 PreviousVelocity;

        // Just flags, modified from states, only for getting current state
        public bool IsSliding;
        public bool IsWallrunning;

        // Wallrun data
        public Vector3 LastWallNormal;
        public bool WallJump; // To distinct walljumps from normal jumps
        public float WallJumpCurrentHeight;
        public CountdownTimer WallrunBeginTimer;
        public CountdownTimer WallrunEndTimer;

        // Jump timers
        public CountdownTimer CoyoteTimer;

        // Railgrinding
        public bool IsAttached;
        public SplineContainer RailSplineContainer;
        public float CurrentT;

        // Jump amount
        public int JumpsLeft;
    }

    public class PlayerInput
    {
        public Vector2 Move;
        public bool Jump;
        public bool Sprint;
        public bool Crouch;

        public CountdownTimer JumpBufferTimer = new(0.2f);

        public void Update()
        {
            Move = InputService.Instance.Move;
            Jump = InputService.Instance.Jump;
            Sprint = InputService.Instance.Sprint;
            Crouch = InputService.Instance.Crouch;

            if (Jump)
                JumpBufferTimer.Start();
        }

        public void ConsumeJumpBuffer()
        {
            JumpBufferTimer.Cancel();
        }
    }

    public class PlayerContext
    {
        public readonly PlayerMovementData Data;
        public readonly CharacterMovement Controller;
        public readonly CharacterOrientation Orientation;
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
            LocomotionSensors sensors,
            PlayerInput input,
            PlayerState state)
        {
            Data = data;
            Controller = controller;
            Orientation = orientaiton;
            //Body = body;
            Sensors = sensors;
            Input = input;
            State = state;

            // Init timers
            State.WallrunBeginTimer = new CountdownTimer(Data.WallrunAgainTimer);
            State.WallrunEndTimer = new CountdownTimer(Data.WallrunDuration);
            State.CoyoteTimer = new CountdownTimer(Data.CoyoteTime);

            State.CoyoteTimer.OnTimerFinished += () => State.JumpsLeft--;
        }

        public void Update()
        {
            // TODO: move from here?
            _wishDir = Orientation.Forward * Input.Move.y + Orientation.Right * Input.Move.x;
            _wishDir = Vector3.ClampMagnitude(_wishDir, 1f);
        }
    }
}