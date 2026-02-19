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
                var go = new GameObject(nameof(InputService));
                _instance = go.AddComponent<InputService>();
            }

            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    [SerializeField] private InputActionReference _moveAction;
    [SerializeField] private InputActionReference _mouseDeltaAction;
    [SerializeField] private InputActionReference _jumpAction;

    private Vector2 _move;
    private Vector2 _mouseDelta;
    private bool _jump;

    public Vector2 Move => _move;
    public Vector2 MouseDelta => _mouseDelta;
    public bool Jump => _jump;

    private void Update()
    {
        _move = _moveAction.action.ReadValue<Vector2>();
        _jump = _jumpAction.action.ReadValue<float>() > 0.1f;
        _mouseDelta = _mouseDeltaAction.action.ReadValue<Vector2>();
    }
}
