using UnityEngine;

namespace Game.Player.Movement.States
{
    public class JumpState : PlayerMovementStateBase
    {
        public JumpState(PlayerContext context) : base(context) { }

        public override void Enter()
        {
            Debug.Log("Entered jump");
            context.State.Velocity.y += Mathf.Sqrt(context.Data.JumpHeight * context.Data.Gravity * -2f);
            context.Controller.PauseGroundConstraint();
        }

        public override void Process()
        {
            context.State.Velocity.y += context.Data.Gravity * Time.fixedDeltaTime;
        }
    }
}