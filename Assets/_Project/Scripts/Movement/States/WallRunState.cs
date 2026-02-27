using Game.Utils;
using UnityEngine;

namespace Game.Movement.States
{
    public class WallRunState : LocomotionStateBase
    {
        private float _wallrunTimer;
        private Vector3 _wallNormal;
        private Vector3 _wallDirection;

        public bool WallRunTimerEnded => _wallrunTimer <= 0f;

        public WallRunState(LocomotionContext context) : base(context) { }

        public override void Enter()
        {
            //context.State.IsWallrunning = true;
            context.State.Wallrun.LastWallNormal = context.Sensors.WallCollision.normal;
            context.State.Jump.JumpsLeft = context.Data.Jump.Amount;
            context.State.Wallrun.DurationTimer.Start();
            _wallNormal = context.Sensors.WallCollision.normal;

            context.State.Kinematics.Velocity.y = MovementMath.ToJumpForce(context.Data.Wallrun.EnterBoostHeight, context.Data.Wallrun.Gravity);
        }

        public override void Exit()
        {
            //context.State.IsWallrunning = false;
            context.State.Wallrun.BeginCooldown.Start();
        }

        public override void Process()
        {
            context.State.Wallrun.LastWallNormal = _wallNormal;

            float dt = Time.fixedDeltaTime;

            _wallNormal = context.Sensors.WallCollision.normal;
            Vector3 velocity = context.State.Kinematics.Velocity;

            // Recalculate wallrun direction
            Vector3 tangent = Vector3.Cross(Vector3.up, _wallNormal);
            if (tangent.sqrMagnitude > 0.0001f)
            {
                tangent.Normalize();
                float a = Vector3.Dot(tangent, context.Orientation.Forward);
                float b = Vector3.Dot(-tangent, context.Orientation.Forward);
                _wallDirection = (b > a) ? -tangent : tangent;
            }

            ////// Cancel velocity that pushes away from wall
            ////float vOut = Vector3.Dot(velocity, _wallNormal);

            ////if (vOut > 0f)
            ////{
            ////    // Cancel outward velocity smoothly (accel-based, stable)
            ////    float cancel = context.Data.WallNormalCancel * dt;
            ////    float newVOut = Mathf.Max(0f, vOut - cancel);
            ////    velocity += _wallNormal * (newVOut - vOut);
            ////}

            // Apply stick to wall force
            velocity += (-_wallNormal) * (context.Data.Wallrun.StickForce * dt);

            // Apply wallrun gravity
            velocity += Vector3.up * (context.Data.Wallrun.Gravity * dt);

            // Clamp vertical speed
            velocity.y = Mathf.Clamp(
                velocity.y,
                context.Data.Wallrun.VerticalSpeedLimit.x,
                context.Data.Wallrun.VerticalSpeedLimit.y
            );

            // Clamp horizontal speed
            Vector3 horiz = new Vector3(velocity.x, 0f, velocity.z);
            float horizSpeed = horiz.magnitude;

            if (context.Data.Wallrun.Speed > 0f && horizSpeed > context.Data.Wallrun.Speed)
            {
                horiz = horiz * (context.Data.Wallrun.Speed / horizSpeed);
                velocity.x = horiz.x;
                velocity.z = horiz.z;
            }

            // Write back
            context.State.Kinematics.Velocity = velocity;
        }
    }
}