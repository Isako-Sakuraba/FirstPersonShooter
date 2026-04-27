using Game.Experimental;
using Game.Movement;
using ImprovedTimers;
using UnityEngine;

namespace Game.Weapons.Main
{
    public class Revolver : WeaponBase
    {
        private const int MaxCoinCharges = 4;

        private static readonly int fireHash = Animator.StringToHash("Fire");
        private static readonly int coinThrowHash = Animator.StringToHash("CoinThrow");

        [SerializeField] private int _damage = 50;
        [SerializeField] private float _cooldown = 1f;
        [SerializeField] private float _coinRechargePerSecond = 0.6f;
        [SerializeField] private float _startingCoinCharge = 4f;
        [SerializeField] private WeaponAudioCue _shootAudio = new(1f);
        [SerializeField] private WeaponAudioCue _coinRechargeAudio = new(1f);
        [SerializeField] private Transform _muzzle;
        [SerializeField] private ParticleSystem _muzzleFlash;
        [SerializeField] private LocomotionController _player;

        private CountdownTimer _cooldownTimer;
        private float _coinCharge;
        private float _lastCoinChargeUpdateTime;

        public int MaxAvailableCoins => MaxCoinCharges;

        public float CurrentCoinCharge
        {
            get
            {
                ApplyCoinRecharge(false);
                return _coinCharge;
            }
        }

        public int AvailableCoinCount
        {
            get
            {
                ApplyCoinRecharge(false);
                return Mathf.FloorToInt(_coinCharge);
            }
        }

        public override void WeaponAwake()
        {
            _cooldownTimer = new(_cooldown);
            _cooldownTimer.Reset();

            _coinCharge = Mathf.Clamp(_startingCoinCharge, 0f, MaxCoinCharges);
            _lastCoinChargeUpdateTime = Time.time;
        }

        public override void OnEquip()
        {
            base.OnEquip();
            ApplyCoinRecharge(false);
        }

        public override void OnFireHold(in FireContext context)
        {
            if (_cooldownTimer.IsRunning)
                return;

            _cooldownTimer.Start();

            weaponAnimator.Play(fireHash);
            PlayShootAudio();
            _muzzleFlash.Play();

            bool hit = Physics.Raycast(context.Position, context.Direction, out var hitInfo, 500f, context.LayerMask);

            if (hit)
            {
                TryDamage(hitInfo.collider, in context, hitInfo.point, hitInfo.normal, out var result);
                if (result.Point.HasValue)
                    HitTracerManager.Instance.DrawTracer(_muzzle.position, result.Point.Value);
                else
                    HitTracerManager.Instance.DrawTracer(_muzzle.position, hitInfo.point);
            }
            else
            {
                HitTracerManager.Instance.DrawTracer(_muzzle.position, context.Position + context.Direction * 100f);
            }
        }

        public override void OnAltFireStart(in FireContext context)
        {
            ApplyCoinRecharge(true);
            if (_coinCharge < 1f)
            {
                return;
            }

            CoinManager manager = CoinManager.Instance;
            if (manager == null)
            {
                return;
            }

            _coinCharge = Mathf.Max(_coinCharge - 1f, 0f);
            manager.ThrowCoin(_player.transform.position + Vector3.up * 0.8f, _player.Velocity, context.Direction);
            armsAnimator.Play(coinThrowHash, 0, 0f);
        }

        public float GetCoinFill01(int coinIndex)
        {
            ApplyCoinRecharge(false);

            if (coinIndex < 0 || coinIndex >= MaxCoinCharges)
            {
                return 0f;
            }

            return Mathf.Clamp01(_coinCharge - coinIndex);
        }

        public void TickCoinRecharge(bool playRechargeAudio)
        {
            ApplyCoinRecharge(playRechargeAudio);
        }

        private bool TryDamage(Collider collider, in FireContext ctx, Vector3 point, Vector3 normal, out DamageResult result)
        {
            result = default;
            if (collider.TryGetComponent<IDamageable>(out var damageable))
            {
                var damageContext = new DamageContext(
                    _damage,
                    point,
                    normal,
                    DamageSender.Player,
                    DamageType.Piercing,
                    DamageSource.Revolver);

                result = damageable.TakeDamage(in damageContext);
                return true;
            }
            return false;
        }

        private bool TryParry(Collider collider, in FireContext ctx)
        {
            if (collider.TryGetComponent<IParryable>(out var parryable))
            {
                var parryContext = new ParryContext(ctx.Direction);
                parryable.OnParry(parryContext);
                return true;
            }
            return false;
        }

        private void PlayShootAudio()
        {
            if (weaponAudio == null || _shootAudio.Clip == null)
            {
                return;
            }

            weaponAudio.PlayOneShot(_shootAudio.Clip, _shootAudio.ResolveVolume());
        }

        private void ApplyCoinRecharge(bool playRechargeAudio)
        {
            float now = Time.time;
            float elapsed = now - _lastCoinChargeUpdateTime;
            if (elapsed <= 0f)
            {
                return;
            }

            float previousCharge = _coinCharge;
            float rechargeAmount = Mathf.Max(0f, _coinRechargePerSecond) * elapsed;
            _coinCharge = Mathf.Clamp(previousCharge + rechargeAmount, 0f, MaxCoinCharges);
            _lastCoinChargeUpdateTime = now;

            if (!playRechargeAudio || weaponAudio == null || _coinRechargeAudio.Clip == null)
            {
                return;
            }

            int previousFullCoins = Mathf.FloorToInt(previousCharge + 0.0001f);
            int currentFullCoins = Mathf.FloorToInt(_coinCharge + 0.0001f);
            int gainedCoins = currentFullCoins - previousFullCoins;

            for (int i = 0; i < gainedCoins; i++)
            {
                weaponAudio.PlayOneShot(_coinRechargeAudio.Clip, _coinRechargeAudio.ResolveVolume());
            }
        }
    }
}
