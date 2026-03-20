using System.Collections.Generic;
using UnityEngine;
using Game.Experimental;
using Game.Systems;

namespace Game.Weapons.Main
{
    public class Coin : MonoBehaviour, IDamageable, IParryable
    {
        [SerializeField] private LayerMask _targetLayer;
        [SerializeField] private LayerMask _obstacleLayer = Physics.DefaultRaycastLayers;
        [SerializeField] private Transform _visuals;
        [SerializeField] private float _gravity = -10f;
        [SerializeField] private float _apex = 6f;
        [SerializeField] private float _radius = 0.36f;
        [SerializeField] private float _bounceDamping = 0.9f;
        [SerializeField] private float _checkNearestRadius = 40f;
        [SerializeField] private Collider _personalCollider;
        [SerializeField] private TrailRenderer _trailRenderer;
        [SerializeField] private ParticleSystem _apexWindowParticles;
        [SerializeField] private float _apexWindowSeconds = 0.1f;
        [SerializeField] private float _visualsEndTimeOffset;
        [SerializeField] private float _lowMultiplier = 1.2f;
        [SerializeField] private float _apexMultiplier = 2f;
        [SerializeField] private int _parryDamage = 40;

        private const float CollisionSkin = 0.001f;
        private const int MaxHits = 8;
        private const int MaxColliders = 16;

        private readonly RaycastHit[] _hits = new RaycastHit[MaxHits];
        private readonly Collider[] _colliders = new Collider[MaxColliders];

        private Vector3 _currentVelocity;
        private Vector3 _previousVelocity;
        private Vector3 _currentPosition;
        private Vector3 _previousPosition;
        private Vector3 _visualsLocalOffset;

        private Collider _selfCollider;

        private bool _hasPendingHit;
        private PendingHitType _pendingHitType;
        private PendingDamageMode _pendingDamageMode;
        private DamageContext _pendingDamageContext;
        private ParryContext _pendingParryContext;
        private float _pendingQueuedAtFixedTime;

        private float _flightTime;
        private float _timeToApex;
        private bool _apexWindowParticlesPlayed;
        private bool _enableTrailNextFrame;

        public bool HasPendingHit => _hasPendingHit;

        private enum PendingHitType
        {
            None,
            Damage,
            Parry
        }

        private enum PendingDamageMode
        {
            SingleLow,
            SingleApex,
            SplitLow
        }

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

        private void OnEnable()
        {
            _currentPosition = transform.position;
            _previousPosition = _currentPosition;

            if (_visuals != null)
            {
                _visuals.localPosition = _visualsLocalOffset;
            }

            ClearPendingHit();
            _flightTime = 0f;
            _timeToApex = 0f;
            _apexWindowParticlesPlayed = false;
            _enableTrailNextFrame = false;

            if (_apexWindowParticles != null)
            {
                _apexWindowParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        private void FixedUpdate()
        {
            _previousPosition = _currentPosition;
            _previousVelocity = _currentVelocity;

            if (_hasPendingHit)
            {
                _currentVelocity = Vector3.zero;
                _previousVelocity = Vector3.zero;

                transform.position = _currentPosition;
                CoinManager.Instance?.UpdateCoinPosition(this, _currentPosition);

                if (Time.fixedTime > _pendingQueuedAtFixedTime)
                {
                    ProcessPendingHit();
                }

                return;
            }

            float dt = Time.fixedDeltaTime;
            _flightTime += dt;

            TryPlayApexWindowParticles();

            _currentVelocity += Vector3.up * (_gravity * dt);

            Vector3 displacement = _currentVelocity * dt;
            float distance = displacement.magnitude;
            if (distance > Mathf.Epsilon)
            {
                Vector3 direction = displacement / distance;

                if (TryGetClosestHit(direction, distance, out RaycastHit hit))
                {
                    float moveDistance = Mathf.Max(hit.distance - CollisionSkin, 0f);
                    _currentPosition += direction * moveDistance;
                    _currentVelocity = Vector3.Reflect(_currentVelocity, hit.normal) * _bounceDamping;
                }
                else
                {
                    _currentPosition += displacement;
                }
            }

            transform.position = _currentPosition;
            CoinManager.Instance?.UpdateCoinPosition(this, _currentPosition);
        }

        private void Update()
        {
            if (_visuals == null)
            {
                return;
            }

            float alpha = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
            Vector3 interpolatedPosition = Vector3.Lerp(_previousPosition, _currentPosition, alpha);
            _visuals.localPosition = _visualsLocalOffset + (interpolatedPosition - _currentPosition);
        }

        private void LateUpdate()
        {
            if (_visuals == null)
            {
                TryEnableTrailEmission();
                return;
            }

            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                TryEnableTrailEmission();
                return;
            }

            _visuals.forward = mainCamera.transform.forward;
            TryEnableTrailEmission();
        }

        public void Launch(Vector3 playerVelocity, Vector3 flatForward)
        {
            ClearPendingHit();
            _flightTime = 0f;

            _currentVelocity = playerVelocity;
            _currentVelocity += Vector3.up * Mathf.Sqrt(_apex * -2f * _gravity);
            _currentVelocity += flatForward * 4f;
            _previousVelocity = _currentVelocity;

            float gravityMagnitude = Mathf.Abs(_gravity);
            _timeToApex = gravityMagnitude > Mathf.Epsilon
                ? Mathf.Max(_currentVelocity.y, 0f) / gravityMagnitude
                : 0f;

            ConfigureApexWindowParticles();

            _currentPosition = transform.position;
            _previousPosition = _currentPosition;
        }

        public DamageResult TakeDamage(in DamageContext context)
        {
            if (_hasPendingHit)
            {
                return new DamageResult(transform.position);
            }

            _pendingDamageContext = context;
            _pendingDamageMode = EvaluateDamageMode();
            _pendingHitType = PendingHitType.Damage;
            _pendingQueuedAtFixedTime = Time.fixedTime;
            _hasPendingHit = true;
            _currentVelocity = Vector3.zero;
            _previousVelocity = Vector3.zero;

            return new DamageResult(transform.position);
        }

        public void OnParry(in ParryContext context)
        {
            if (_hasPendingHit)
            {
                return;
            }

            _pendingParryContext = context;
            _pendingHitType = PendingHitType.Parry;
            _pendingQueuedAtFixedTime = Time.fixedTime;
            _hasPendingHit = true;
            _currentVelocity = Vector3.zero;
            _previousVelocity = Vector3.zero;
        }

        public void OnTakenFromPool()
        {
            ClearPendingHit();

            if (_trailRenderer != null)
            {
                _trailRenderer.emitting = false;
                _trailRenderer.Clear();
            }
        }

        public void OnReturnedToPool()
        {
            ClearPendingHit();
            _flightTime = 0f;
            _timeToApex = 0f;
            _apexWindowParticlesPlayed = false;
            _enableTrailNextFrame = false;
            _currentVelocity = Vector3.zero;
            _previousVelocity = Vector3.zero;

            if (_apexWindowParticles != null)
            {
                _apexWindowParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            if (_trailRenderer != null)
            {
                _trailRenderer.emitting = false;
                _trailRenderer.Clear();
            }
        }

        public void PrepareForSpawn()
        {
            _enableTrailNextFrame = false;

            if (_trailRenderer != null)
            {
                _trailRenderer.emitting = false;
                _trailRenderer.Clear();
            }
        }

        public void QueueTrailEnable()
        {
            if (_trailRenderer == null)
            {
                return;
            }

            _enableTrailNextFrame = true;
        }

        private PendingDamageMode EvaluateDamageMode()
        {
            float apexStart = _timeToApex - _apexWindowSeconds;
            float apexEnd = _timeToApex + _apexWindowSeconds;

            if (_flightTime < apexStart)
            {
                return PendingDamageMode.SingleLow;
            }

            if (_flightTime <= apexEnd)
            {
                return PendingDamageMode.SingleApex;
            }

            return PendingDamageMode.SplitLow;
        }

        private void ProcessPendingHit()
        {
            PendingHitType hitType = _pendingHitType;
            PendingDamageMode damageMode = _pendingDamageMode;
            DamageContext damageContext = _pendingDamageContext;
            ParryContext parryContext = _pendingParryContext;

            ClearPendingHit();

            if (hitType == PendingHitType.Damage)
            {
                ResolveDamageHit(in damageContext, damageMode);
            }
            else if (hitType == PendingHitType.Parry)
            {
                ResolveParryHit(in parryContext);
            }
        }

        private void ResolveDamageHit(in DamageContext context, PendingDamageMode mode)
        {
            int lowDamage = Mathf.RoundToInt(context.Damage * _lowMultiplier);
            int apexDamage = Mathf.RoundToInt(context.Damage * _apexMultiplier);

            if (mode == PendingDamageMode.SingleLow)
            {
                RedirectSingleHit(in context, lowDamage, null);
                ReleaseSelf();
                return;
            }

            if (mode == PendingDamageMode.SingleApex)
            {
                RedirectSingleHit(in context, apexDamage, null);
                ReleaseSelf();
                return;
            }

            RedirectResult firstResult = RedirectSingleHit(in context, lowDamage, null);
            if (firstResult.TargetType == RedirectResultType.Coin)
            {
                RedirectCoinOnly(in context, lowDamage, firstResult.CoinTarget);
            }
            else if (firstResult.TargetType == RedirectResultType.Enemy)
            {
                RedirectEnemyOnly(in context, lowDamage, firstResult.EnemyTarget);
            }

            ReleaseSelf();
        }

        private void ResolveParryHit(in ParryContext context)
        {
            int damage = _parryDamage * 2;
            bool hit = Physics.Raycast(transform.position, context.Direction, out RaycastHit hitInfo, 100f, _targetLayer);
            if (hit)
            {
                TryDamage(hitInfo.collider, damage, hitInfo.point, hitInfo.normal);
                HitTracerManager.Instance.DrawTracer(transform.position, hitInfo.point);
            }
            else
            {
                HitTracerManager.Instance.DrawTracer(transform.position, transform.position + context.Direction * 40f);
            }

            ReleaseSelf();
        }

        private RedirectResult RedirectSingleHit(in DamageContext context, int damage, Coin excludedCoin, Collider excludedEnemy = null)
        {
            Vector3 origin = transform.position;

            if (TryGetNearestCoin(origin, excludedCoin, out Coin bestCoin))
            {
                DamageContext redirectedContext = new DamageContext(
                    damage,
                    bestCoin.transform.position,
                    Vector3.up,
                    context.Sender,
                    context.DamageType);

                bestCoin.TakeDamage(in redirectedContext);
                HitTracerManager.Instance.DrawTracer(origin, bestCoin.transform.position);
                return RedirectResult.ToCoin(bestCoin);
            }

            if (TryGetNearestEnemy(origin, excludedEnemy, out Collider bestEnemy) && bestEnemy.TryGetComponent(out IDamageable enemy))
            {
                Vector3 normal = bestEnemy.transform.position - origin;
                normal.Normalize();

                DamageContext redirectedContext = new DamageContext(
                    damage,
                    bestEnemy.transform.position,
                    normal,
                    context.Sender,
                    context.DamageType);

                enemy.TakeDamage(in redirectedContext);
                HitTracerManager.Instance.DrawTracer(origin, bestEnemy.transform.position);
                return RedirectResult.ToEnemy(bestEnemy);
            }

            Vector3 random = Random.insideUnitSphere;
            if (random.sqrMagnitude <= Mathf.Epsilon)
            {
                random = Vector3.up;
            }
            else
            {
                random.Normalize();
            }

            if (Physics.Raycast(origin, random, out RaycastHit hitInfo, 100f, _targetLayer))
            {
                TryDamage(hitInfo.collider, damage, hitInfo.point, hitInfo.normal);
                HitTracerManager.Instance.DrawTracer(origin, hitInfo.point);
            }
            else
            {
                HitTracerManager.Instance.DrawTracer(origin, origin + random * 40f);
            }

            return RedirectResult.None;
        }

        private bool RedirectCoinOnly(in DamageContext context, int damage, Coin excludedCoin)
        {
            Vector3 origin = transform.position;
            if (!TryGetNearestCoin(origin, excludedCoin, out Coin bestCoin))
            {
                return false;
            }

            DamageContext redirectedContext = new DamageContext(
                damage,
                bestCoin.transform.position,
                Vector3.up,
                context.Sender,
                context.DamageType);

            bestCoin.TakeDamage(in redirectedContext);
            HitTracerManager.Instance.DrawTracer(origin, bestCoin.transform.position);
            return true;
        }

        private bool RedirectEnemyOnly(in DamageContext context, int damage, Collider excludedEnemy)
        {
            Vector3 origin = transform.position;
            if (!TryGetNearestEnemy(origin, excludedEnemy, out Collider bestEnemy) || !bestEnemy.TryGetComponent(out IDamageable enemy))
            {
                return false;
            }

            Vector3 normal = bestEnemy.transform.position - origin;
            normal.Normalize();

            DamageContext redirectedContext = new DamageContext(
                damage,
                bestEnemy.transform.position,
                normal,
                context.Sender,
                context.DamageType);

            enemy.TakeDamage(in redirectedContext);
            HitTracerManager.Instance.DrawTracer(origin, bestEnemy.transform.position);
            return true;
        }

        private bool TryGetNearestCoin(Vector3 origin, Coin excludedCoin, out Coin bestCoin)
        {
            bestCoin = null;

            CoinManager manager = CoinManager.Instance;
            if (manager == null)
            {
                return false;
            }

            IReadOnlyList<Coin> coins = manager.ActiveCoins;
            IReadOnlyList<Vector3> positions = manager.ActiveCoinPositions;

            float bestDistanceSqr = _checkNearestRadius * _checkNearestRadius;
            int count = coins.Count;
            for (int i = 0; i < count; i++)
            {
                Coin candidate = coins[i];
                if (candidate == null || candidate == this || candidate == excludedCoin || candidate.HasPendingHit)
                {
                    continue;
                }

                float distanceSqr = (origin - positions[i]).sqrMagnitude;
                if (distanceSqr < bestDistanceSqr)
                {
                    bestDistanceSqr = distanceSqr;
                    bestCoin = candidate;
                }
            }

            return bestCoin != null;
        }

        private bool TryGetNearestEnemy(Vector3 origin, Collider excludedEnemy, out Collider bestEnemy)
        {
            bestEnemy = null;
            float bestDistanceSqr = _checkNearestRadius * _checkNearestRadius;

            int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _checkNearestRadius, _colliders, _targetLayer);
            for (int i = 0; i < hitCount; i++)
            {
                Collider current = _colliders[i];
                if (current == null || current == _selfCollider || current == excludedEnemy)
                {
                    continue;
                }

                if (current.TryGetComponent<Coin>(out _))
                {
                    continue;
                }

                if (!current.CompareTag("Enemy"))
                {
                    continue;
                }

                Vector3 currentPosition = current.transform.position;
                float distanceSqr = (origin - currentPosition).sqrMagnitude;
                if (distanceSqr < bestDistanceSqr)
                {
                    bestDistanceSqr = distanceSqr;
                    bestEnemy = current;
                }
            }

            return bestEnemy != null;
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

        private bool TryDamage(Collider collider, int damage, Vector3 point, Vector3 normal)
        {
            if (collider.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                DamageContext damageContext = new DamageContext(
                    damage,
                    point,
                    normal,
                    DamageSender.Player,
                    DamageType.Piercing);

                damageable.TakeDamage(in damageContext);
                return true;
            }

            return false;
        }

        private void ClearPendingHit()
        {
            _hasPendingHit = false;
            _pendingHitType = PendingHitType.None;
            _pendingDamageMode = PendingDamageMode.SingleLow;
            _pendingDamageContext = default;
            _pendingParryContext = default;
            _pendingQueuedAtFixedTime = -1f;
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

        private void ConfigureApexWindowParticles()
        {
            if (_apexWindowParticles == null)
            {
                return;
            }

            float duration = Mathf.Max(_apexWindowSeconds, 0.01f);
            float lifetime = Mathf.Max(_apexWindowSeconds - _visualsEndTimeOffset, 0.01f);
            ParticleSystem.MainModule main = _apexWindowParticles.main;
            main.duration = duration;
            main.startLifetime = lifetime;
        }

        private void TryPlayApexWindowParticles()
        {
            if (_apexWindowParticles == null || _apexWindowParticlesPlayed)
            {
                return;
            }

            float apexWindowStartTime = _timeToApex - _apexWindowSeconds;
            if (_flightTime < apexWindowStartTime)
            {
                return;
            }

            _apexWindowParticlesPlayed = true;
            _apexWindowParticles.Play();
        }

        private void ReleaseSelf()
        {
            CoinManager manager = CoinManager.Instance;
            if (manager != null)
            {
                manager.ReleaseCoin(this);
                return;
            }

            Destroy(gameObject);
        }

        private readonly struct RedirectResult
        {
            public readonly RedirectResultType TargetType;
            public readonly Coin CoinTarget;
            public readonly Collider EnemyTarget;

            public RedirectResult(RedirectResultType targetType, Coin coinTarget, Collider enemyTarget)
            {
                TargetType = targetType;
                CoinTarget = coinTarget;
                EnemyTarget = enemyTarget;
            }

            public static RedirectResult None => new RedirectResult(RedirectResultType.None, null, null);

            public static RedirectResult ToCoin(Coin coin)
                => new RedirectResult(RedirectResultType.Coin, coin, null);

            public static RedirectResult ToEnemy(Collider enemy)
                => new RedirectResult(RedirectResultType.Enemy, null, enemy);
        }

        private enum RedirectResultType
        {
            None,
            Coin,
            Enemy
        }
    }
}
