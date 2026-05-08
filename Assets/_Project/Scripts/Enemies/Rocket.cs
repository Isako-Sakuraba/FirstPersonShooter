using UnityEngine;
using Game.Experimental;
using Game.Movement;
using Game.Weapons;
using Game.Systems;

namespace Game.Enemies
{
    public class Rocket : MonoBehaviour, IParryable
    {
        private const float CollisionSkin = 0.001f;
        private const int MaxHits = 8;

        [SerializeField] private LayerMask _hitLayer = Physics.DefaultRaycastLayers;
        [SerializeField] private float _radius = 0.08f;
        [SerializeField] private float _lifetime = 6f;
        [SerializeField] private int _damage = 30;
        [SerializeField] private int _parryDamage = 60;
        [SerializeField] private float _hitExplosionRadius = 1.8f;
        [SerializeField] private float _ignoreSenderDuration = 0.15f;
        [SerializeField] private float _homingTurnSpeed = 180f;
        [SerializeField] private float _playerTargetVerticalOffset = 0.8f;
        [SerializeField] private Transform _visuals;
        [SerializeField] private TrailRenderer _trailRenderer;
        [SerializeField] private bool _interpolateRootWhenNoVisuals = true;
        [SerializeField] private DamageSender _damageSender = DamageSender.Enemy;
        [SerializeField] private DamageType _damageType = DamageType.Piercing;
        [SerializeField] private DamageSource _damageSource = DamageSource.Unknown;
        [SerializeField] private float _parrySpeedMultiplier = 2f;

        private readonly RaycastHit[] _hits = new RaycastHit[MaxHits];

        private RocketManager _manager;
        private Collider _selfCollider;
        private Vector3 _currentPosition;
        private Vector3 _previousPosition;
        private Vector3 _visualsLocalOffset;
        private Vector3 _velocity;
        private int _currentDamage;
        private DamageSender _currentDamageSender;
        private DamageType _currentDamageType;
        private DamageSource _currentDamageSource;
        private float _ignoreSenderUntilTime;
        private float _remainingLifetime;
        private bool _isActive;
        private bool _isParried;
        private bool _enableTrailNextFrame;

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

            if (!_isParried)
            {
                ApplyHoming(dt);
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

                TryExplodeOnImpact(hit.collider, hit.point, hit.normal);
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
            if (!_enableTrailNextFrame || _trailRenderer == null)
            {
                return;
            }

            _enableTrailNextFrame = false;
            _trailRenderer.Clear();
            _trailRenderer.emitting = true;
        }

        public void Launch(RocketManager manager, Vector3 position, Vector3 velocity)
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

            _currentDamage = _damage;
            _currentDamageSender = _damageSender;
            _currentDamageType = _damageType;
            _currentDamageSource = _damageSource;
            _ignoreSenderUntilTime = Time.time + Mathf.Max(0f, _ignoreSenderDuration);

            _remainingLifetime = Mathf.Max(0.01f, _lifetime);
            _isActive = true;
            _isParried = false;
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

        public void Launch(Vector3 position, Vector3 velocity)
        {
            Launch(null, position, velocity);
        }

        public void Launch(Vector3 velocity)
        {
            Launch(transform.position, velocity);
        }

        public void OnReturnedToPool()
        {
            _manager = null;
            _velocity = Vector3.zero;
            _currentDamage = _damage;
            _currentDamageSender = _damageSender;
            _currentDamageType = _damageType;
            _currentDamageSource = _damageSource;
            _ignoreSenderUntilTime = 0f;
            _remainingLifetime = 0f;
            _isActive = false;
            _isParried = false;
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

        private void ApplyHoming(float deltaTime)
        {
            if (!PlayerLocator.HasPlayer)
            {
                return;
            }

            float speed = _velocity.magnitude;
            if (speed <= Mathf.Epsilon)
            {
                return;
            }

            Vector3 targetPoint = PlayerLocator.Position + Vector3.up * _playerTargetVerticalOffset;
            Vector3 toTarget = targetPoint - _currentPosition;
            if (toTarget.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            Quaternion currentRotation = Quaternion.LookRotation(_velocity.normalized);
            Quaternion targetRotation = Quaternion.LookRotation(toTarget.normalized);
            Quaternion steeredRotation = Quaternion.RotateTowards(
                currentRotation,
                targetRotation,
                Mathf.Max(0f, _homingTurnSpeed) * deltaTime);

            _velocity = steeredRotation * Vector3.forward * speed;
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

                if (ShouldIgnoreCollider(hit.collider))
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

            if (ShouldIgnoreCollider(collider))
            {
                return;
            }

            DamageContext damageContext = new(
                _currentDamage,
                point,
                normal,
                _currentDamageSender,
                _currentDamageType,
                _currentDamageSource);

            damageable.TakeDamage(in damageContext);
        }

        private void TryExplodeOnImpact(Collider hitCollider, Vector3 hitPoint, Vector3 hitNormal)
        {
            float explosionRadius = Mathf.Max(0f, _hitExplosionRadius);
            if (explosionRadius <= Mathf.Epsilon)
            {
                TryDamage(hitCollider, hitPoint, hitNormal);
                return;
            }

            ExperimentalExplosionManager manager = ExperimentalExplosionManager.Instance;
            if (manager == null)
            {
                TryDamage(hitCollider, hitPoint, hitNormal);
                return;
            }

            manager.CauseExplosion(hitPoint, explosionRadius, _currentDamage, _currentDamageSender);
        }

        public void OnParry(in ParryContext context)
        {
            if (!_isActive)
            {
                return;
            }

            HitstopSystem.Trigger();

            Vector3 direction = context.Direction;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                direction = transform.forward;
            }

            direction.Normalize();

            float currentSpeed = _velocity.magnitude;
            float speed = currentSpeed * Mathf.Max(0f, _parrySpeedMultiplier);
            _velocity = direction * speed;
            transform.rotation = Quaternion.LookRotation(direction);

            _currentDamage = Mathf.Max(0, _parryDamage);
            _currentDamageSender = DamageSender.Player;
            _isParried = true;
            _ignoreSenderUntilTime = Time.time + Mathf.Max(0f, _ignoreSenderDuration);
        }

        private bool ShouldIgnoreCollider(Collider collider)
        {
            if (collider == null)
            {
                return false;
            }

            if (Time.time >= _ignoreSenderUntilTime)
            {
                return false;
            }

            return IsSenderCollider(collider);
        }

        private bool IsSenderCollider(Collider collider)
        {
            if (_currentDamageSender == DamageSender.Player)
            {
                return collider.GetComponentInParent<LocomotionController>() != null;
            }

            Transform root = collider.transform.root;
            return collider.CompareTag("Enemy") || (root != null && root.CompareTag("Enemy"));
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
                _manager.ReleaseRocket(this);
                return;
            }

            Destroy(gameObject);
        }
    }
}
