using Game.Player.Movement;
using UnityEngine;

namespace Game.Player
{
    public class CameraRig : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private CharacterOrientation _orientation;
        [SerializeField] private Transform _target;
        [SerializeField] private Transform _head;

        [Header("Settings")]
        [SerializeField] private Vector2 _sensitivity;
        [SerializeField] private float _smoothing = 5f;
        [SerializeField] private Vector2 _pitchLimits;
        [SerializeField] private bool _lockCursor = true;

        private float _pitch;
        private float _yaw;
        private Vector2 _previousDelta;

        private InputService _inputService;

        private void Awake()
        {
            _inputService = InputService.Instance;
            LockCursor();
        }

        private void Update()
        {
            Vector2 delta = _inputService.MouseDelta;
            delta = Vector2.Lerp(_previousDelta, delta, _smoothing * Time.deltaTime);

            float mouseX = delta.x * _sensitivity.x;
            float mouseY = delta.y * _sensitivity.y;

            _yaw += mouseX;

            _pitch -= mouseY;
            _pitch = Mathf.Clamp(_pitch, _pitchLimits.x, _pitchLimits.y);
            _orientation.UpdateYaw(mouseX);

            _previousDelta = delta;
        }

        private void LateUpdate()
        {
            _target.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            _target.position = _head.position;
        }

        private void LockCursor()
        {
            if (_lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}