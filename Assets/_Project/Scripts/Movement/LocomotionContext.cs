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

        public bool IsGrounded => Motor.isGrounded;

        public Vector3 WishDir { get; private set; }

        public LocomotionContext(
            MovementConfig data,
            CharacterMovement motor,
            CharacterOrientation orientation,
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
        }

        public void InitializeTimers()
        {
            State.Wallrun.BeginCooldown = new ImprovedTimers.CountdownTimer(Data.Wallrun.NormalCooldown);
            State.Wallrun.DurationTimer = new ImprovedTimers.CountdownTimer(Data.Wallrun.Duration);
            State.Jump.CoyoteTimer = new ImprovedTimers.CountdownTimer(Data.Jump.CoyoteTime);
        }

        public void RecalculateWishDir()
        {
            var wish = Orientation.Forward * Input.Move.y + Orientation.Right * Input.Move.x;
            WishDir = Vector3.ClampMagnitude(wish, 1f);
        }
    }

    public static class MovementMath
    {
        public static Vector3 Accelerate(Vector3 velocity, Vector3 wishDir, float wishSpeed, float accel, float deltaTime)
        {
            if (wishSpeed <= 0f || wishDir.sqrMagnitude < 0.0001f) return velocity;

            float currentSpeed = Vector3.Dot(velocity, wishDir);
            float addSpeed = wishSpeed - currentSpeed;
            if (addSpeed <= 0f) return velocity;

            float accelSpeed = accel * wishSpeed * deltaTime;
            if (accelSpeed > addSpeed) accelSpeed = addSpeed;

            return velocity + wishDir * accelSpeed;
        }

        public static Vector3 ApplyFriction(Vector3 velocity, float friction, float stopSpeed, float deltaTime)
        {
            float speed = velocity.magnitude;
            if (speed < 0.001f) return Vector3.zero;

            float control = speed < stopSpeed ? stopSpeed : speed;
            float drop = control * friction * deltaTime;
            float newSpeed = Mathf.Max(speed - drop, 0f);

            return velocity * (newSpeed / speed);
        }

        public static float ToJumpForce(float jumpHeight, float gravity)
            => Mathf.Sqrt(jumpHeight * -2f * gravity);
    }
}