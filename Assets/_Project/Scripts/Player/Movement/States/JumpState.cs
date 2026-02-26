using UnityEngine;

namespace Game.Movement.States
{
    public class JumpState : LocomotionStateBase
    {
        public JumpState(LocomotionContext context) : base(context) { }

        public override void Enter()
        {
            context.State.Jump.Payload.TryConsume(out var payload);

            float height = 0f;

            if (payload.Kind == JumpKind.Normal || payload.Kind == JumpKind.Rail)
                height = context.Data.Jump.Height;
            else if (payload.Kind == JumpKind.Wall)
                height = context.Data.Wallrun.ExitJumpHeight;

            context.State.Kinematics.Velocity.y = MovementMath.ToJumpForce(context.Data.Jump.Height, context.Data.Environment.Gravity);

            if (payload.Kind == JumpKind.Wall)
                context.State.Kinematics.Velocity += payload.WallNormal * context.Data.Wallrun.ExitSeparationImpulse;

            context.Motor.PauseGroundConstraint();
            context.Input.ConsumeJumpBuffer();
            context.State.Jump.CoyoteTimer.Cancel();
            context.State.Jump.JumpsLeft--;
        }
    }
}