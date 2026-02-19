using UnityEngine;

namespace Game.Player.Movement
{
    [CreateAssetMenu(fileName = "NewMovementData", menuName = "Static Data/Movement Data")]
    public class PlayerMovementData : ScriptableObject
    {
        [Header("Ground settings")]
        public float GroundSpeed = 8f;
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

        [Header("Layer Masks")]
        public LayerMask GroundMask;
    }
}