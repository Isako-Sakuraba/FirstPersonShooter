using Game.Movement;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ExperimentalWeaponBob : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _target;
    [SerializeField] private LocomotionController _player;

    [Header("Bob Settings")]
    [SerializeField] private float _maxSpeed = 8f;
    [SerializeField] private float _bobFrequency = 14f;
    [SerializeField] private float _returnSpeed = 14f;
    [SerializeField] private float _minSpeedToBob = 0.1f;

    [Header("Bob Shape")]
    [SerializeField] private float _bobHorizontalAmount = 0.04f;
    [SerializeField] private float _bobTopHeight = 0.02f;
    [SerializeField] private float _bobBottomDepth = 0.03f;

    [Header("Sway Position")]
    [SerializeField] private float _swayPositionX = 0.015f;
    [SerializeField] private float _swayPositionY = 0.01f;
    [SerializeField] private float _swayPositionSmooth = 10f;
    [SerializeField] private float _maxSwayPosition = 0.03f;

    [Header("Sway Rotation")]
    [SerializeField] private float _swayRotationX = 2.5f;
    [SerializeField] private float _swayRotationY = 3.5f;
    [SerializeField] private float _swayRotationZ = 2f;
    [SerializeField] private float _swayRotationSmooth = 12f;
    [SerializeField] private float _maxSwayRotation = 5f;

    private Vector3 _baseLocalPosition;
    private Quaternion _baseLocalRotation;

    private float _bobTime;

    private Vector3 _currentBobOffset;
    private Vector3 _currentSwayPosition;
    private Vector3 _currentSwayEuler;

    private void Awake()
    {
        if (_target == null)
            _target = transform;

        _baseLocalPosition = _target.localPosition;
        _baseLocalRotation = _target.localRotation;
    }

    private void LateUpdate()
    {
        if (_player == null)
            return;

        float _speed = GetHorizontalSpeed();
        Vector2 _lookDelta = GetLookDelta();

        UpdateBob(_speed);
        UpdateSway(_lookDelta);

        _target.localPosition = _baseLocalPosition + _currentBobOffset + _currentSwayPosition;
        _target.localRotation = _baseLocalRotation * Quaternion.Euler(_currentSwayEuler);
    }

    private float GetHorizontalSpeed()
    {
        Vector2 _horizontal = new Vector2(_player.Velocity.x, _player.Velocity.z);
        return _horizontal.magnitude;
    }

    private void UpdateBob(float _speed)
    {
        if (!CanBob(_speed))
        {
            _currentBobOffset = Vector3.Lerp(
                _currentBobOffset,
                Vector3.zero,
                _returnSpeed * Time.deltaTime
            );
            return;
        }

        float _normalizedSpeed = Mathf.Clamp01(_speed / _maxSpeed);
        _bobTime += Time.deltaTime * _bobFrequency * Mathf.Lerp(0.8f, 1.25f, _normalizedSpeed);

        float _t = Mathf.PingPong(_bobTime, 2f);

        Vector3 _topLeft = new Vector3(-_bobHorizontalAmount, _bobTopHeight, 0f);
        Vector3 _bottomCenter = new Vector3(0f, -_bobBottomDepth, 0f);
        Vector3 _topRight = new Vector3(_bobHorizontalAmount, _bobTopHeight, 0f);

        Vector3 _targetBobOffset = _t < 1f
            ? Vector3.Lerp(_topLeft, _bottomCenter, _t)
            : Vector3.Lerp(_bottomCenter, _topRight, _t - 1f);

        _targetBobOffset *= _normalizedSpeed;

        _currentBobOffset = Vector3.Lerp(
            _currentBobOffset,
            _targetBobOffset,
            _returnSpeed * Time.deltaTime
        );
    }

    private bool CanBob(float _speed)
    {
        if (_player.IsSliding)
            return false;

        if (_player.IsRailgrinding)
            return false;

        if (_player.IsGrappling)
            return false;

        if (_player.MachineState == LocomotionMachineState.Air)
            return false;

        if (_speed < _minSpeedToBob)
            return false;

        return true;
    }

    private void UpdateSway(Vector2 _lookDelta)
    {
        Vector3 _targetSwayPosition = new Vector3(
            -_lookDelta.x * _swayPositionX,
            -_lookDelta.y * _swayPositionY,
            0f
        );

        _targetSwayPosition = Vector3.ClampMagnitude(_targetSwayPosition, _maxSwayPosition);

        _currentSwayPosition = Vector3.Lerp(
            _currentSwayPosition,
            _targetSwayPosition,
            _swayPositionSmooth * Time.deltaTime
        );

        Vector3 _targetSwayEuler = new Vector3(
            _lookDelta.y * _swayRotationX,
            -_lookDelta.x * _swayRotationY,
            -_lookDelta.x * _swayRotationZ
        );

        _targetSwayEuler.x = Mathf.Clamp(_targetSwayEuler.x, -_maxSwayRotation, _maxSwayRotation);
        _targetSwayEuler.y = Mathf.Clamp(_targetSwayEuler.y, -_maxSwayRotation, _maxSwayRotation);
        _targetSwayEuler.z = Mathf.Clamp(_targetSwayEuler.z, -_maxSwayRotation, _maxSwayRotation);

        _currentSwayEuler = Vector3.Lerp(
            _currentSwayEuler,
            _targetSwayEuler,
            _swayRotationSmooth * Time.deltaTime
        );
    }

    private Vector2 GetLookDelta()
    {
        return InputService.Instance.MouseDelta;
    }
}