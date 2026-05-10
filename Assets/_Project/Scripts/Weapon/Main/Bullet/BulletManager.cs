using UnityEngine;
using UnityEngine.Pool;

namespace Game.Weapons.Main
{
    public class BulletManager : MonoBehaviour
    {
        [SerializeField] private Bullet _bulletPrefab;
        [SerializeField] private Transform _bulletContainer;
        [SerializeField] private LayerMask _hitLayer = Physics.DefaultRaycastLayers;
        [SerializeField] private float _bulletLifetime = 2f;
        [SerializeField] private int _defaultCapacity = 64;
        [SerializeField] private int _maxPoolSize = 512;

        private static BulletManager _instance;
        public static BulletManager Instance => _instance;

        private ObjectPool<Bullet> _bulletPool;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            if (_bulletPrefab == null)
            {
                Debug.LogError("BulletManager requires a bullet prefab reference.", this);
                enabled = false;
                return;
            }

            _bulletPool = new ObjectPool<Bullet>(
                CreateBullet,
                OnBulletGet,
                OnBulletRelease,
                OnBulletDestroy,
                collectionCheck: true,
                defaultCapacity: _defaultCapacity,
                maxSize: _maxPoolSize);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        public Bullet SpawnBullet(Vector3 position, Vector3 velocity, int damage)
        {
            if (_bulletPool == null)
            {
                return null;
            }

            Bullet bullet = _bulletPool.Get();
            bullet.Launch(this, position, velocity, damage, _bulletLifetime, _hitLayer);
            return bullet;
        }

        public void ReleaseBullet(Bullet bullet)
        {
            if (bullet == null || _bulletPool == null)
            {
                return;
            }

            _bulletPool.Release(bullet);
        }

        private Bullet CreateBullet()
        {
            Transform parent = _bulletContainer != null ? _bulletContainer : transform;
            return Instantiate(_bulletPrefab, parent);
        }

        private static void OnBulletGet(Bullet bullet)
        {
            bullet.gameObject.SetActive(true);
        }

        private static void OnBulletRelease(Bullet bullet)
        {
            bullet.OnReturnedToPool();
            bullet.gameObject.SetActive(false);
        }

        private static void OnBulletDestroy(Bullet bullet)
        {
            if (bullet != null)
            {
                Destroy(bullet.gameObject);
            }
        }
    }
}
