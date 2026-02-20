using Unity.VisualScripting;
using UnityEngine;

namespace Game.Player.Movement
{
    [CreateAssetMenu(fileName = "NewMovementData", menuName = "Static Data/Movement Data")]
    public class PlayerMovementData : ScriptableObject
    {
        [Header("Ground settings")]
        public float WalkSpeed = 6f;
        public float RunSpeed = 10f;
        public float CrouchSpeed = 4f;
        public float GroundAcceleration = 10f;
        public float GroundFriction = 8f;
        public float StopSpeed = 2f;

        [Header("Airborne settings")]
        public float AirSpeed = 4f;
        public float AirAcceleration = 40f;
        public float AirAccelerationSpeedCap = 8f;

        [Header("Jump settings")]
        public float JumpHeight = 2f;

        [Header("Gravity settings")]
        public float Gravity = -10f;

        [Header("Slide settings")]
        public float MinEnterSpeed = 6.0f;        // must be moving fast enough
        public float MinSlideSpeed = 3.0f;        // below this -> end slide

        public float EnterBoost = 1.0f;           // small impulse forward on entry (m/s)
        public float Friction = 3.5f;             // higher = slows faster on flat
        public float Steering = 6.0f;             // how strongly slide direction can rotate (rad/s-ish)
        public float SteeringMinSpeed = 4.0f;     // below this, reduce steering

        public float DownhillAccel = 20.0f;       // accel along slope direction (m/s^2), 0 disables
        public float MaxSpeed = 20.0f;            // optional clamp for slide

        public float MaxGroundAngle = 55.0f;      // too steep? decide policy; still fine to slide on steep
        public float ExitOnReleaseDelay = 0.0f;   // if you want “hold to slide”; 0 = immediate check
        public bool RequireCrouchHeld = false;

        [Header("Layer Masks")]
        public LayerMask GroundMask;
    }
}