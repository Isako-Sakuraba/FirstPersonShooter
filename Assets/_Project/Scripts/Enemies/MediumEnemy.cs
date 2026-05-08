using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Game.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class MediumEnemy : MonoBehaviour
    {
        private static readonly int isMovingHash = Animator.StringToHash("isMoving");
        private static readonly int fireHash = Animator.StringToHash("Fire");

        [SerializeField] private float _minDistanceToPlayer = 8f;
        [SerializeField] private float _maxDistanceToPlayer = 14f;
        [SerializeField] private float _retreatDistance = 7f;
        [SerializeField] private float _repathInterval = 0.15f;
        [SerializeField] private float _shootCooldown = 1.25f;
        [SerializeField, FormerlySerializedAs("_fireDuration")] private float _minFireDuration = 0.35f;
        [SerializeField] private float _maxFireDuration = 0.5f;
        [SerializeField] private float _rocketSpawnDelay = 0.2f;
        [SerializeField] private float _rocketLaunchSpeed = 28f;
        [SerializeField] private float _shootTurnSpeed = 540f;
        [SerializeField] private float _destinationSampleRadius = 3f;
        [SerializeField] private int _retreatDirectionChecks = 12;
        [SerializeField] private float _moveAnimationSpeedThreshold = 0.1f;
        [SerializeField] private Animator _animationController;
        [SerializeField] private RocketManager _rocketManager;
        [SerializeField] private Transform _rocketSpawnPoint;
        [SerializeField] private Transform _rocketSpawnDirection;

        private NavMeshAgent _agent;
        private NavMeshPath _sharedPath;
        private float _nextRepathTime;
        private float _nextShootTime;
        private float _fireUntilTime;
        private float _pendingRocketSpawnTime;
        private bool _isFiring;
        private bool _hasPendingRocketSpawn;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _sharedPath = new NavMeshPath();
            _nextShootTime = Time.time + GetRandomFireDuration();

            if (_animationController == null)
            {
                _animationController = GetComponent<Animator>();
            }
        }

        private void OnEnable()
        {
            _nextShootTime = Time.time + GetRandomFireDuration();
        }

        private void Update()
        {
            UpdatePendingRocketSpawn();

            if (!PlayerLocator.HasPlayer)
            {
                StopMovement();
                SetIsMoving(false);
                _isFiring = false;
                return;
            }

            Vector3 playerPosition = PlayerLocator.Position;
            Vector3 toPlayer = playerPosition - transform.position;
            float distanceToPlayer = toPlayer.magnitude;
            bool isTooClose = distanceToPlayer < _minDistanceToPlayer;
            bool isTooFar = distanceToPlayer > _maxDistanceToPlayer;

            UpdateFireState();
            if (_isFiring && isTooClose)
            {
                _isFiring = false;
            }

            if (_isFiring)
            {
                StopMovement();
                RotateTowards(playerPosition);
                SetIsMoving(false);
                return;
            }

            if (Time.time >= _nextRepathTime)
            {
                UpdateNavigation(playerPosition, toPlayer, isTooClose, isTooFar);
                _nextRepathTime = Time.time + Mathf.Max(0.01f, _repathInterval);
            }

            if (!isTooClose && !isTooFar)
            {
                TryShoot();
            }

            UpdateAnimatorMovement();
        }

        private void UpdateNavigation(Vector3 playerPosition, Vector3 toPlayer, bool isTooClose, bool isTooFar)
        {
            if (isTooClose)
            {
                SetRetreatDestination(playerPosition, toPlayer);
                return;
            }

            if (isTooFar)
            {
                SetDestination(playerPosition);
                return;
            }

            StopMovement();
        }

        private void SetRetreatDestination(Vector3 playerPosition, Vector3 toPlayer)
        {
            if (!IsAgentReady())
            {
                return;
            }

            if (TryGetRetreatDestination(playerPosition, toPlayer, out Vector3 retreatDestination))
            {
                _agent.isStopped = false;
                _agent.SetDestination(retreatDestination);
                return;
            }

            StopMovement();
        }

        private void SetDestination(Vector3 targetPosition)
        {
            if (!IsAgentReady())
            {
                return;
            }

            if (TrySampleNavMesh(targetPosition, out Vector3 destination))
            {
                _agent.isStopped = false;
                _agent.SetDestination(destination);
                return;
            }

            StopMovement();
        }

        private bool TrySampleNavMesh(Vector3 position, out Vector3 sampledPosition)
        {
            if (NavMesh.SamplePosition(position, out NavMeshHit navMeshHit, Mathf.Max(0.1f, _destinationSampleRadius), NavMesh.AllAreas))
            {
                sampledPosition = navMeshHit.position;
                return true;
            }

            sampledPosition = default;
            return false;
        }

        private bool TryGetRetreatDestination(Vector3 playerPosition, Vector3 toPlayer, out Vector3 retreatDestination)
        {
            retreatDestination = default;

            Vector3 awayDirection = -toPlayer;
            awayDirection.y = 0f;
            if (awayDirection.sqrMagnitude <= Mathf.Epsilon)
            {
                awayDirection = transform.forward;
            }

            awayDirection.Normalize();

            bool found = false;
            float bestDistanceToPlayerSqr = 0f;
            int directionChecks = Mathf.Max(1, _retreatDirectionChecks);

            for (int i = 0; i < directionChecks; i++)
            {
                float angle = (360f / directionChecks) * i;
                Vector3 candidateDirection = Quaternion.Euler(0f, angle, 0f) * awayDirection;
                Vector3 desiredPosition = transform.position + candidateDirection * _retreatDistance;

                if (!TrySampleNavMesh(desiredPosition, out Vector3 sampledPosition))
                {
                    continue;
                }

                if (!CanReach(sampledPosition))
                {
                    continue;
                }

                float distanceToPlayerSqr = (sampledPosition - playerPosition).sqrMagnitude;
                if (!found || distanceToPlayerSqr > bestDistanceToPlayerSqr)
                {
                    found = true;
                    bestDistanceToPlayerSqr = distanceToPlayerSqr;
                    retreatDestination = sampledPosition;
                }
            }

            return found;
        }

        private bool CanReach(Vector3 destination)
        {
            if (!IsAgentReady())
            {
                return false;
            }

            if (!_agent.CalculatePath(destination, _sharedPath))
            {
                return false;
            }

            return _sharedPath.status == NavMeshPathStatus.PathComplete;
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

        private void TryShoot()
        {
            if (Time.time < _nextShootTime)
            {
                return;
            }

            _nextShootTime = Time.time + Mathf.Max(0.01f, _shootCooldown);
            _isFiring = true;
            _fireUntilTime = Time.time + GetRandomFireDuration();
            _animationController.SetTrigger(fireHash);
            Shoot();
        }

        private float GetRandomFireDuration()
        {
            float minDuration = Mathf.Max(0.01f, Mathf.Min(_minFireDuration, _maxFireDuration));
            float maxDuration = Mathf.Max(minDuration, Mathf.Max(_minFireDuration, _maxFireDuration));
            return Random.Range(minDuration, maxDuration);
        }

        private void UpdateFireState()
        {
            if (!_isFiring)
            {
                return;
            }

            if (Time.time >= _fireUntilTime)
            {
                _isFiring = false;
            }
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
            float maxDegreesDelta = Mathf.Max(0f, _shootTurnSpeed) * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxDegreesDelta);
        }

        protected virtual void Shoot()
        {
            QueueRocketSpawn();
        }

        private void UpdateAnimatorMovement()
        {
            if (!IsAgentReady())
            {
                SetIsMoving(false);
                return;
            }

            float speedThreshold = Mathf.Max(0f, _moveAnimationSpeedThreshold);
            bool hasMoveVelocity = _agent.velocity.sqrMagnitude > speedThreshold * speedThreshold;
            bool hasPendingPath = _agent.hasPath && _agent.remainingDistance > _agent.stoppingDistance;
            bool isMoving = !_agent.isStopped && (hasMoveVelocity || hasPendingPath);
            SetIsMoving(isMoving);
        }

        private bool IsAgentReady()
        {
            return _agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh;
        }

        private void SetIsMoving(bool isMoving)
        {
            _animationController.SetBool(isMovingHash, isMoving);
        }

        private void OnDisable()
        {
            if (_agent != null)
            {
                StopMovement();
            }

            if (_animationController != null)
            {
                SetIsMoving(false);
            }

            _isFiring = false;
            _hasPendingRocketSpawn = false;
        }

        private void QueueRocketSpawn()
        {
            _pendingRocketSpawnTime = Time.time + Mathf.Max(0f, _rocketSpawnDelay);
            _hasPendingRocketSpawn = true;
        }

        private void UpdatePendingRocketSpawn()
        {
            if (!_hasPendingRocketSpawn)
            {
                return;
            }

            if (Time.time < _pendingRocketSpawnTime)
            {
                return;
            }

            _hasPendingRocketSpawn = false;
            SpawnRocket();
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
    }
}
