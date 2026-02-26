using UnityEngine;

namespace Game.Movement.States
{
    public class FallState : LocomotionStateBase
    {
        public FallState(LocomotionContext context) : base(context) { }

        public override void Process()
        {
            ApplyGravity();
            AirMove();
        }

        private void ApplyGravity()
        {
            context.State.Kinematics.Velocity.y += context.Data.Environment.Gravity * Time.fixedDeltaTime;
        }

        private void AirMove()
        {
            // Create shortcuts for context's input
            Vector3 wishDir = context.WishDir;

            // Get current horizontal velocity
            Vector3 velocity = new Vector3(context.State.Kinematics.Velocity.x, 0f, context.State.Kinematics.Velocity.z);

            float wishSpeed = context.Data.Air.Speed * wishDir.magnitude;
            wishSpeed = Mathf.Min(wishSpeed, context.Data.Air.AccelerationCap);

            velocity = MovementMath.Accelerate(velocity, wishDir, wishSpeed, context.Data.Air.Acceleration, Time.fixedDeltaTime);

            // Apply new velocity to state
            context.State.Kinematics.Velocity.x = velocity.x;
            context.State.Kinematics.Velocity.z = velocity.z;
        }

    }
}