using UnityEngine;

namespace Game.Player.Movement.States
{
    public class WalkState : PlayerMovementStateBase
    {
        public WalkState(PlayerContext context) : base(context) { }

        public override void Process()
        {
            Vector2 moveInput = context.Input.Move.normalized;

            Vector3 forward = context.Input.Orientation;

            Quaternion rightRotaiton = Quaternion.Euler(0f, 90f, 0f);
            Vector3 right = rightRotaiton * forward;
            Vector3 velocity = forward * moveInput.y + right * moveInput.x;

            context.State.Velocity = velocity;
        }

        public override void Exit()
        {
            Debug.Log("Exited walk");
        }
    }
}