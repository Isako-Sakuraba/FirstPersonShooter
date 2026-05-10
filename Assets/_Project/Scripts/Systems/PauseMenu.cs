using UnityEngine;

namespace Game.Systems
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject _menuRoot;
        [SerializeField] private CanvasGroup _menuCanvasGroup;
        [SerializeField] private bool _lockCursor = true;

        private InputService _inputService;
        private bool _isOpen;
        private bool _menuRootIsSelf;

        public bool IsOpen => _isOpen;

        private void Awake()
        {
            _inputService = InputService.Instance;

            if (_menuRoot == null)
            {
                _menuRoot = transform.childCount > 0 ? transform.GetChild(0).gameObject : gameObject;
            }

            _menuRootIsSelf = _menuRoot == gameObject;

            if (_menuCanvasGroup == null && _menuRoot != null)
            {
                _menuCanvasGroup = _menuRoot.GetComponent<CanvasGroup>();
            }

            SetMenuVisible(_menuRoot != null && _menuRoot.activeSelf);

            _isOpen = _menuRoot.activeSelf;
        }

        private void Start()
        {
            if (!_lockCursor && !_isOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void Update()
        {
            Debug.Log($"PauseMenu IsOpen: {_isOpen}");

            if (!_inputService.ExitPressed)
            {
                return;
            }

            Toggle();
        }

        public void Open()
        {
            SetMenuVisible(true);

            Time.timeScale = 0f;
            _isOpen = true;

            if (_lockCursor)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void Close()
        {
            Time.timeScale = 1f;

            SetMenuVisible(false);

            _isOpen = false;

            if (_lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public void Toggle()
        {
            if (_isOpen)
            {
                Close();
                return;
            }

            Open();
        }

        private void SetMenuVisible(bool isVisible)
        {
            if (_menuRoot == null)
            {
                return;
            }

            if (!_menuRootIsSelf)
            {
                _menuRoot.SetActive(isVisible);
                return;
            }

            if (_menuCanvasGroup != null)
            {
                _menuCanvasGroup.alpha = isVisible ? 1f : 0f;
                _menuCanvasGroup.interactable = isVisible;
                _menuCanvasGroup.blocksRaycasts = isVisible;
            }
        }
    }
}
