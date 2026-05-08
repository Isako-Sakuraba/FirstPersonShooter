using ImprovedTimers;
using UnityEngine;
using Game.Movement;

namespace Game.Weapons.Main
{
    public class SMG : WeaponBase
    {
        private const int MaxBulletAmount = 30;

        private static readonly int fireHash = Animator.StringToHash("Fire");
        private static readonly int altFireHash = Animator.StringToHash("AltFire");

        [SerializeField] private float _fireCooldown = 0.1f;
        [SerializeField] private float _altFireCooldown = 0.5f;
        [SerializeField] private float _bulletRechargePerSecond = 12f;
        [SerializeField] private float _startingBulletCharge = 30f;
        [SerializeField] private int _bulletDamage = 10;
        [SerializeField] private float _bulletSpeed = 120f;
        [SerializeField] private float _grenadeChargeSpeed = 1f;
        [SerializeField] private float _grenadeMinVelocity = 8f;
        [SerializeField] private float _grenadeMaxVelocity = 24f;
        [SerializeField] private float _grenadeUpwardBoost = 2f;
        [SerializeField] private float _maxShakeDistance = 0.015f;
        [SerializeField] private float _shakeFrequency = 20f;
        [SerializeField] private float _shakeSmoothing = 18f;
        [SerializeField] private LocomotionController _player;
        [SerializeField] private WeaponAudioCue _fireAudio = new(1f);
        [SerializeField] private WeaponAudioCue _chargeAudio = new(1f);
        [SerializeField] private WeaponAudioCue _altFireAudio = new(1f);
        [SerializeField] private Transform _muzzle;
        [SerializeField] private ParticleSystem _muzzleFlash;
        [SerializeField] private Transform _visuals;
        [SerializeField] private Grenade _grenadePrefab;
        [SerializeField] private BulletManager _bulletManager;

        private CountdownTimer _fireCooldownTimer;
        private CountdownTimer _altFireCooldownTimer;
        private Vector3 _visualsBaseLocalPosition;
        private Vector3 _currentShakeOffset;
        private float _bulletCharge;
        private float _lastBulletChargeUpdateTime;
        private float _grenadeCharge;
        private bool _isCharging;

        public int MaxAvailableBullets => MaxBulletAmount;

        public float CurrentBulletCharge
        {
            get
            {
                ApplyBulletRecharge();
                return _bulletCharge;
            }
        }

        public int CurrentBulletAmount
        {
            get
            {
                ApplyBulletRecharge();
                return Mathf.FloorToInt(_bulletCharge);
            }
        }

        public float CurrentGrenadeCharge01 => _isCharging ? Mathf.Clamp01(_grenadeCharge) : 0f;
        public bool IsGrenadeCharging => _isCharging;
        public bool IsGrenadeReady => !_isCharging && (_altFireCooldownTimer == null || !_altFireCooldownTimer.IsRunning);

        public float CurrentBulletFill01
        {
            get
            {
                ApplyBulletRecharge();
                return Mathf.Clamp01(_bulletCharge / MaxBulletAmount);
            }
        }

        public override void WeaponAwake()
        {
            _fireCooldownTimer = new(_fireCooldown);
            _fireCooldownTimer.Reset();
            _altFireCooldownTimer = new(_altFireCooldown);
            _altFireCooldownTimer.Reset();

            _bulletCharge = Mathf.Clamp(_startingBulletCharge, 0f, MaxBulletAmount);
            _lastBulletChargeUpdateTime = Time.time;

            if (_visuals != null)
            {
                _visualsBaseLocalPosition = _visuals.localPosition;
            }
        }

        public override void OnEquip()
        {
            base.OnEquip();
            ApplyBulletRecharge();
        }

        public override void OnFireHold(in FireContext context)
        {
            if (_fireCooldownTimer.IsRunning || _altFireCooldownTimer.IsRunning)
            {
                return;
            }

            ApplyBulletRecharge();
            if (_bulletCharge < 1f)
            {
                return;
            }

            if (!SpawnBullet(in context))
            {
                return;
            }

            _fireCooldownTimer.Start();
            _bulletCharge = Mathf.Max(_bulletCharge - 1f, 0f);

            weaponAnimator.Play(fireHash);
            PlayFireAudio();
            _muzzleFlash.Play();
        }

        public override void OnAltFireStart(in FireContext context)
        {
            if (_altFireCooldownTimer.IsRunning)
            {
                return;
            }

            _isCharging = true;
            _grenadeCharge = 0f;
            _currentShakeOffset = Vector3.zero;
            PlayChargeAudio();
            UpdateChargeAudioVolume();

            if (_visuals != null)
            {
                _visualsBaseLocalPosition = _visuals.localPosition;
            }
        }

        public override void OnAltFireHold(in FireContext context)
        {
            if (!_isCharging)
            {
                return;
            }

            _grenadeCharge = Mathf.Clamp01(_grenadeCharge + _grenadeChargeSpeed * Time.deltaTime);
            ApplyChargeShake();
            UpdateChargeAudioVolume();
        }

        public override void OnAltFireEnd(in FireContext context)
        {
            if (!_isCharging)
            {
                return;
            }

            float releasedCharge = _grenadeCharge;
            _isCharging = false;
            _grenadeCharge = 0f;
            _altFireCooldownTimer.Start();
            StopChargeAudio();

            weaponAnimator.Play(altFireHash);
            PlayAltFireAudio();
            SpawnGrenade(in context, releasedCharge);
            ResetVisuals();
        }

        private void OnDisable()
        {
            _isCharging = false;
            _grenadeCharge = 0f;
            StopChargeAudio();
            ResetVisuals();
        }

        private void ApplyChargeShake()
        {
            if (_visuals == null)
            {
                return;
            }

            float maxShake = _grenadeCharge * Mathf.Max(0f, _maxShakeDistance);
            float noiseTime = Time.time * Mathf.Max(0f, _shakeFrequency);
            Vector3 targetOffset = new(
                (Mathf.PerlinNoise(13.37f, noiseTime) - 0.5f) * 2f,
                (Mathf.PerlinNoise(29.91f, noiseTime) - 0.5f) * 2f,
                (Mathf.PerlinNoise(47.13f, noiseTime) - 0.5f) * 2f);

            targetOffset *= maxShake;

            float lerpFactor = 1f - Mathf.Exp(-Mathf.Max(0f, _shakeSmoothing) * Time.deltaTime);
            _currentShakeOffset = Vector3.Lerp(_currentShakeOffset, targetOffset, lerpFactor);
            _visuals.localPosition = _visualsBaseLocalPosition + _currentShakeOffset;
        }

        private void ResetVisuals()
        {
            if (_visuals == null)
            {
                return;
            }

            _currentShakeOffset = Vector3.zero;
            _visuals.localPosition = _visualsBaseLocalPosition;
        }

        private void SpawnGrenade(in FireContext context, float charge)
        {
            if (_grenadePrefab == null)
            {
                return;
            }

            Vector3 direction = context.Direction;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                direction = transform.forward;
            }

            direction.Normalize();

            Vector3 spawnPosition = _muzzle != null ? _muzzle.position : context.Position;
            Quaternion spawnRotation = Quaternion.LookRotation(direction);
            Grenade grenade = Instantiate(_grenadePrefab, spawnPosition, spawnRotation);

            float t = Mathf.Clamp01(charge);
            float speed = Mathf.Lerp(_grenadeMinVelocity, _grenadeMaxVelocity, t);
            Vector3 playerVelocity = _player != null ? _player.Velocity : Vector3.zero;
            Vector3 upwardBoost = Vector3.up * _grenadeUpwardBoost;
            grenade.Launch(direction * speed + playerVelocity + upwardBoost);
        }

        public void TickBulletRecharge()
        {
            ApplyBulletRecharge();
        }

        private bool SpawnBullet(in FireContext context)
        {
            BulletManager manager = _bulletManager != null ? _bulletManager : BulletManager.Instance;
            if (manager == null)
            {
                return false;
            }

            Vector3 direction = context.Direction;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                direction = transform.forward;
            }

            direction.Normalize();

            Vector3 spawnPosition = _muzzle != null ? _muzzle.position : context.Position;
            Vector3 velocity = direction * _bulletSpeed;
            manager.SpawnBullet(spawnPosition, velocity, _bulletDamage);
            return true;
        }

        private void ApplyBulletRecharge()
        {
            float now = Time.time;
            float elapsed = now - _lastBulletChargeUpdateTime;
            if (elapsed <= 0f)
            {
                return;
            }

            float rechargeAmount = Mathf.Max(0f, _bulletRechargePerSecond) * elapsed;
            _bulletCharge = Mathf.Clamp(_bulletCharge + rechargeAmount, 0f, MaxBulletAmount);
            _lastBulletChargeUpdateTime = now;
        }

        private void PlayFireAudio()
        {
            if (weaponAudio == null || _fireAudio.Clip == null)
            {
                return;
            }

            weaponAudio.PlayOneShot(_fireAudio.Clip, _fireAudio.ResolveVolume());
        }

        private void PlayChargeAudio()
        {
            if (weaponAudio == null || _chargeAudio.Clip == null)
            {
                return;
            }

            if (weaponAudio.isPlaying && weaponAudio.clip == _chargeAudio.Clip)
            {
                return;
            }

            weaponAudio.clip = _chargeAudio.Clip;
            weaponAudio.volume = 0f;
            weaponAudio.loop = true;
            weaponAudio.Play();
        }

        private void UpdateChargeAudioVolume()
        {
            if (weaponAudio == null || weaponAudio.clip != _chargeAudio.Clip)
            {
                return;
            }

            float targetVolume = _chargeAudio.ResolveVolume();
            weaponAudio.volume = Mathf.Clamp01(_grenadeCharge) * targetVolume;
        }

        private void StopChargeAudio()
        {
            if (weaponAudio == null)
            {
                return;
            }

            if (weaponAudio.clip != _chargeAudio.Clip)
            {
                return;
            }

            weaponAudio.Stop();
            weaponAudio.loop = false;
            weaponAudio.clip = null;
            weaponAudio.volume = 1f;
        }

        private void PlayAltFireAudio()
        {
            if (weaponAudio == null || _altFireAudio.Clip == null)
            {
                return;
            }

            weaponAudio.PlayOneShot(_altFireAudio.Clip, _altFireAudio.ResolveVolume());
        }
    }
}
