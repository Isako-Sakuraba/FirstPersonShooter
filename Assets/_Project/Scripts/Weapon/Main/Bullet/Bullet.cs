using UnityEngine;
using Game.Experimental;

namespace Game.Weapons.Main
{
    public class Bullet : MonoBehaviour
    {
        private const float CollisionSkin = 0.001f;
        private const int MaxHits = 8;

        [SerializeField] private float _radius = 0.03f;
        [SerializeField] private Transform _visuals;
        [SerializeField] private TrailRenderer _trailRenderer;
        [SerializeField] private bool _interpolateRootWhenNoVisuals = true;

        private readonly RaycastHit[] _hits = new RaycastHit[MaxHits];

        private BulletManager _manager;
        private Collider _selfCollider;
        private Vector3 _currentPosition;
        private Vector3 _previousPosition;
        private Vector3 _visualsLocalOffset;
        private Vector3 _velocity;
        private LayerMask _hitLayer;
        private int _damage;
        private int _explosionDamage;
        private float _explosionRadius;
        private float _remainingLifetime;
        private bool _isActive;
        private bool _isExplosive;
        private bool _enableTrailNextFrame;

        public bool IsActive => _isActive;

        private void Awake()
        {
            _selfCollider = GetComponent<Collider>();
            _currentPosition = transform.position;
            _previousPosition = _currentPosition;

            if (_visuals != null)
            {
                _visualsLocalOffset = _visuals.localPosition;
            }
        }

        private void FixedUpdate()
        {
            if (!_isActive)
            {
                return;
            }

            float dt = Time.fixedDeltaTime;
            _previousPosition = _currentPosition;
            _remainingLifetime -= dt;
            if (_remainingLifetime <= 0f)
            {
                ReleaseSelf();
                return;
            }

            Vector3 displacement = _velocity * dt;
            float distance = displacement.magnitude;
            if (distance <= Mathf.Epsilon)
            {
                return;
            }

            Vector3 direction = displacement / distance;
            transform.rotation = Quaternion.LookRotation(direction);

            if (TryGetClosestHit(direction, distance, out RaycastHit hit))
            {
                float moveDistance = Mathf.Max(hit.distance - CollisionSkin, 0f);
                _currentPosition += direction * moveDistance;
                transform.position = _currentPosition;

                if (_isExplosive)
                {
                    if (!TryExplode(hit.point))
                    {
                        TryDamage(hit.collider, hit.point, hit.normal);
                    }
                }
                else
                {
                    TryDamage(hit.collider, hit.point, hit.normal);
                }

                ReleaseSelf();
                return;
            }

            _currentPosition += displacement;
            transform.position = _currentPosition;
        }

        private void Update()
        {
            if (!_isActive)
            {
                return;
            }

            float alpha = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
            Vector3 interpolatedPosition = Vector3.Lerp(_previousPosition, _currentPosition, alpha);

            if (_visuals != null)
            {
                _visuals.localPosition = _visualsLocalOffset + (interpolatedPosition - _currentPosition);
                return;
            }

            if (_interpolateRootWhenNoVisuals)
            {
                transform.position = interpolatedPosition;
            }
        }

        private void LateUpdate()
        {
            TryEnableTrailEmission();
        }

        public void Launch(BulletManager manager, Vector3 position, Vector3 velocity, int damage, float lifetime, LayerMask hitLayer)
        {
            _manager = manager;
            _currentPosition = position;
            _previousPosition = position;
            transform.position = position;

            _velocity = velocity;
            if (_velocity.sqrMagnitude > Mathf.Epsilon)
            {
                transform.rotation = Quaternion.LookRotation(_velocity.normalized);
            }

            _damage = Mathf.Max(0, damage);
            _explosionDamage = 0;
            _explosionRadius = 0f;
            _remainingLifetime = Mathf.Max(0f, lifetime);
            _hitLayer = hitLayer;
            _isActive = true;
            _isExplosive = false;
            _enableTrailNextFrame = false;

            if (_trailRenderer != null)
            {
                _trailRenderer.emitting = false;
                _trailRenderer.Clear();
                _enableTrailNextFrame = true;
            }

            if (_visuals != null)
            {
                _visuals.localPosition = _visualsLocalOffset;
            }
        }

        public void OnReturnedToPool()
        {
            _manager = null;
            _previousPosition = _currentPosition;
            _velocity = Vector3.zero;
            _damage = 0;
            _explosionDamage = 0;
            _explosionRadius = 0f;
            _remainingLifetime = 0f;
            _isActive = false;
            _isExplosive = false;
            _enableTrailNextFrame = false;

            if (_trailRenderer != null)
            {
                _trailRenderer.emitting = false;
                _trailRenderer.Clear();
            }

            if (_visuals != null)
            {
                _visuals.localPosition = _visualsLocalOffset;
            }
        }

        private bool TryGetClosestHit(Vector3 direction, float distance, out RaycastHit closestHit)
        {
            closestHit = default;

            int hitCount = Physics.SphereCastNonAlloc(
                _currentPosition,
                _radius,
                direction,
                _hits,
                distance,
                _hitLayer,
                QueryTriggerInteraction.Ignore);

            if (hitCount <= 0)
            {
                return false;
            }

            bool found = false;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = _hits[i];
                if (hit.collider == null || hit.collider == _selfCollider)
                {
                    continue;
                }

                if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                    closestHit = hit;
                    found = true;
                }
            }

            return found;
        }

        private void TryDamage(Collider collider, Vector3 point, Vector3 normal)
        {
            if (collider == null || !collider.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                return;
            }

            DamageContext damageContext = new(
                _damage,
                point,
                normal,
                DamageSender.Player,
                DamageType.Piercing,
                DamageSource.Bullet);

            damageable.TakeDamage(in damageContext);
        }

        public void MakeExplosive(float radius, int damage)
        {
            if (!_isActive)
            {
                return;
            }

            _isExplosive = true;
            _explosionRadius = Mathf.Max(0f, radius);
            _explosionDamage = Mathf.Max(0, damage);
        }

        private bool TryExplode(Vector3 point)
        {
            ExperimentalExplosionManager manager = ExperimentalExplosionManager.Instance;
            if (manager == null)
            {
                return false;
            }

            manager.CauseExplosion(point, _explosionRadius, _explosionDamage);
            return true;
        }

        private void ReleaseSelf()
        {
            if (!_isActive)
            {
                return;
            }

            _isActive = false;

            if (_manager != null)
            {
                _manager.ReleaseBullet(this);
                return;
            }

            Destroy(gameObject);
        }

        private void TryEnableTrailEmission()
        {
            if (!_enableTrailNextFrame || _trailRenderer == null)
            {
                return;
            }

            _enableTrailNextFrame = false;
            _trailRenderer.Clear();
            _trailRenderer.emitting = true;
        }
    }
}
