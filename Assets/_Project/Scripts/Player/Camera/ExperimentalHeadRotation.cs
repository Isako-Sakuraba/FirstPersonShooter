using Game.Player.Movement;
using UnityEngine;

namespace Game.Player.Experimental
{
    public class ExperimentalHeadRotation : MonoBehaviour
    {
        [SerializeField] private PlayerMovement _player;
        [SerializeField] private Transform _target;

        [Header("Lean")]
        [SerializeField] private float _maxRollDegrees = 12f;   // max tilt (slide)
        [SerializeField] private float _wallRunRollDegrees = 16f; // max tilt (wallrun)
        [SerializeField] private float _smooth = 12f;           // higher = snappier
        [SerializeField] private float _minSpeed = 0.5f;        // ignore tiny motion

        private Quaternion _baseLocalRot;
        private float _currentRoll;

        private void Awake()
        {
            if (_target != null)
                _baseLocalRot = _target.localRotation;
        }

        private void Update()
        {
            if (_player == null || _target == null) return;

            float dt = Time.deltaTime;
            float targetRoll = 0f;

            var ctx = _player.Conext;

            // Priority: wallrun over slide (feel free to swap)
            if (ctx.Sensors.WallDetected && ctx.State.IsWallrunning)
            {
                Vector3 wallN = ctx.Sensors.WallCollision.normal;

                float side = Vector3.Dot(wallN, ctx.Orientation.Right); // >0 => wall normal points right => wall is on left, typically
                // We want the wall side, not normal direction. If your normal convention differs, flip the sign.
                // Lean into wall: wall on right -> roll right. A simple way is invert side.
                float leanSign = -Mathf.Sign(side);

                // Optionally scale by speed so tiny contacts don't tilt
                Vector3 v = ctx.State.Velocity;
                Vector3 horiz = new Vector3(v.x, 0f, v.z);
                float speed = horiz.magnitude;

                float forward = Vector3.Dot(horiz.normalized, ctx.Orientation.Forward);
                forward = Mathf.Abs(forward);
                

                if (speed > _minSpeed)
                {
                    //float speed01 = Mathf.Clamp01((speed - _minSpeed) / Mathf.Max(0.0001f, (ctx.Data.WalkSpeed - _minSpeed)));
                    targetRoll = forward * leanSign * _wallRunRollDegrees * 1f;
                }
                else
                {
                    targetRoll = 0f;
                }
            }
            else if (ctx.State.IsSliding)
            {
                // Use horizontal velocity as the slide direction
                Vector3 v = ctx.State.Velocity;
                Vector3 horiz = new Vector3(v.x, 0f, v.z);

                float speed = horiz.magnitude;
                if (speed > _minSpeed)
                {
                    Vector3 dir = horiz / speed;

                    // How much the slide points to the player's right (-1..+1)
                    float dotRight = Vector3.Dot(dir, ctx.Orientation.Right);

                    // Slide right -> tilt left (negative)
                    targetRoll = -dotRight * -_maxRollDegrees;
                }
            }

            // Smooth roll
            float t = 1f - Mathf.Exp(-_smooth * dt);
            _currentRoll = Mathf.Lerp(_currentRoll, targetRoll, t);

            // Roll about local forward axis (Z)
            _target.localRotation = _baseLocalRot * Quaternion.AngleAxis(_currentRoll, Vector3.forward);
        }
    }
}