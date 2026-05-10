using System.Collections.Generic;
using ECM2;
using UnityEngine;
using Game.Movement;
using Game.Systems;
using Game.Weapons;

namespace Game.Experimental
{
    public class ExperimentalExplosionManager : MonoBehaviour
    {
        [System.Serializable]
        private struct RadiusParticleCount
        {
            [Min(0f)] public float Radius;
            [Min(0)] public int Count;

            public RadiusParticleCount(float radius, int count)
            {
                Radius = radius;
                Count = count;
            }
        }

        private const int DefaultMaxOverlapResults = 64;

        [SerializeField] private LayerMask _damageableLayer = Physics.DefaultRaycastLayers;
        [SerializeField] private ParticleSystem _explosionParticlesPrefab;
        [SerializeField] private WeaponAudioCue _explosionAudio = new(1f);
        [SerializeField] private Transform _effectsRoot;
        [SerializeField] private int _maxOverlapResults = DefaultMaxOverlapResults;
        [SerializeField] private float _particleLifetime = 3f;
        [SerializeField] private float _playerLaunchForce = 18f;
        [SerializeField] private RadiusParticleCount _burstAtSmallRadius = new(1f, 24);
        [SerializeField] private RadiusParticleCount _burstAtLargeRadius = new(4f, 80);

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

        public void CauseExplosion(Vector3 point, float radius, int damage, DamageSender sender = DamageSender.Player)
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
                    sender,
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

            int burstCount = ResolveBurstCount(explosionRadius);
            ParticleSystem[] allParticles = particles.GetComponentsInChildren<ParticleSystem>(true);
            int particleSystemsCount = allParticles.Length;
            for (int i = 0; i < particleSystemsCount; i++)
            {
                ParticleSystem particleSystem = allParticles[i];
                ApplyShapeRadius(particleSystem, explosionRadius);
                ApplyBurstCount(particleSystem, burstCount);
            }

            particles.Play();
            Destroy(particles.gameObject, _particleLifetime);
        }

        private int ResolveBurstCount(float radius)
        {
            float safeRadius = Mathf.Max(0f, radius);

            float minRadius = _burstAtSmallRadius.Radius;
            float maxRadius = _burstAtLargeRadius.Radius;
            int minCount = Mathf.Max(0, _burstAtSmallRadius.Count);
            int maxCount = Mathf.Max(0, _burstAtLargeRadius.Count);

            if (Mathf.Abs(maxRadius - minRadius) <= Mathf.Epsilon)
            {
                return Mathf.Max(minCount, maxCount);
            }

            if (maxRadius < minRadius)
            {
                float swapRadius = minRadius;
                minRadius = maxRadius;
                maxRadius = swapRadius;

                int swapCount = minCount;
                minCount = maxCount;
                maxCount = swapCount;
            }

            float t = Mathf.InverseLerp(minRadius, maxRadius, safeRadius);
            return Mathf.Max(0, Mathf.RoundToInt(Mathf.Lerp(minCount, maxCount, t)));
        }

        private void ApplyShapeRadius(ParticleSystem particles, float radius)
        {
            if (particles == null)
            {
                return;
            }

            ParticleSystem.ShapeModule shape = particles.shape;
            if (!shape.enabled)
            {
                return;
            }

            shape.radius = Mathf.Max(0f, radius);
        }

        private void ApplyBurstCount(ParticleSystem particles, int targetCount)
        {
            if (particles == null)
            {
                return;
            }

            ParticleSystem.EmissionModule emission = particles.emission;
            int burstsCount = emission.burstCount;
            if (burstsCount <= 0)
            {
                return;
            }

            short burstValue = (short)Mathf.Clamp(targetCount, 0, short.MaxValue);
            for (int i = 0; i < burstsCount; i++)
            {
                ParticleSystem.Burst burst = emission.GetBurst(i);
                burst.minCount = burstValue;
                burst.maxCount = burstValue;
                emission.SetBurst(i, burst);
            }
        }

        private void PlayExplosionAudio(Vector3 point)
        {
            if (_explosionAudio.Clip == null)
            {
                return;
            }

            float volume = _explosionAudio.ResolveVolume() * VolumeSlider.SfxVolume01;
            AudioSource.PlayClipAtPoint(_explosionAudio.Clip, point, Mathf.Clamp01(volume));
        }
    }
}
