using ECM2;
using Game.Data.Movement;
using UnityEngine;

namespace Game.Movement
{
    public sealed class LocomotionContext
    {
        public readonly MovementConfig Data;
        public readonly CharacterMovement Motor;
        public readonly CharacterOrientation Orientation;
        public readonly BodyController Body;
        public readonly LocomotionSensors Sensors;

        public readonly LocomotionInput Input;
        public readonly LocomotionState State;

        public readonly Transform Pointer;

        public bool IsGrounded => Motor.isGrounded;

        public Vector3 WishDir { get; private set; }

        public LocomotionContext(
            MovementConfig data,
            CharacterMovement motor,
            CharacterOrientation orientation,
            Transform pointer,
            BodyController body,
            LocomotionSensors sensors,
            LocomotionInput input,
            LocomotionState state)
        {
            Data = data;
            Motor = motor;
            Orientation = orientation;
            Body = body;
            Sensors = sensors;
            Input = input;
            State = state;
            Pointer = pointer;
        }

        public void InitializeTimers()
        {
            State.Wallrun.BeginCooldown = new ImprovedTimers.CountdownTimer(Data.Wallrun.NormalCooldown);
            State.Wallrun.DurationTimer = new ImprovedTimers.CountdownTimer(Data.Wallrun.Duration);
            State.Jump.CoyoteTimer = new ImprovedTimers.CountdownTimer(Data.Jump.CoyoteTime);
        }

        public void RecalculateWishDir()
        {
            var wish = Orientation.ForwardFlat * Input.Move.y + Orientation.RightFlat * Input.Move.x;
            WishDir = Vector3.ClampMagnitude(wish, 1f);
        }
    }
}