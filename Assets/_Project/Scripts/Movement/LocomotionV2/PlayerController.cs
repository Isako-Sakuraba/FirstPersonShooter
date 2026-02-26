using ECM2;
using Game.Data.Movement;
using Game.Movement.API;
using Game.Movement.API.Requests;
using System;
using UnityEngine;

// TODO: change the namespace to something more appropriate
namespace Game.Movement.Player
{
    [RequireComponent(typeof(CharacterMovement))]
    public class PlayerController : MonoBehaviour,
        ILocomotionInfo,
        IRailAttachable
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

        private PlayerInputSource _inputSource;

        // Exposable fields
        private LocomotionSnapshot _snapshot;
        public LocomotionSnapshot Snapshot => _snapshot;

        public IBodyState Body => _body;

        #region ILocomotionInfo impl
        public Vector3 Forward => _orientation.Forward;

        public Vector3 Right => _orientation.Right;

        public Vector3 Velocity => _state.Kinematics.Velocity;

        public bool IsSliding => _machine.CurrentState == LocomotionMachineState.Grounded
                            && _machine.GroundedState == GroundedState.Slide;

        public bool IsWallrunning => _machine.CurrentState == LocomotionMachineState.Wall;

        public bool IsGrounded => _context.IsGrounded;

        public bool HasWallContact => _sensors.WallDetected;

        public Vector3 WallNormal => _sensors.WallCollision.normal;

        #endregion

        public bool IsOnRail => _state.Rail.IsAttached;

        public event Action<Stance, float> OnStanceChanged;

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

            _inputSource = new PlayerInputSource();

            _context.InitializeTimers();

            _machine = new LocomotionStateMachine(_context);
            _machine.CreateDefault(_context);


            UpdateSnapshot();
        }

        private void OnEnable()
        {
            _body.OnStanceChanged += HandleStanceChanged;
        }

        private void OnDisable()
        {
            _body.OnStanceChanged -= HandleStanceChanged;
        }

        private void Update()
        {
            _inputSource.UpdateFromService();
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

            UpdateSnapshot();
        }

        private void UpdateSnapshot()
        {
            _snapshot = new LocomotionSnapshot(
                _context.State.Kinematics.Velocity,
                _context.IsGrounded,
                _context.Body.Stance,
                _machine.CurrentState,
                _body.Height
            );
        }

        private void HandleStanceChanged(Stance stance)
        {
            OnStanceChanged.Invoke(stance, _body.Height);
        }

        #region IRailAttachable impl


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
        #endregion

        public readonly struct LocomotionSnapshot
        {
            public readonly Vector3 Velocity;
            public readonly bool IsGrounded;
            public readonly Stance Stance;

            // Movement mode if you track it (optional)
            public readonly LocomotionMachineState MachineState;

            // Camera-friendly values
            public readonly float Height;       // or current collider height

            public float HorizontalSpeed => new Vector2(Velocity.x, Velocity.z).magnitude;
            public float VerticalSpeed => Velocity.y;

            public LocomotionSnapshot(
                Vector3 velocity,
                bool isGrounded,
                Stance stance,
                LocomotionMachineState state,
                float capsuleHeight
            )
            {
                Velocity = velocity;
                IsGrounded = isGrounded;
                Stance = stance;
                MachineState = state;
                Height = capsuleHeight;
            }
        }
    }
}