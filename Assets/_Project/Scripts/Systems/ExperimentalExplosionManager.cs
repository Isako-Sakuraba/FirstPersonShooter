using System.Collections.Generic;
using ECM2;
using UnityEngine;
using Game.Movement;
using Game.Weapons;

namespace Game.Experimental
{
    public class ExperimentalExplosionManager : MonoBehaviour
    {
        private const int DefaultMaxOverlapResults = 64;

        [SerializeField] private LayerMask _damageableLayer = Physics.DefaultRaycastLayers;
        [SerializeField] private ParticleSystem _explosionParticlesPrefab;
        [SerializeField] private WeaponAudioCue _explosionAudio = new(1f);
        [SerializeField] private Transform _effectsRoot;
        [SerializeField] private int _maxOverlapResults = DefaultMaxOverlapResults;
        [SerializeField] private float _particleLifetime = 3f;
        [SerializeField] private float _playerLaunchForce = 18f;

        private static ExperimentalExplosionManager _instance;
        public static ExperimentalExplosionManager Instance => _instance;

        private readonly HashSet<IDamageable> _damagedTargets = new();
        private readonly HashSet<LocomotionController> _launchedPlayers = new();
        private Collider[] _overlapResults;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _overlapResults = new Collider[Mathf.Max(1, _maxOverlapResults)];
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        public void CauseExplosion(Vector3 point, float radius, int damage)
        {
            float safeRadius = Mathf.Max(0f, radius);
            SpawnExplosionParticles(point, safeRadius);
            PlayExplosionAudio(point);

            int hitCount = Physics.OverlapSphereNonAlloc(
                point,
                safeRadius,
                _overlapResults,
                _damageableLayer,
                QueryTriggerInteraction.Ignore);

            _damagedTargets.Clear();
            _launchedPlayers.Clear();

            for (int i = 0; i < hitCount; i++)
            {
                Collider hitCollider = _overlapResults[i];
                if (hitCollider == null)
                {
                    continue;
                }

                TryLaunchPlayer(hitCollider, point);

                if (!hitCollider.TryGetComponent<IDamageable>(out IDamageable damageable))
                {
                    continue;
                }

                if (!_damagedTargets.Add(damageable))
                {
                    continue;
                }

                Vector3 hitPoint = hitCollider.ClosestPoint(point);
                Vector3 normal = hitPoint - point;
                if (normal.sqrMagnitude <= Mathf.Epsilon)
                {
                    normal = Vector3.up;
                }
                else
                {
                    normal.Normalize();
                }

                DamageContext damageContext = new(
                    damage,
                    hitPoint,
                    normal,
                    DamageSender.Player,
                    DamageType.Explosion);

                damageable.TakeDamage(in damageContext);
            }
        }

        private void TryLaunchPlayer(Collider hitCollider, Vector3 explosionPoint)
        {
            LocomotionController locomotionController = hitCollider.GetComponentInParent<LocomotionController>();
            if (locomotionController == null)
            {
                return;
            }

            if (!_launchedPlayers.Add(locomotionController))
            {
                return;
            }

            if (!locomotionController.TryGetComponent<CharacterMovement>(out CharacterMovement characterMovement))
            {
                return;
            }

            Vector3 launchDirection = locomotionController.transform.position - explosionPoint;
            if (launchDirection.sqrMagnitude <= Mathf.Epsilon)
            {
                launchDirection = Vector3.up;
            }
            else
            {
                launchDirection.Normalize();
            }

            characterMovement.LaunchCharacter(launchDirection * Mathf.Max(0f, _playerLaunchForce), false, false);
            characterMovement.PauseGroundConstraint();
        }

        private void SpawnExplosionParticles(Vector3 point, float explosionRadius)
        {
            if (_explosionParticlesPrefab == null)
            {
                return;
            }

            Transform parent = _effectsRoot != null ? _effectsRoot : transform;
            ParticleSystem particles = Instantiate(_explosionParticlesPrefab, point, Quaternion.identity, parent);
            ParticleSystem.MainModule main = particles.main;
            main.startSize = explosionRadius * 2f;
            particles.Play();
            Destroy(particles.gameObject, _particleLifetime);
        }

        private void PlayExplosionAudio(Vector3 point)
        {
            if (_explosionAudio.Clip == null)
            {
                return;
            }

            AudioSource.PlayClipAtPoint(_explosionAudio.Clip, point, _explosionAudio.ResolveVolume());
        }
    }
}
