using Game.Movement;
using Game.Player;
using Game.Systems;
using UnityEngine;

namespace Game.Rigs
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
        private PauseMenu _pauseMenu;

        private void Awake()
        {
            _inputService = InputService.Instance;
            _pauseMenu = FindFirstObjectByType<PauseMenu>();
            LockCursor();
        }

        private void Update()
        {
            if (PlayerHealth.Instance != null && PlayerHealth.Instance.IsDead)
            {
                _previousDelta = Vector2.zero;
                return;
            }

            if (_pauseMenu != null && _pauseMenu.IsOpen)
            {
                _previousDelta = Vector2.zero;
                return;
            }

            Vector2 delta = _inputService.MouseDelta;
            float dt = Time.unscaledDeltaTime;
            float smoothingFactor = 1f - Mathf.Exp(-_smoothing * dt);
            delta = Vector2.Lerp(_previousDelta, delta, smoothingFactor);

            float mouseX = delta.x * _sensitivity.x;
            float mouseY = delta.y * _sensitivity.y;

            float sensitivityScale = SensitivitySlider.Value;
            mouseX *= sensitivityScale;
            mouseY *= sensitivityScale;

            _yaw += mouseX;

            _pitch -= mouseY;
            _pitch = Mathf.Clamp(_pitch, _pitchLimits.x, _pitchLimits.y);
            _orientation.UpdateAll(mouseX, -mouseY, _pitchLimits);

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
