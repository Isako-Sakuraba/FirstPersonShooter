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

    [SerializeField] private InputActionReference _fireAction;
    [SerializeField] private InputActionReference _moveAction;
    [SerializeField] private InputActionReference _sprintAction;
    [SerializeField] private InputActionReference _crouchAction;
    [SerializeField] private InputActionReference _mouseDeltaAction;
    [SerializeField] private InputActionReference _jumpAction;
    [SerializeField] private InputActionReference _interactAction;
    [SerializeField] private InputActionReference _noclipAction;

    private Vector2 _move;
    private Vector2 _mouseDelta;
    private bool _jumpPressed;
    private bool _jumpHeld;
    private bool _sprint;
    private bool _crouch;
    private bool _interact;
    private bool _noclipPressed;
    private bool _firePressed;

    public Vector2 Move => _move;
    public Vector2 MouseDelta => _mouseDelta;
    public bool JumpPressed => _jumpPressed;
    public bool JumpHeld => _jumpHeld;
    public bool Sprint => _sprint;
    public bool Crouch => _crouch;
    public bool Interact => _interact;
    public bool NoclipPressed => _noclipPressed;
    public bool FirePressed => _firePressed;

    public event Action OnInteractPressed = delegate { };

    private void Update()
    {
        _move = _moveAction.action.ReadValue<Vector2>();
        _jumpPressed = _jumpAction.action.triggered;
        _jumpHeld = _jumpAction.action.ReadValue<float>() > 0.1f;
        _mouseDelta = _mouseDeltaAction.action.ReadValue<Vector2>();
        _sprint = _sprintAction.action.ReadValue<float>() > 0.1f;
        _crouch = _crouchAction.action.ReadValue<float>() > 0.1f;
        _interact = _interactAction.action.triggered;
        _noclipPressed = _noclipAction.action.triggered;
        _firePressed = _fireAction.action.triggered;
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
