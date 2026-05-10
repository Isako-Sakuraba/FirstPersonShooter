using UnityEngine;
using UnityEngine.AI;
using Game.Weapons;

namespace Game.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class SmallEnemy : MonoBehaviour
    {
        private static readonly int isWalkingHash = Animator.StringToHash("isWalking");
        private static readonly int attackHash = Animator.StringToHash("Attack");

        [Header("Movement")]
        [SerializeField] private float _chaseRange = 40f;
        [SerializeField] private float _attackRange = 1.8f;
        [SerializeField] private float _repathInterval = 0.12f;
        [SerializeField] private float _turnSpeed = 640f;
        [SerializeField] private float _destinationSampleRadius = 2.5f;

        [Header("Attack")]
        [SerializeField] private int _attackDamage = 18;
        [SerializeField] private float _attackCooldown = 0.9f;
        [SerializeField] private float _attackDuration = 0.35f;
        [SerializeField] private float _attackRadius = 0.9f;
        [SerializeField] private float _attackForwardOffset = 1f;
        [SerializeField] private LayerMask _attackLayer = Physics.DefaultRaycastLayers;

        [Header("References")]
        [SerializeField] private Animator _animationController;

        private readonly Collider[] _attackHits = new Collider[16];

        private NavMeshAgent _agent;
        private float _nextRepathTime;
        private float _nextAttackTime;
        private float _attackUntilTime;
        private bool _isAttacking;

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
            if (!PlayerLocator.HasPlayer)
            {
                StopMovement();
                SetIsWalking(false);
                _isAttacking = false;
                return;
            }

            Vector3 playerPosition = PlayerLocator.Position;
            Vector3 toPlayer = playerPosition - transform.position;
            float distanceToPlayer = toPlayer.magnitude;

            UpdateAttackState();

            if (_isAttacking)
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

            if (distanceToPlayer <= _attackRange)
            {
                StopMovement();
                RotateTowards(playerPosition);
                TryAttack();
                SetIsWalking(false);
                return;
            }

            if (Time.time >= _nextRepathTime)
            {
                SetDestination(playerPosition);
                _nextRepathTime = Time.time + Mathf.Max(0.01f, _repathInterval);
            }

            RotateTowards(playerPosition);
            UpdateAnimatorMovement();
        }

        private void TryAttack()
        {
            if (Time.time < _nextAttackTime)
            {
                return;
            }

            _nextAttackTime = Time.time + Mathf.Max(0.01f, _attackCooldown);
            _attackUntilTime = Time.time + Mathf.Max(0.01f, _attackDuration);
            _isAttacking = true;

            if (_animationController != null)
            {
                _animationController.SetTrigger(attackHash);
            }

            PerformMeleeAttack();
        }

        private void PerformMeleeAttack()
        {
            Vector3 center = transform.position + transform.forward * _attackForwardOffset;
            int hits = Physics.OverlapSphereNonAlloc(center, _attackRadius, _attackHits, _attackLayer, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hits; i++)
            {
                Collider hit = _attackHits[i];
                if (hit == null)
                {
                    continue;
                }

                if (!hit.TryGetComponent<IDamageable>(out IDamageable damageable))
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
                    _attackDamage,
                    point,
                    normal,
                    DamageSender.Enemy,
                    DamageType.Melee);

                damageable.TakeDamage(in context);
            }
        }

        private void UpdateAttackState()
        {
            if (_isAttacking && Time.time >= _attackUntilTime)
            {
                _isAttacking = false;
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

            bool isWalking = !_agent.isStopped && (_agent.velocity.sqrMagnitude > 0.01f || (_agent.hasPath && _agent.remainingDistance > _agent.stoppingDistance));
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
            _isAttacking = false;
            SetIsWalking(false);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector3 center = transform.position + transform.forward * _attackForwardOffset;
            Gizmos.DrawWireSphere(center, _attackRadius);
        }
    }
}
