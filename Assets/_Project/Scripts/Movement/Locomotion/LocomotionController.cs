using ECM2;
using Game.Data.Movement;
using Game.Movement.API;
using Game.Movement.API.Requests;
using UnityEngine;

namespace Game.Movement
{
    [RequireComponent(typeof(CharacterMovement))]
    public partial class LocomotionController : MonoBehaviour
    {
        [SerializeField] private MovementConfig _movementData;
        [SerializeField] private BodyConfig _bodyData;
        [SerializeField] private CharacterOrientation _orientation;

        private CharacterMovement _motor;

        private LocomotionContext _context;
        private LocomotionState _state;
        private LocomotionInput _input;
        private BodyController _body;
        private LocomotionSensors _sensors;

        private LocomotionStateMachine _machine;

        private ILocomotionInputSource _inputSource;

        public void Initialize(ILocomotionInputSource inputSource)
        {
            _inputSource = inputSource;
        }

        private void Awake()
        {
            _motor = GetComponent<CharacterMovement>();
            _orientation ??= GetComponent<CharacterOrientation>();

            _state = new LocomotionState();
            _input = new LocomotionInput();

            _body = new BodyController(_motor, _bodyData);
            _sensors = new LocomotionSensors(_motor);

            _context = new LocomotionContext(
                _movementData,
                _motor,
                _orientation,
                _body,
                _sensors,
                _input,
                _state
            );

            _context.InitializeTimers();

            _machine = new LocomotionStateMachine(_context);
            _machine.CreateDefault(_context);
        }

        private void Update()
        {
            if (_inputSource != null)
                _input.UpdateFrom(_inputSource);
            _context.RecalculateWishDir();
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;

            _context.Sensors.Update();
            _body.Update(_input.CrouchHeld ? Stance.Crouched : Stance.Standing);

            var oldVelocity = _state.Kinematics.Velocity;

            _machine.Process();
            _motor.Move(_state.Kinematics.Velocity, dt);
            _state.Kinematics.Velocity = _motor.velocity;

            _state.Kinematics.PreviousVelocity = oldVelocity;
        }
    }

    public partial class LocomotionController
        : IReadOnlyLocomotionController
    {
        public Vector3 Forward => _context.Orientation.Forward;
        public Vector3 Right => _context.Orientation.Right;

        public IBodyState Body => _body;

        public Vector3 Velocity => _state.Kinematics.Velocity;

        public Vector3 GroundNormal => _motor.groundNormal;

        public bool IsGrounded => _motor.isGrounded;

        public LocomotionMachineState MachineState => _machine.CurrentState;
        public bool IsSliding => _machine.CurrentState == LocomotionMachineState.Grounded && _machine.GroundedState == GroundedState.Slide;
        public bool IsWallrunning => _machine.CurrentState == LocomotionMachineState.Wall;
        public bool IsRailgrinding => _machine.CurrentState == LocomotionMachineState.Rail;

        public bool HasWallContact => _context.Sensors.WallDetected;

        public Vector3 WallNormal => _context.Sensors.WallCollision.normal;

        public string GetMachinePath()
            => _machine.GetFullPath();
    }

    public partial class LocomotionController
        : IRailAttachable
    {
        public bool IsAttachedToRail => _state.Rail.IsAttached;

        public bool TryAttachRail(in RailAttachRequest request)
        {
            if (_state.Rail.IsAttached || _state.Rail.AttachmentPayload.IsPresent)
                return false;

            _state.Rail.AttachmentPayload.Set(new RailAttachPayload(request.Spline, request.StartT));
            return true;
        }

        public void DetachRail()
        {
            if (!_state.Rail.IsAttached || _state.Rail.DetachmentPayload.IsPresent)
                return;

            _state.Rail.DetachmentPayload.Set(new RailDetachPayload());
        }
    }
}