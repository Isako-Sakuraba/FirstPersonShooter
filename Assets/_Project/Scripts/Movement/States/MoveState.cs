using Game.Utils;
using UnityEngine;

namespace Game.Movement.States
{
    public class MoveState : LocomotionStateBase
    {
        public MoveState(LocomotionContext context) : base(context) { }

        public override void Enter()
        {
            context.State.Wallrun.BeginCooldown.Finish();
            context.State.Jump.CoyoteTimer.Cancel();
            context.State.Jump.JumpsLeft = context.Data.Jump.Amount;
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
            Vector3 velocity = new Vector3(context.State.Kinematics.Velocity.x, 0f, context.State.Kinematics.Velocity.z);

            // Apply friction and acceleration
            velocity = MovementMath.ApplyFriction(velocity, context.Data.Ground.Friction, context.Data.Ground.StopSpeed, Time.fixedDeltaTime);

            //Calculate target speed
            float targetSpeed = context.Input.SprintHeld ? context.Data.Ground.RunSpeed : context.Data.Ground.WalkSpeed;
            targetSpeed = context.Body.Stance == Stance.Standing ? targetSpeed : context.Data.Ground.CrouchSpeed;
            targetSpeed *= wishDir.magnitude;

            velocity = MovementMath.Accelerate(velocity, wishDir, targetSpeed, context.Data.Ground.Acceleration, Time.fixedDeltaTime);

            // Apply new velocity to state
            context.State.Kinematics.Velocity = velocity;
        }
    }
}