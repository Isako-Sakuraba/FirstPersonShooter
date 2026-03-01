using Game.Utils;
using UnityEngine;

namespace Game.Movement.States
{
    public class GrappleState : LocomotionStateBase
    {
        public GrappleState(LocomotionContext context) : base(context)
        {
            var go = new GameObject("GrappleLine");
            _line = go.AddComponent<LineRenderer>();
            _line.positionCount = 2;
            _line.widthMultiplier = 0.1f;
            _line.enabled = false;
        }

        private float _length;
        private LineRenderer _line; // Experimental

        public override void Enter()
        {
            if (!context.State.Grapple.AttachmentPayload.TryConsume(out var payload))
                Debug.LogError("Enetered Grapple state, but no payload present");

            context.State.Grapple.IsGrappled = true;
            context.State.Grapple.Point = payload.Point;
            _length = (context.State.Grapple.Point - context.Motor.transform.position).magnitude;
            context.Input.ConsumeGrappleBuffer();
            _line.enabled = true;
        }

        public override void Process()
        {
            var dt = Time.fixedDeltaTime;
            var pivot = context.State.Grapple.Point;
            var position = context.Motor.transform.position;
            var velocity = context.State.Kinematics.Velocity;
            var gravity = context.Data.Grapple.Gravity;
            var drag = context.Data.Grapple.Drag;

            velocity = MovementMath.Accelerate(velocity, context.WishDir, 40f, 0.2f, dt);

            MovementMath.StepPendulum3D(
                pivot,
                _length,
                gravity * Vector3.up,
                dt,
                position,
                velocity,
                out var nextPosition,
                out var nextVelocity,
                drag);

            //_position = nextPosition;
            context.State.Kinematics.Velocity = nextVelocity;

            _line.SetPosition(0, pivot);
            _line.SetPosition(1, position);
        }

        public override void Exit()
        {
            context.State.Grapple.IsGrappled = false;
            context.State.Grapple.Point = Vector3.zero;
            _line.enabled = false;
        }
    }
}