using UnityEngine;

namespace Game.Player.Movement.States
{
    public class FallState : PlayerMovementStateBase
    {
        public FallState(PlayerContext context) : base(context) { }

        public override void Process()
        {
            context.State.Velocity.y += context.Data.Gravity * Time.fixedDeltaTime;
        }
    }
}