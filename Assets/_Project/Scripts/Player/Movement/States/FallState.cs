using UnityEngine;

namespace Game.Player.Movement.States
{
    public class FallState : PlayerMovementStateBase
    {
        public FallState(PlayerContext context) : base(context) { }

        public override void Process()
        {
            ApplyGravity();
            AirMove();
        }

        private void ApplyGravity()
        {
            context.State.Velocity.y += context.Data.Gravity * Time.fixedDeltaTime;
        }

        private void AirMove()
        {
            // Create shortcuts for context's input
            Vector3 wishDir = context.WishDir;

            // Get current horizontal velocity
            Vector3 velocity = new Vector3(context.State.Velocity.x, 0f, context.State.Velocity.z);

            float wishSpeed = context.Data.AirSpeed * wishDir.magnitude;
            wishSpeed = Mathf.Min(wishSpeed, context.Data.AirAccelerationSpeedCap);
            
            velocity = PlayerMovement.Accelerate(velocity, wishDir, wishSpeed, context.Data.AirAcceleration, Time.fixedDeltaTime);

            // Apply new velocity to state
            context.State.Velocity.x = velocity.x;
            context.State.Velocity.z = velocity.z;
        }

    }
}