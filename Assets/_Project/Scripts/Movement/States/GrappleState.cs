using Game.Utils;
using UnityEngine;

namespace Game.Movement.States
{
    public class GrappleState : LocomotionStateBase
    {
        public GrappleState(LocomotionContext context) : base(context)
        {

        }

        private float _ropeRestLength;
        private float _minLength = 2f ;
        private float _maxLength = 6f;

        private Vector3 _lastPivot = Vector3.zero;
        private Vector3 _prevPivotVelocity = Vector3.zero;

        public override void Enter()
        {
            if (!context.State.Grapple.AttachmentPayload.TryConsume(out var payload))
                Debug.LogError("Enetered Grapple state, but no payload present");

            context.State.Grapple.IsGrappled = true;
            context.State.Grapple.LocalPoint = payload.LocalPoint;
            context.State.Grapple.Target = payload.Target;
            _maxLength = payload.MaxLength;
            var pivot = GetWorldPivot(payload.LocalPoint, payload.Target);
            _ropeRestLength = (pivot - context.Motor.transform.position).magnitude;
            _lastPivot = pivot;
            context.State.Jump.JumpsLeft = context.Data.Jump.Amount;
            context.State.Grapple.OnGrappleStatusChanged.Invoke(true);
        }

        public override void Process()
        {
            var dt = Time.fixedDeltaTime;
            var invDt = 1 / dt;
            var localPivot = context.State.Grapple.LocalPoint;
            var pivot = GetWorldPivot(localPivot, context.State.Grapple.Target);
            var pivotVelocity = (pivot - _lastPivot) * invDt;
            var pivotAccel = (pivotVelocity - _prevPivotVelocity) * invDt;
            var position = context.Motor.transform.position;
            var velocity = context.State.Kinematics.Velocity;
            var gravity = context.Data.Grapple.Gravity;
            var drag = context.Data.Grapple.Drag;

            var toPivotDirection = pivot - position;
            toPivotDirection.Normalize();

            var lookDot = Vector3.Dot(context.Orientation.Forward, toPivotDirection);
            bool isLookMode = lookDot > 0.8f || lookDot < -0.8f;

            float lengthChange = -context.Input.Move.y * 4f * dt;
            float nextRestLength = Mathf.Clamp(_ropeRestLength + lengthChange, _minLength, _maxLength);

            float rate = (nextRestLength - _ropeRestLength) * invDt;

            if (!isLookMode)
                rate = 0f;
            else
                _ropeRestLength = nextRestLength;

            MovementMath.StepRopeSpring3D(
                pivot,
                pivotVelocity,
                pivotAccel,
                _ropeRestLength,
                rate,
                0.5f,
                200f,
                10f,
                gravity * Vector3.up,
                dt,
                isLookMode ? Vector3.zero : context.WishDir,
                10,
                drag,
                position,
                velocity,
                out var nextPosition,
                out var nextVelocity
            );

            //_position = nextPosition;
            context.State.Kinematics.Velocity = nextVelocity;

            _lastPivot = pivot;
            _prevPivotVelocity = pivotVelocity;

            context.State.Grapple.WorldPoint = pivot;
        }

        public override void Exit()
        {
            context.State.Grapple.IsGrappled = false;
            context.State.Grapple.LocalPoint = Vector3.zero;
            context.State.Grapple.Target = null;
            context.State.Grapple.OnGrappleStatusChanged.Invoke(false);
        }

        private Vector3 GetWorldPivot(Vector3 localPivot, Transform target)
        {
            return target.TransformPoint(localPivot);
        }
    }
}