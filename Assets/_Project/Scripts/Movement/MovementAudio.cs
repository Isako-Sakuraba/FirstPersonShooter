using UnityEngine;
using Game.Movement;

namespace Game.Experimental
{
    [DisallowMultipleComponent]
    public class MovementAudio : MonoBehaviour
    {
        [SerializeField] private LocomotionController _locomotionController;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField, Min(0f)] private float _minSpeedForFootsteps = 0.1f;
        [SerializeField, Min(0.01f)] private float _maxSpeedForCadence = 8f;
        [SerializeField, Min(0.05f)] private float _slowStepInterval = 0.5f;
        [SerializeField, Min(0.05f)] private float _fastStepInterval = 0.2f;

        private float _nextStepTime;

        private void Awake()
        {
            if (_audioSource == null)
                _audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (_locomotionController == null || _audioSource == null)
                return;

            Vector3 velocity = _locomotionController.Velocity;
            velocity.y = 0f;
            float speed = velocity.magnitude;

            bool shouldPlayFootsteps =
                _locomotionController.IsGrounded &&
                !_locomotionController.IsSliding &&
                speed >= _minSpeedForFootsteps;

            if (!shouldPlayFootsteps)
            {
                _nextStepTime = Time.time;
                return;
            }

            if (Time.time < _nextStepTime)
                return;

            if (_audioSource.isPlaying)
                return;

            _audioSource.Play();

            float normalizedSpeed = Mathf.Clamp01(speed / _maxSpeedForCadence);
            float stepInterval = Mathf.Lerp(_slowStepInterval, _fastStepInterval, normalizedSpeed);
            _nextStepTime = Time.time + stepInterval;
        }
    }
}
