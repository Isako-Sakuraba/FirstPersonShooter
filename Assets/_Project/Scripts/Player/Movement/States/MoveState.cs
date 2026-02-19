using UnityEngine;

namespace Game.Player.Movement.States
{
    public class MoveState : PlayerMovementStateBase
    {
        public MoveState(PlayerContext context) : base(context) { }

        public override void Process()
        {
            GroundMove();
        }

        private void GroundMove()
        {
            // Create shortcuts for context's input
            Vector3 wishDir = context.WishDir;

            // Get current velocity
            Vector3 velocity = new Vector3(context.State.Velocity.x, 0f, context.State.Velocity.z);

            // Apply friction and acceleration
            velocity = PlayerMovement.ApplyFriction(velocity, context.Data.GroundFriction, context.Data.StopSpeed, Time.fixedDeltaTime);
            float wishSpeed = context.Data.GroundSpeed * wishDir.magnitude;
            velocity = PlayerMovement.Accelerate(velocity, wishDir, wishSpeed, context.Data.GroundAcceleration, Time.fixedDeltaTime);

            // Apply new velocity to state
            context.State.Velocity = velocity;
        }
    }
}