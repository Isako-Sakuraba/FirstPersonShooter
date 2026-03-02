using ECM2;
using Game.Data.Movement;
using Game.Movement.API;
using Game.Movement.API.Requests;
using System;
using UnityEngine;

namespace Game.Movement
{
    [RequireComponent(typeof(CharacterMovement))]
    public partial class LocomotionController : MonoBehaviour
    {
        [SerializeField] private MovementConfig _movementData;
        [SerializeField] private BodyConfig _bodyData;
        [SerializeField] private CharacterOrientation _orientation;
        [SerializeField] private Transform _pointer; //TODO: move somewhere else

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

        private void OnEnable()
        {
            _state.Grapple.OnGrappleStatusChanged += GrappleStatusChanged;
        }

        private void OnDisable()
        {
            _state.Grapple.OnGrappleStatusChanged -= GrappleStatusChanged;
        }

        private void GrappleStatusChanged(bool attached)
        {
            if (attached)
                OnGrappleAttached.Invoke();
            else
                OnGrappleDetached.Invoke(transform.position);
        }

        private void Awake()
        {
            _motor = GetComponent<CharacterMovement>();
            _orientation ??= GetComponent<CharacterOrientation>();

            _state = new LocomotionState();
            _input = new LocomotionInput();

            _body = new BodyController(_motor, _bodyData);
            _sensors = new LocomotionSensors(_motor, _movementData, _pointer);

            _context = new LocomotionContext(
                _movementData,
                _motor,
                _orientation,
                _pointer,
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

        public void Reset()
        {
            _context.State.Kinematics.Velocity = Vector3.zero;

            _machine.Reset();

            _context.State.Rail.IsAttached = false;
            _context.State.Rail.Spline = null;

            _context.Input.ConsumeJumpBuffer();
            _context.State.Jump.CoyoteTimer.Cancel();
            _context.State.Wallrun.BeginCooldown.Cancel();
            _context.State.Wallrun.DurationTimer.Cancel();

            _sensors.Update();
        }
    }

    public partial class LocomotionController : IReadOnlyLocomotionController
    {
        public Vector3 Forward => _context.Orientation.ForwardFlat;
        public Vector3 Right => _context.Orientation.RightFlat;

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

        public bool IsGrappling => _machine.CurrentState == LocomotionMachineState.Grapple;
        public Vector3 PivotWorldPoint => _state.Grapple.WorldPoint;
        public float MaxLength => _state.Grapple.MaxLength;
        public float CurrentLength => _state.Grapple.CurrentLength;

        public event Action OnGrappleAttached = delegate { };
        public event Action<Vector3> OnGrappleDetached = delegate { };

        public string GetMachinePath()
            => _machine.GetFullPath();
    }

    public partial class LocomotionController : IRailAttachable
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

    public partial class LocomotionController : IGrappleAttachable
    {
        public bool IsGrappleAttached => _state.Grapple.IsGrappled;

        public bool TryGrappleAttach(in GrappleAttachRequest request)
        {
            if (_state.Grapple.IsGrappled || _state.Grapple.AttachmentPayload.IsPresent || _context.IsGrounded)
                return false;

            _state.Grapple.AttachmentPayload.Set(new GrappleAttachPayload(request.Type, request.LocalPoint, request.Target, request.MaxLength));
            return true;
        }

        public void DetachGrapple()
        {
            if (!_state.Grapple.IsGrappled || _state.Grapple.DetachmentPayload.IsPresent)
                return;

            _state.Grapple.DetachmentPayload.Set(new GrappleDetachPayload());
        }
    }
}