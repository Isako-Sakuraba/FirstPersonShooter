using ImprovedTimers;
using UnityEngine;

namespace Game.Weapons.Main
{
    public class Shotgun : WeaponBase
    {
        private const int MaxPumpLevel = 3;

        private static readonly int fireHash = Animator.StringToHash("Fire");
        private static readonly int pumpHash = Animator.StringToHash("AltFire");

        [SerializeField] private float _pumpDelay = 0.25f;
        [SerializeField] private float _projectileBoostWindow = 0.2f;
        [SerializeField] private float _projectileBoostExplosionRadius = 4f;
        [SerializeField] private int _projectileBoostExplosionDamage = 40;
        [SerializeField] private float _bulletSpeed = 90f;
        [SerializeField] private WeaponAudioCue _fireAudio = new(1f);
        [SerializeField] private WeaponAudioCue _pumpAudio = new(1f);
        [SerializeField] private Transform _muzzle;
        [SerializeField] private ParticleSystem _muzzleFlash;
        [SerializeField] private BulletManager _bulletManager;
        [SerializeField] private PumpLevelConfig _pumpLevel1 = new PumpLevelConfig(6, 8, 0.03f);
        [SerializeField] private PumpLevelConfig _pumpLevel2 = new PumpLevelConfig(10, 7, 0.07f);
        [SerializeField] private PumpLevelConfig _pumpLevel3 = new PumpLevelConfig(16, 6, 0.12f);

        private CountdownTimer _pumpDelayTimer;
        private float _projectileBoostUntilTime;
        private bool _isProjectileBoostAvailable;
        private Bullet _projectileBoostCandidate;
        private int _pumpLevel;

        public override void WeaponAwake()
        {
            _pumpDelayTimer = new(Mathf.Max(0f, _pumpDelay));
            _pumpDelayTimer.Reset();
        }

        public override void OnEquip()
        {
            base.OnEquip();
            _pumpLevel = 0;
            _pumpDelayTimer.Reset();
            _isProjectileBoostAvailable = false;
            _projectileBoostCandidate = null;
        }

        public override void OnAltFireStart(in FireContext context)
        {
            if (_pumpLevel >= MaxPumpLevel || _pumpDelayTimer.IsRunning)
            {
                return;
            }

            _pumpLevel++;
            _pumpDelayTimer.Start();
            weaponAnimator.Play(pumpHash, -1, 0f);
            PlayPumpAudio();
        }

        public override void OnFireStart(in FireContext context)
        {
            if (_pumpLevel <= 0)
            {
                return;
            }

            BulletManager manager = _bulletManager != null ? _bulletManager : BulletManager.Instance;
            if (manager == null)
            {
                return;
            }

            PumpLevelConfig config = GetCurrentPumpConfig();
            int bulletCount = Mathf.Max(0, config.BulletCount);
            int bulletDamage = Mathf.Max(0, config.BulletDamage);
            float sprayRadius = Mathf.Max(0f, config.SprayRadius);

            Vector3 baseDirection = context.Direction;
            if (baseDirection.sqrMagnitude <= Mathf.Epsilon)
            {
                baseDirection = transform.forward;
            }

            baseDirection.Normalize();
            Quaternion spreadBasis = Quaternion.LookRotation(baseDirection);

            Vector3 spawnPosition = _muzzle != null ? _muzzle.position : context.Position;

            Bullet straightBullet = null;
            if (bulletCount > 0)
            {
                Vector3 straightVelocity = baseDirection * _bulletSpeed;
                straightBullet = manager.SpawnBullet(spawnPosition, straightVelocity, bulletDamage);
            }

            for (int i = 1; i < bulletCount; i++)
            {
                Vector2 offset = Random.insideUnitCircle * sprayRadius;
                Vector3 localSpreadDirection = new Vector3(offset.x, offset.y, 1f).normalized;
                Vector3 bulletDirection = (spreadBasis * localSpreadDirection).normalized;
                Vector3 bulletVelocity = bulletDirection * _bulletSpeed;

                manager.SpawnBullet(spawnPosition, bulletVelocity, bulletDamage);
            }

            _pumpLevel = 0;
            _projectileBoostUntilTime = Time.time + Mathf.Max(0f, _projectileBoostWindow);
            _isProjectileBoostAvailable = straightBullet != null;
            _projectileBoostCandidate = straightBullet;
            weaponAnimator.Play(fireHash);
            _muzzleFlash.Play();
            PlayFireAudio();
        }

        public bool TryTriggerProjectileBoost()
        {
            if (!_isProjectileBoostAvailable)
            {
                return false;
            }

            if (Time.time > _projectileBoostUntilTime)
            {
                _isProjectileBoostAvailable = false;
                return false;
            }

            if (_projectileBoostCandidate == null || !_projectileBoostCandidate.IsActive)
            {
                _isProjectileBoostAvailable = false;
                _projectileBoostCandidate = null;
                return false;
            }

            _projectileBoostCandidate.MakeExplosive(_projectileBoostExplosionRadius, _projectileBoostExplosionDamage);

            _isProjectileBoostAvailable = false;
            _projectileBoostCandidate = null;
            return true;
        }

        private PumpLevelConfig GetCurrentPumpConfig()
        {
            if (_pumpLevel >= 3)
            {
                return _pumpLevel3;
            }

            if (_pumpLevel == 2)
            {
                return _pumpLevel2;
            }

            return _pumpLevel1;
        }

        private void PlayFireAudio()
        {
            if (weaponAudio == null || _fireAudio.Clip == null)
            {
                return;
            }

            weaponAudio.PlayOneShot(_fireAudio.Clip, _fireAudio.ResolveVolume());
        }

        private void PlayPumpAudio()
        {
            if (weaponAudio == null || _pumpAudio.Clip == null)
            {
                return;
            }

            weaponAudio.PlayOneShot(_pumpAudio.Clip, _pumpAudio.ResolveVolume());
        }

        [System.Serializable]
        private struct PumpLevelConfig
        {
            public int BulletCount;
            public int BulletDamage;
            public float SprayRadius;

            public PumpLevelConfig(int bulletCount, int bulletDamage, float sprayRadius)
            {
                BulletCount = bulletCount;
                BulletDamage = bulletDamage;
                SprayRadius = sprayRadius;
            }
        }
    }
}
