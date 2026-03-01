using ImprovedTimers;
using UnityEngine;

namespace Game.Movement
{
    public interface ILocomotionInputSource
    {
        Vector2 Move { get; }
        bool JumpPressed { get; }
        bool JumpHeld { get; }
        bool SprintHeld { get; }
        bool CrouchHeld { get; }
        bool FirePressed { get; }
    }

    public sealed class LocomotionInput
    {
        public Vector2 Move { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool SprintHeld { get; private set; }
        public bool CrouchHeld { get; private set; }
        public bool FirePressed { get; private set; }

        public CountdownTimer JumpBufferTimer { get; } = new(0.2f);
        public CountdownTimer GrappleBufferTimer { get; } = new(0.2f);

        public void UpdateFrom(ILocomotionInputSource source)
        {
            Move = source.Move;
            JumpPressed = source.JumpPressed;
            SprintHeld = source.SprintHeld;
            CrouchHeld = source.CrouchHeld;
            FirePressed = source.FirePressed;

            if (JumpPressed)
                JumpBufferTimer.Start();

            if (FirePressed)
                GrappleBufferTimer.Start();
        }

        public void ConsumeJumpBuffer()
            => JumpBufferTimer.Cancel();

        public void ConsumeGrappleBuffer()
            => JumpBufferTimer.Cancel();
    }
}