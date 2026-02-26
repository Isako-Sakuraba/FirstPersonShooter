using System;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-120)]
public class InputService : MonoBehaviour
{
    private static InputService _instance;
    public static InputService Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<InputService>();
            }

            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    [SerializeField] private InputActionReference _moveAction;
    [SerializeField] private InputActionReference _sprintAction;
    [SerializeField] private InputActionReference _crouchAction;
    [SerializeField] private InputActionReference _mouseDeltaAction;
    [SerializeField] private InputActionReference _jumpAction;
    [SerializeField] private InputActionReference _interactAction;

    private Vector2 _move;
    private Vector2 _mouseDelta;
    private bool _jump;
    private bool _sprint;
    private bool _crouch;
    private bool _interact;

    public Vector2 Move => _move;
    public Vector2 MouseDelta => _mouseDelta;
    public bool Jump => _jump;
    public bool Sprint => _sprint;
    public bool Crouch => _crouch;
    public bool Interact => _interact;

    public event Action OnInteractPressed = delegate { };

    private void Update()
    {
        _move = _moveAction.action.ReadValue<Vector2>();
        _jump = _jumpAction.action.triggered;
        _mouseDelta = _mouseDeltaAction.action.ReadValue<Vector2>();
        _sprint = _sprintAction.action.ReadValue<float>() > 0.1f;
        _crouch = _crouchAction.action.ReadValue<float>() > 0.1f;
        _interact = _interactAction.action.triggered;
    }

    private void OnEnable()
    {
        _interactAction.action.started += OnInteractActionStarted;
    }

    private void OnDisable()
    {
        _interactAction.action.started -= OnInteractActionStarted;
    }

    private void OnInteractActionStarted(InputAction.CallbackContext context)
    {
        OnInteractPressed.Invoke();
    }
}
