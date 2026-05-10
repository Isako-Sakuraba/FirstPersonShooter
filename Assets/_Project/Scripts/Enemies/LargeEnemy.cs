using UnityEngine;
using UnityEngine.AI;
using Game.Weapons;

namespace Game.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class LargeEnemy : MonoBehaviour
    {
        private static readonly int isWalkingHash = Animator.StringToHash("isWalking");
        private static readonly int attackHash = Animator.StringToHash("Attack");
        private static readonly int fireHash = Animator.StringToHash("Fire");

        [Header("Movement")]
        [SerializeField] private float _chaseRange = 60f;
        [SerializeField] private float _stopDistanceToPlayer = 12f;
        [SerializeField] private float _repathInterval = 0.15f;
        [SerializeField] private float _turnSpeed = 520f;
        [SerializeField] private float _destinationSampleRadius = 3f;

        [Header("Melee")]
        [SerializeField] private int _meleeDamage = 22;
        [SerializeField] private float _meleeRange = 2.2f;
        [SerializeField] private float _meleeCooldown = 1.2f;
        [SerializeField] private float _meleeDuration = 0.45f;
        [SerializeField] private float _meleeHitDelay = 0.15f;
        [SerializeField] private float _meleeRadius = 1.1f;
        [SerializeField] private float _meleeForwardOffset = 1.2f;
        [SerializeField] private LayerMask _meleeLayer = Physics.DefaultRaycastLayers;

        [Header("Rockets")]
        [SerializeField] private RocketManager _rocketManager;
        [SerializeField] private Transform _rocketSpawnPoint;
        [SerializeField] private Transform _rocketSpawnDirection;
        [SerializeField] private float _rocketLaunchSpeed = 26f;
        [SerializeField] private float _rocketBurstCooldown = 3f;
        [SerializeField] private float _fireDuration = 0.6f;
        [SerializeField] private float _firstRocketSpawnDelay = 0.25f;
        [SerializeField] private float _betweenRocketsDelay = 0.12f;

        [Header("References")]
        [SerializeField] private Animator _animationController;

        private readonly Collider[] _meleeHits = new Collider[16];

        private NavMeshAgent _agent;
        private float _nextRepathTime;
        private float _nextMeleeTime;
        private float _nextRocketBurstTime;
        private float _meleeUntilTime;
        private float _fireUntilTime;
        private float _nextRocketSpawnTime;
        private float _pendingMeleeHitTime;
        private int _pendingRockets;
        private bool _isMeleeAttacking;
        private bool _isFiring;
        private bool _hasPendingMeleeHit;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();

            if (_animationController == null)
            {
                _animationController = GetComponent<Animator>();
            }
        }

        private void Update()
        {
            UpdatePendingMeleeHit();
            UpdateRocketBurstQueue();

            if (!PlayerLocator.HasPlayer)
            {
                StopMovement();
                SetIsWalking(false);
                _isMeleeAttacking = false;
                _isFiring = false;
                return;
            }

            Vector3 playerPosition = PlayerLocator.Position;
            Vector3 toPlayer = playerPosition - transform.position;
            float distanceToPlayer = toPlayer.magnitude;

            UpdateActionStates();

            if (_isMeleeAttacking || _isFiring)
            {
                StopMovement();
                RotateTowards(playerPosition);
                SetIsWalking(false);
                return;
            }

            if (distanceToPlayer > _chaseRange)
            {
                StopMovement();
                SetIsWalking(false);
                return;
            }

            if (distanceToPlayer <= _meleeRange)
            {
                StopMovement();
                RotateTowards(playerPosition);
                TryMeleeAttack();
                SetIsWalking(false);
                return;
            }

            if (distanceToPlayer > _stopDistanceToPlayer)
            {
                if (Time.time >= _nextRepathTime)
                {
                    SetDestination(playerPosition);
                    _nextRepathTime = Time.time + Mathf.Max(0.01f, _repathInterval);
                }

                RotateTowards(playerPosition);
                UpdateAnimatorMovement();
                return;
            }

            StopMovement();
            RotateTowards(playerPosition);
            SetIsWalking(false);
            TryStartRocketBurst();
        }

        private void TryMeleeAttack()
        {
            if (Time.time < _nextMeleeTime)
            {
                return;
            }

            _nextMeleeTime = Time.time + Mathf.Max(0.01f, _meleeCooldown);
            _isMeleeAttacking = true;
            _meleeUntilTime = Time.time + Mathf.Max(0.01f, _meleeDuration);
            _pendingMeleeHitTime = Time.time + Mathf.Max(0f, _meleeHitDelay);
            _hasPendingMeleeHit = true;

            if (_animationController != null)
            {
                _animationController.SetTrigger(attackHash);
                _animationController.Play(attackHash, -1, 0f);
            }
        }

        private void UpdatePendingMeleeHit()
        {
            if (!_hasPendingMeleeHit)
            {
                return;
            }

            if (Time.time < _pendingMeleeHitTime)
            {
                return;
            }

            _hasPendingMeleeHit = false;
            PerformMeleeAttack();
        }

        private void PerformMeleeAttack()
        {
            Vector3 center = transform.position + transform.forward * _meleeForwardOffset;
            int hits = Physics.OverlapSphereNonAlloc(center, _meleeRadius, _meleeHits, _meleeLayer, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hits; i++)
            {
                Collider hit = _meleeHits[i];
                if (hit == null || !hit.TryGetComponent<IDamageable>(out IDamageable damageable))
                {
                    continue;
                }

                Vector3 point = hit.ClosestPoint(center);
                Vector3 normal = point - center;
                if (normal.sqrMagnitude <= Mathf.Epsilon)
                {
                    normal = transform.forward;
                }
                else
                {
                    normal.Normalize();
                }

                DamageContext context = new(
                    _meleeDamage,
                    point,
                    normal,
                    DamageSender.Enemy,
                    DamageType.Melee);

                damageable.TakeDamage(in context);
            }
        }

        private void TryStartRocketBurst()
        {
            if (Time.time < _nextRocketBurstTime)
            {
                return;
            }

            _nextRocketBurstTime = Time.time + Mathf.Max(0.01f, _rocketBurstCooldown);
            _isFiring = true;
            _fireUntilTime = Time.time + Mathf.Max(0.01f, _fireDuration);
            _pendingRockets = 3;
            _nextRocketSpawnTime = Time.time + Mathf.Max(0f, _firstRocketSpawnDelay);

            if (_animationController != null)
            {
                _animationController.SetTrigger(fireHash);
            }
        }

        private void UpdateRocketBurstQueue()
        {
            if (_pendingRockets <= 0)
            {
                return;
            }

            if (Time.time < _nextRocketSpawnTime)
            {
                return;
            }

            SpawnRocket();
            _pendingRockets--;

            if (_pendingRockets > 0)
            {
                _nextRocketSpawnTime = Time.time + Mathf.Max(0f, _betweenRocketsDelay);
            }
        }

        private void SpawnRocket()
        {
            RocketManager manager = _rocketManager != null ? _rocketManager : RocketManager.Instance;
            if (manager == null)
            {
                return;
            }

            Vector3 spawnPosition = _rocketSpawnPoint != null ? _rocketSpawnPoint.position : transform.position;
            Vector3 direction = _rocketSpawnDirection != null ? _rocketSpawnDirection.forward : transform.forward;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                direction = transform.forward;
            }

            direction.Normalize();
            Vector3 velocity = direction * Mathf.Max(0f, _rocketLaunchSpeed);
            manager.SpawnRocket(spawnPosition, velocity);
        }

        private void UpdateActionStates()
        {
            if (_isMeleeAttacking && Time.time >= _meleeUntilTime)
            {
                _isMeleeAttacking = false;
            }

            if (_isFiring && Time.time >= _fireUntilTime && _pendingRockets <= 0)
            {
                _isFiring = false;
            }
        }

        private void SetDestination(Vector3 targetPosition)
        {
            if (!IsAgentReady())
            {
                return;
            }

            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit navMeshHit, Mathf.Max(0.1f, _destinationSampleRadius), NavMesh.AllAreas))
            {
                _agent.isStopped = false;
                _agent.SetDestination(navMeshHit.position);
                return;
            }

            StopMovement();
        }

        private void RotateTowards(Vector3 worldTarget)
        {
            Vector3 toTarget = worldTarget - transform.position;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(toTarget.normalized);
            float maxDegreesDelta = Mathf.Max(0f, _turnSpeed) * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxDegreesDelta);
        }

        private void StopMovement()
        {
            if (!IsAgentReady())
            {
                return;
            }

            _agent.isStopped = true;
            _agent.ResetPath();
        }

        private void UpdateAnimatorMovement()
        {
            if (!IsAgentReady())
            {
                SetIsWalking(false);
                return;
            }

            bool isWalking = !_agent.isStopped &&
                             (_agent.velocity.sqrMagnitude > 0.01f ||
                              (_agent.hasPath && _agent.remainingDistance > _agent.stoppingDistance));
            SetIsWalking(isWalking);
        }

        private void SetIsWalking(bool isWalking)
        {
            if (_animationController != null)
            {
                _animationController.SetBool(isWalkingHash, isWalking);
            }
        }

        private bool IsAgentReady()
        {
            return _agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh;
        }

        private void OnDisable()
        {
            StopMovement();
            _isMeleeAttacking = false;
            _isFiring = false;
            _hasPendingMeleeHit = false;
            _pendingRockets = 0;
            SetIsWalking(false);
        }
    }
}
