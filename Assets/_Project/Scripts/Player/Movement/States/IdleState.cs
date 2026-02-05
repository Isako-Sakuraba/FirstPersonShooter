
using UnityEngine;

namespace Game.Player.Movement.States
{
    public class IdleState : PlayerMovementStateBase
    {
        public IdleState(PlayerContext context) : base(context) { }

        public override void Enter()
        {
            context.State.Velocity = Vector3.zero;
        }
    }
}