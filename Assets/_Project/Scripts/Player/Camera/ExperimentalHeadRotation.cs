using Game.Player.Movement;
using UnityEngine;

namespace Game.Player.Experimental
{
    public class ExperimentalHeadRotation : MonoBehaviour
    {
        [SerializeField] private PlayerMovement _player;
        [SerializeField] private Transform _target;

        [Header("Lean")]
        [SerializeField] private float _maxRollDegrees = 12f;   // max tilt
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

            if (_player.Conext.State.IsSliding)
            {
                // Use horizontal velocity as the slide direction
                Vector3 v = _player.Conext.State.Velocity;
                Vector3 horiz = new Vector3(v.x, 0f, v.z);

                float speed = horiz.magnitude;
                if (speed > _minSpeed)
                {
                    Vector3 dir = horiz / speed;

                    // How much the slide points to the player's right (-1..+1)
                    float dotRight = Vector3.Dot(dir, _player.Conext.Orientation.Right);

                    // Slide right -> dotRight > 0 -> roll should go left (negative)
                    targetRoll = dotRight * _maxRollDegrees;
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