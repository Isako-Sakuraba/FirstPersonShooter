using UnityEngine;

namespace Game.Player.Movement.States
{
    public class JumpState : PlayerMovementStateBase
    {
        public JumpState(PlayerContext context) : base(context) { }

        public override void Enter()
        {
            context.State.Velocity.y = Mathf.Sqrt(context.Data.JumpHeight * context.Data.Gravity * -2f);
            context.Controller.PauseGroundConstraint();
            context.State.JumpBufferTimer = 0f;
            context.State.JumpsLeft--;
        }

        public override void Process()
        {
            context.State.Velocity.y += context.Data.Gravity * Time.fixedDeltaTime;
        }
    }
}