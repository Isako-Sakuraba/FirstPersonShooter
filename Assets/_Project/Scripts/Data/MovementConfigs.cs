using System;
using UnityEngine;

namespace Game.Data.Movement
{
    [Serializable]
    public class GroundConfig
    {
        public float WalkSpeed = 6f;
        public float RunSpeed = 10f;
        public float CrouchSpeed = 4f;
        public float Acceleration = 10f;
        public float Friction = 8f;
        public float StopSpeed = 2f;
    }

    [Serializable]
    public class AirConfig
    {
        public float Speed = 4f;
        public float Acceleration = 40f;
        public float AccelerationCap = 8f;
        public float VerticalTerminalVelocity = -40f;
    }

    [Serializable]
    public class WallrunConfig
    {
        public float Duration = 3f;
        public float NormalCooldown = 2f; // When wall is the same, we must wait n seconds before running on the same wall

        public float Gravity = -2f;
        public float StickForce = 10f;
        public float EnterBoostHeight = 2.5f;

        public float Acceleration = 25f;
        public float Speed = 12f;
        public Vector2 VerticalSpeedLimit = new Vector2(-3f, 10f);

        public float WallJumpHeight = 2f;
        public float ExitJumpHeight = 1f;
        public float ExitSeparationImpulse = 2f;

        public LayerMask IncludedLayers;

    }


    [Serializable]
    public class SlideConfig
    {
        public float RequiredEnterSpeed = 7.0f;        // must be moving fast enough
        public float MinExitSpeed = 4.0f;        // below this -> end slide

        public float EnterVelocityMultiplier = 1.2f;           // small impulse forward on entry (m/s)
        public float Friction = 3.5f;             // higher = slows faster on flat
        public float Steering = 6.0f;             // how strongly slide direction can rotate (rad/s-ish)
        public float SteeringDisableSpeed = 4.0f;     // below this, reduce steering

        public float DownhillAcceleration = 20.0f;       // accel along slope direction (m/s^2), 0 disables
        public float AllignRate = 10f;
    }


    [Serializable]
    public class JumpConfig
    {
        public float Height = 2f;
        public float Buffer = 0.1f;
        public float CoyoteTime = 0.1f;
        public int Amount = 1;
    }

    [Serializable]
    public class EnvironmentConfig
    {
        public float Gravity = -18f;
    }
}