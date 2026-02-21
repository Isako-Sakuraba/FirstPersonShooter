using UnityEngine;

namespace Game.Player.Movement.States
{
    public class WallState : PlayerMovementStateBase
    {
        public WallState(PlayerContext context) : base(context) { }

        public override void Process()
        {
            Vector3 currentNormal = context.Sensors.WallCollision.normal;
            Vector3 stickToWallNormal = (-currentNormal).normalized;

            // TODO: rewrite to context.State.Velocity
            context.Controller.AddForce(stickToWallNormal * 8f, ForceMode.Acceleration);
        }
    }
}