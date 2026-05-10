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
    [SerializeField] private InputActionReference _altFireAction;
    [SerializeField] private InputActionReference _scrollAction;
    [SerializeField] private InputActionReference _punchAction;
    [SerializeField] private InputActionReference _moveAction;
    [SerializeField] private InputActionReference _sprintAction;
    [SerializeField] private InputActionReference _crouchAction;
    [SerializeField] private InputActionReference _mouseDeltaAction;
    [SerializeField] private InputActionReference _jumpAction;
    [SerializeField] private InputActionReference _interactAction;
    [SerializeField] private InputActionReference _noclipAction;
    [SerializeField] private InputActionReference _exitAction;
    [SerializeField] private InputActionReference _weapon1Action;
    [SerializeField] private InputActionReference _weapon2Action;
    [SerializeField] private InputActionReference _weapon3Action;

    private Vector2 _move;
    private Vector2 _mouseDelta;
    private bool _jumpPressed;
    private bool _jumpHeld;
    private bool _sprint;
    private bool _crouch;
    private bool _interact;
    private bool _noclipPressed;
    private bool _exitPressed;
    private bool _puchPressed;
    private int _scroll;

    private bool _firePressed;
    private bool _fireHeld;
    private bool _fireReleased;

    private bool _altFirePressed;
    private bool _altFireHeld;
    private bool _altFireReleased;

    private bool _weapon1Pressed;
    private bool _weapon2Pressed;
    private bool _weapon3Pressed;

    public Vector2 Move => _move;
    public Vector2 MouseDelta => _mouseDelta;
    public bool JumpPressed => _jumpPressed;
    public bool JumpHeld => _jumpHeld;
    public bool Sprint => _sprint;
    public bool Crouch => _crouch;
    public bool Interact => _interact;
    public bool NoclipPressed => _noclipPressed;
    public bool ExitPressed => _exitPressed;
    public bool PunchPressed => _puchPressed;

    public bool FirePressed => _firePressed;
    public bool FireHeld => _fireHeld;
    public bool FireReleased => _fireReleased;

    public bool AltFirePressed => _altFirePressed;
    public bool AltFireHeld => _altFireHeld;
    public bool AltFireReleased => _altFireReleased;

    public bool Weapon1Pressed => _weapon1Pressed;
    public bool Weapon2Pressed => _weapon2Pressed;
    public bool Weapon3Pressed => _weapon3Pressed;


    public int Scroll => _scroll;

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
        _exitPressed = _exitAction.action.WasPressedThisFrame();

        _firePressed = _fireAction.action.WasPressedThisFrame();
        _fireHeld = _fireAction.action.IsPressed();
        _fireReleased = _fireAction.action.WasReleasedThisFrame();

        _altFirePressed = _altFireAction.action.WasPressedThisFrame();
        _altFireHeld = _altFireAction.action.IsPressed();
        _altFireReleased = _altFireAction.action.WasReleasedThisFrame();

        var scrollDelta = _scrollAction.action.ReadValue<Vector2>().y;
        _scroll = 0;

        if (scrollDelta < 0f)
            _scroll = 1;
        else if (scrollDelta > 0f)
            _scroll = -1;

        _puchPressed = _punchAction.action.WasPressedThisFrame();

        _weapon1Pressed = _weapon1Action.action.WasPressedThisFrame();
        _weapon2Pressed = _weapon2Action.action.WasPressedThisFrame();
        _weapon3Pressed = _weapon3Action.action.WasPressedThisFrame();

        Debug.Log($"ExitPressed: {_exitPressed}");
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
