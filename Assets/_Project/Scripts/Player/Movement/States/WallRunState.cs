using System.Data;
using UnityEngine;

namespace Game.Player.Movement.States
{
    public class WallRunState : PlayerMovementStateBase
    {
        private float _wallrunTimer;
        private Vector3 _wallNormal;
        private Vector3 _wallDirection;

        public bool WallRunTimerEnded => _wallrunTimer <= 0f;

        public WallRunState(PlayerContext context) : base(context) 
        { 
            _wallrunTimer = context.Data.WallrunDuration;
        }

        public override void Enter()
        {
            context.State.IsWallrunning = true;
            context.State.LastWallNormal = context.Sensors.WallCollision.normal;
            context.State.JumpsLeft = context.Data.JumpAmount;
            _wallNormal = context.Sensors.WallCollision.normal;

            context.State.Velocity.y = Mathf.Sqrt(context.Data.WallrunEnterBoostHeight * context.Data.WallrunGravity * -2f);
        }

        public override void Exit()
        {
            _wallrunTimer = context.Data.WallrunDuration;
            context.State.IsWallrunning = false;
            context.State.WallRunBeginTimer = context.Data.WallrunAgainTimer;
            context.State.WallrunEndTimerEnded = false;

        }

        public override void Process()
        {
            _wallrunTimer -= Time.fixedDeltaTime;
            context.State.WallrunEndTimerEnded = WallRunTimerEnded;
            context.State.LastWallNormal = _wallNormal;

            float dt = Time.fixedDeltaTime;

            _wallNormal = context.Sensors.WallCollision.normal;
            Vector3 velocity = context.State.Velocity;

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
            velocity += (-_wallNormal) * (context.Data.WallrunStickForce * dt);

            // Apply wallrun gravity
            velocity += Vector3.up * (context.Data.WallrunGravity * dt);
            if (context.Data.WallrunUpBias != 0f)
                velocity += Vector3.up * (context.Data.WallrunUpBias * dt);

            // Clamp vertical speed
            // velocity.y = Mathf.Clamp(velocity.y, -context.Data.WallrunMaxDownSpeed, context.Data.WallrunMaxUpSpeed);

            //// Remove all non-wall velocity
            //velocity = Vector3.ProjectOnPlane(velocity, _wallNormal);

            //// Acceleration along wall
            //Vector3 wish = context.WishDir;
            //float wishMag = wish.magnitude;

            //if (wishMag > 0.05f)
            //{
            //    Vector3 wishDir = wish / wishMag;

            //    // Project wish onto the wall plane (so pushing into wall doesn't matter)
            //    Vector3 wishOnWall = Vector3.ProjectOnPlane(wishDir, _wallNormal);
            //    if (wishOnWall.sqrMagnitude > 0.0001f)
            //    {
            //        wishOnWall.Normalize();

            //        // Optional: prefer acceleration primarily along _wallDir (classic TF feel).
            //        // This prevents “sideways wobble” when the camera isn't perfectly aligned.
            //        float along = Vector3.Dot(wishOnWall, _wallDirection); // -1..1

            //        // Accelerate along wall direction based on input alignment.
            //        Vector3 accelDir = _wallDirection * Mathf.Sign(along);

            //        // If input is mostly opposite, you can allow slowdown/reverse or just slow down.
            //        // Titanfall tends to keep direction consistent; so we scale accel by alignment.
            //        float accelScale = Mathf.Abs(along); // 0..1

            //        // Apply accel
            //        velocity += accelDir * (context.Data.WallrunAlongWallAcceleration * accelScale * dt);
            //    }
            //}

            // Clamp horizontal speed
            Vector3 horiz = new Vector3(velocity.x, 0f, velocity.z);
            float horizSpeed = horiz.magnitude;

            if (context.Data.WallrunAlongWallMaxSpeed > 0f && horizSpeed > context.Data.WallrunAlongWallMaxSpeed)
            {
                horiz = horiz * (context.Data.WallrunAlongWallMaxSpeed / horizSpeed);
                velocity.x = horiz.x;
                velocity.z = horiz.z;
            }

            // Write back
            context.State.Velocity = velocity;




            //_wallrunTimer -= Time.fixedDeltaTime;
            //context.State.WallrunEndTimerEnded = WallRunTimerEnded;

            //_wallNormal = context.Sensors.WallCollision.normal;
            //Vector3 stickToWallNormal = (-_wallNormal).normalized;

            //// TODO: rewrite to context.State.Velocity
            //context.Controller.AddForce(stickToWallNormal * context.Data.WallrunStickForce, ForceMode.Acceleration);

            //float normalizedWallrunTime = 1f - _wallrunTimer / context.Data.WallrunDuration;
            //float gravityMultiplyer = 1f - context.Data.WallrunGravityCurve.Evaluate(normalizedWallrunTime);
            //context.State.Velocity.y = (gravityMultiplyer * context.Data.WallrunGravity);
        }
    }
}