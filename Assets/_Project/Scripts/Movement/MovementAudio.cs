using UnityEngine;
using Game.Movement;

namespace Game.Experimental
{
    [DisallowMultipleComponent]
    public class MovementAudio : MonoBehaviour
    {
        [SerializeField] private LocomotionController _locomotionController;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip[] _footstepClips;
        [SerializeField, Range(0f, 1f)] private float _footstepVolume = 1f;
        [SerializeField, Min(0f)] private float _minSpeedForFootsteps = 0.1f;
        [SerializeField, Min(0.05f)] private float _stepDistance = 1.3f;

        private Vector3 _lastSamplePosition;
        private float _distanceAccumulator;
        private bool _hasLastSamplePosition;
        private int _footstepClipIndex;

        private void Awake()
        {
            if (_locomotionController == null)
            {
                _locomotionController = GetComponent<LocomotionController>();
            }

            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
            }
        }

        private void OnEnable()
        {
            _distanceAccumulator = 0f;
            _hasLastSamplePosition = false;
            _footstepClipIndex = 0;
        }

        private void Update()
        {
            if (_locomotionController == null || _audioSource == null)
            {
                return;
            }

            Vector3 velocity = _locomotionController.Velocity;
            velocity.y = 0f;
            float speed = velocity.magnitude;

            bool shouldPlayFootsteps =
                _locomotionController.IsGrounded &&
                !_locomotionController.IsSliding &&
                speed >= _minSpeedForFootsteps;

            Vector3 currentPosition = _locomotionController.transform.position;

            if (!shouldPlayFootsteps)
            {
                ResetDistanceTracking(currentPosition);
                return;
            }

            if (!_hasLastSamplePosition)
            {
                _lastSamplePosition = currentPosition;
                _hasLastSamplePosition = true;
                return;
            }

            Vector3 displacement = currentPosition - _lastSamplePosition;
            displacement.y = 0f;
            _lastSamplePosition = currentPosition;

            float travelledDistance = displacement.magnitude;
            if (travelledDistance <= Mathf.Epsilon)
            {
                return;
            }

            _distanceAccumulator += travelledDistance;

            float stepDistance = Mathf.Max(0.05f, _stepDistance);
            if (_distanceAccumulator < stepDistance)
            {
                return;
            }

            PlayNextFootstep();
            _distanceAccumulator = Mathf.Max(0f, _distanceAccumulator - stepDistance);
        }

        private void PlayNextFootstep()
        {
            int clipsCount = _footstepClips != null ? _footstepClips.Length : 0;
            if (clipsCount <= 0)
            {
                return;
            }

            if (_footstepClipIndex >= clipsCount)
            {
                _footstepClipIndex = 0;
            }

            AudioClip clip = _footstepClips[_footstepClipIndex];

            _footstepClipIndex++;
            if (_footstepClipIndex >= clipsCount)
            {
                _footstepClipIndex = 0;
            }

            if (clip != null)
            {
                _audioSource.PlayOneShot(clip, _footstepVolume);
            }
        }

        private void ResetDistanceTracking(Vector3 currentPosition)
        {
            _lastSamplePosition = currentPosition;
            _hasLastSamplePosition = true;
            _distanceAccumulator = 0f;
        }
    }
}
