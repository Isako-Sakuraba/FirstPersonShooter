using ECM2;
using Game.Input;
using Game.Movement;
using UnityEngine;

namespace Game.Experimental
{
    public class NoclipAbility : MonoBehaviour
    {
        [SerializeField] private LocomotionController _controller;
        [SerializeField] private CharacterMovement _motor;
        [SerializeField] private Transform _look;
        [SerializeField] private float _fastSpeed = 20f;
        [SerializeField] private float _normalSpeed = 6f;
        [SerializeField] private float _verticalSpeed = 6f;

        private InputService _inputService;
        private LayerMask _cachedLayers;

        private bool _noclipEnabled = false;

        private void Awake()
        {
            _inputService = InputService.Instance;
        }

        private void Update()
        {
            if (_inputService.NoclipPressed)
            {
                _noclipEnabled = !_noclipEnabled;
                HandleNoclipButton(_noclipEnabled);
            }
        }

        private void FixedUpdate()
        {
            if ( !_noclipEnabled )
                return;

            Vector2 move = _inputService.Move.normalized;
            bool fast = _inputService.Sprint;
            float vertical = 0f;
            if (_inputService.JumpHeld) vertical += _verticalSpeed;
            if (_inputService.Crouch) vertical -= _verticalSpeed;

            float speed = fast ? _fastSpeed : _normalSpeed;

            Vector3 desired =
                (_look.right * move.x + _look.forward * move.y) * speed +
                (Vector3.up * vertical) * _verticalSpeed;

            _motor.velocity = desired;
            _motor.Move(Time.fixedDeltaTime);
        }

        private void HandleNoclipButton(bool enable)
        {
            if (enable)
            {
                _controller.enabled = false;
                _cachedLayers = _motor.collisionLayers;
                _motor.collisionLayers = 0;
                _motor.velocity = Vector3.zero;
            } 
            else
            {
                _motor.collisionLayers = _cachedLayers;
                _controller.enabled = true;
            }
        }
    }
}