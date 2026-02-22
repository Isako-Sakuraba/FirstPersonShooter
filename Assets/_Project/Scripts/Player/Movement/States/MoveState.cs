using UnityEngine;

namespace Game.Player.Movement.States
{
    public class MoveState : PlayerMovementStateBase
    {
        public MoveState(PlayerContext context) : base(context) { }

        public override void Enter()
        {
            context.State.WallRunBeginTimer = 0f; // TODO: rewrite, please!!!
            context.State.JumpsLeft = context.Data.JumpAmount;
        }

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
            
            //Calculate target speed
            float targetSpeed = context.Input.Sprint ? context.Data.RunSpeed : context.Data.WalkSpeed;
            targetSpeed = context.Body.Stance == Stance.Standing ? targetSpeed : context.Data.CrouchSpeed;
            targetSpeed *= wishDir.magnitude;

            velocity = PlayerMovement.Accelerate(velocity, wishDir, targetSpeed, context.Data.GroundAcceleration, Time.fixedDeltaTime);

            // Apply new velocity to state
            context.State.Velocity = velocity;
        }
    }
}