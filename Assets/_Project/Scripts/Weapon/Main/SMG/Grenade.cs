using UnityEngine;
using Game.Experimental;
using Game.Systems;

namespace Game.Weapons.Main
{
    public class Grenade : MonoBehaviour, IDamageable
    {
        private const float CollisionSkin = 0.001f;
        private const int MaxHits = 8;
        private const float DefaultExplosionMultiplier = 1f;

        [SerializeField] private LayerMask _obstacleLayer = Physics.DefaultRaycastLayers;
        [SerializeField] private float _radius = 0.12f;
        [SerializeField] private float _gravity = -9.81f;
        [SerializeField] private float _rotationSpeed = 420f;
        [SerializeField] private float _explosionRadius = 3f;
        [SerializeField] private int _explosionDamage = 50;
        [SerializeField] private float _revolverHitRadiusMultiplier = 1.2f;
        [SerializeField] private float _revolverHitDamageMultiplier = 1.5f;

        private readonly RaycastHit[] _hits = new RaycastHit[MaxHits];

        private Collider _selfCollider;
        private Vector3 _currentVelocity;
        private Vector3 _currentPosition;
        private Vector3 _rotationAxis;
        private bool _hasExploded;

        private void Awake()
        {
            _selfCollider = GetComponent<Collider>();
            _currentPosition = transform.position;
        }

        private void OnEnable()
        {
            _currentPosition = transform.position;
            _hasExploded = false;
            _rotationAxis = Random.onUnitSphere;
            if (_rotationAxis.sqrMagnitude <= Mathf.Epsilon)
            {
                _rotationAxis = Vector3.up;
            }
        }

        private void Update()
        {
            if (_hasExploded)
            {
                return;
            }

            transform.Rotate(_rotationAxis, _rotationSpeed * Time.deltaTime, Space.World);
        }

        private void FixedUpdate()
        {
            if (_hasExploded)
            {
                return;
            }

            float dt = Time.fixedDeltaTime;
            _currentVelocity += Vector3.up * (_gravity * dt);

            Vector3 displacement = _currentVelocity * dt;
            float distance = displacement.magnitude;
            if (distance <= Mathf.Epsilon)
            {
                return;
            }

            Vector3 direction = displacement / distance;
            if (TryGetClosestHit(direction, distance, out RaycastHit hit))
            {
                float moveDistance = Mathf.Max(hit.distance - CollisionSkin, 0f);
                _currentPosition += direction * moveDistance;
                transform.position = _currentPosition;
                Explode(DefaultExplosionMultiplier, DefaultExplosionMultiplier);
                return;
            }

            _currentPosition += displacement;
            transform.position = _currentPosition;
        }

        public void Launch(Vector3 velocity)
        {
            _currentVelocity = velocity;
            _currentPosition = transform.position;
        }

        public DamageResult TakeDamage(in DamageContext context)
        {
            if (context.DamageType == DamageType.Piercing &&
                context.Sender == DamageSender.Player &&
                context.Source == DamageSource.Revolver)
            {
                HitstopSystem.Trigger();
                Explode(_revolverHitRadiusMultiplier, _revolverHitDamageMultiplier);
            }

            return new DamageResult(transform.position);
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
                _obstacleLayer,
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

        private void Explode(float radiusMultiplier, float damageMultiplier)
        {
            if (_hasExploded)
            {
                return;
            }

            _hasExploded = true;

            ExperimentalExplosionManager manager = ExperimentalExplosionManager.Instance;
            if (manager != null)
            {
                float radius = _explosionRadius * Mathf.Max(0f, radiusMultiplier);
                int damage = Mathf.Max(0, Mathf.RoundToInt(_explosionDamage * Mathf.Max(0f, damageMultiplier)));
                manager.CauseExplosion(transform.position, radius, damage);
            }

            Destroy(gameObject);
        }
    }
}
