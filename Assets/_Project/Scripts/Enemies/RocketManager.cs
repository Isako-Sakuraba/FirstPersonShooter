using UnityEngine;
using UnityEngine.Pool;

namespace Game.Enemies
{
    public class RocketManager : MonoBehaviour
    {
        [SerializeField] private Rocket _rocketPrefab;
        [SerializeField] private Transform _rocketContainer;
        [SerializeField] private int _defaultCapacity = 16;
        [SerializeField] private int _maxPoolSize = 128;

        private static RocketManager _instance;
        public static RocketManager Instance => _instance;

        private ObjectPool<Rocket> _rocketPool;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            if (_rocketPrefab == null)
            {
                Debug.LogError("RocketManager requires a rocket prefab reference.", this);
                enabled = false;
                return;
            }

            _rocketPool = new ObjectPool<Rocket>(
                CreateRocket,
                OnRocketGet,
                OnRocketRelease,
                OnRocketDestroy,
                collectionCheck: true,
                defaultCapacity: Mathf.Max(1, _defaultCapacity),
                maxSize: Mathf.Max(1, _maxPoolSize));
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        public Rocket SpawnRocket(Vector3 position, Vector3 velocity)
        {
            if (_rocketPool == null)
            {
                return null;
            }

            Rocket rocket = _rocketPool.Get();
            rocket.Launch(this, position, velocity);
            return rocket;
        }

        public void ReleaseRocket(Rocket rocket)
        {
            if (_rocketPool == null || rocket == null)
            {
                return;
            }

            _rocketPool.Release(rocket);
        }

        private Rocket CreateRocket()
        {
            Transform parent = _rocketContainer != null ? _rocketContainer : transform;
            return Instantiate(_rocketPrefab, parent);
        }

        private static void OnRocketGet(Rocket rocket)
        {
            rocket.gameObject.SetActive(true);
        }

        private static void OnRocketRelease(Rocket rocket)
        {
            rocket.OnReturnedToPool();
            rocket.gameObject.SetActive(false);
        }

        private static void OnRocketDestroy(Rocket rocket)
        {
            if (rocket != null)
            {
                Destroy(rocket.gameObject);
            }
        }
    }
}
