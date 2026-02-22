using UnityEngine;

namespace Game.Player.Movement.States
{
    public class WallJumpState : PlayerMovementStateBase
    {
        public WallJumpState(PlayerContext context) : base(context) { }

        public override void Enter()
        {
            float height = context.Input.Jump ? context.Data.WallJumpHeight : context.Data.WallDetachJumpHeight;
            context.State.Velocity.y = Mathf.Sqrt(height * context.Data.Gravity * -2f);
            context.State.Velocity += context.State.LastWallNormal * context.Data.WallJumpForce;
            context.State.WallJump = true;
            context.State.JumpsLeft--;

            context.Input.ConsumeJumpBuffer();
            context.State.CoyoteTimer.Cancel();
        }
    }
}