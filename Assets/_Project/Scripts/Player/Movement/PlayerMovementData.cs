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
        public float JumpBuffer = 0.1f;
        public float CoyoteTime = 0.1f;
        public int JumpAmount = 1;

        [Header("Gravity settings")]
        public float Gravity = -10f;

        [Header("Slide settings")]
        public float MinSlideEnterSpeed = 6.0f;        // must be moving fast enough
        public float MinSlideSpeed = 3.0f;        // below this -> end slide

        public float SlideEnterBoost = 1.0f;           // small impulse forward on entry (m/s)
        public float SlideFriction = 3.5f;             // higher = slows faster on flat
        public float SlideSteering = 6.0f;             // how strongly slide direction can rotate (rad/s-ish)
        public float SlideSteeringMinSpeed = 4.0f;     // below this, reduce steering

        public float SlideDownhillAcceleration = 20.0f;       // accel along slope direction (m/s^2), 0 disables
        public float SlideAllignRate = 10f;

        [Header("Wallrun settings")]
        public float WallrunDuration = 3f;
        public float WallrunAgainTimer = 2f; // When wall is the same, we must wait n seconds before running on the same wall

        public AnimationCurve WallrunGravityCurve;
        public float WallrunGravity = -2f;
        public float WallrunStickForce = 10f;
        public float WallrunEnterBoostHeight = 2.5f;

        public float WallrunAlongWallAcceleration = 25f;
        public float WallrunAlongWallMaxSpeed = 12f;
        public float WallrunMaxUpSpeed = 4.0f;
        public float WallrunMaxDownSpeed = 10.0f;

        public float WallJumpHeight = 2f;
        public float WallDetachJumpHeight = 1f;
        public float WallJumpForce;

        public float WallrunMinSpeedToSustain = 4f;
        public float WallrunUpBias = 0f;

        [Header("Layer Masks")]
        public LayerMask GroundMask;
    }
}