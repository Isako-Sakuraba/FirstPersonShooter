using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Player
{
    public class PlayerDeathUI : MonoBehaviour
    {
        [SerializeField] private PlayerHealth _playerHealth;
        [SerializeField] private Text _deathText;
        [SerializeField] private float _restartDelaySeconds = 3f;

        private Coroutine _restartCoroutine;
        private PlayerHealth _subscribedHealth;
        private bool _deathHandled;

        private void Awake()
        {
            if (_deathText != null)
            {
                _deathText.gameObject.SetActive(false);
            }

            _deathHandled = false;
        }

        private void OnEnable()
        {
            TryBindPlayerHealth();
        }

        private void Start()
        {
            TryBindPlayerHealth();
        }

        private void Update()
        {
            if (_subscribedHealth == null)
            {
                TryBindPlayerHealth();
            }
        }

        private void OnDisable()
        {
            UnbindPlayerHealth();

            if (_restartCoroutine != null)
            {
                StopCoroutine(_restartCoroutine);
                _restartCoroutine = null;
            }

            _deathHandled = false;
            Time.timeScale = 1f;
        }

        private PlayerHealth ResolvePlayerHealth()
        {
            if (_playerHealth != null)
            {
                return _playerHealth;
            }

            if (PlayerHealth.Instance != null)
            {
                return PlayerHealth.Instance;
            }

            return FindFirstObjectByType<PlayerHealth>();
        }

        private void TryBindPlayerHealth()
        {
            if (_subscribedHealth != null)
            {
                return;
            }

            PlayerHealth health = ResolvePlayerHealth();
            if (health == null)
            {
                return;
            }

            _subscribedHealth = health;
            _subscribedHealth.Died += OnPlayerDied;
        }

        private void UnbindPlayerHealth()
        {
            if (_subscribedHealth == null)
            {
                return;
            }

            _subscribedHealth.Died -= OnPlayerDied;
            _subscribedHealth = null;
        }

        private void OnPlayerDied()
        {
            if (_deathHandled)
            {
                return;
            }

            _deathHandled = true;

            if (_deathText != null)
            {
                _deathText.gameObject.SetActive(true);
            }

            if (_restartCoroutine != null)
            {
                StopCoroutine(_restartCoroutine);
            }

            _restartCoroutine = StartCoroutine(RestartSceneRoutine());
        }

        private IEnumerator RestartSceneRoutine()
        {
            Time.timeScale = 0f;
            float delay = Mathf.Max(0f, _restartDelaySeconds);
            yield return new WaitForSecondsRealtime(delay);
            Time.timeScale = 1f;
            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }
    }
}
