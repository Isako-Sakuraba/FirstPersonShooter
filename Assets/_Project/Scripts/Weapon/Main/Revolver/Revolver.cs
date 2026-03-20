using Game.Experimental;
using Game.Movement;
using ImprovedTimers;
using UnityEngine;

namespace Game.Weapons.Main
{
    public class Revolver : WeaponBase
    {
        private static readonly int equipHash = Animator.StringToHash("Equip");
        private static readonly int fireHash = Animator.StringToHash("Fire");
        private static readonly int idleHash = Animator.StringToHash("Idle");
        private static readonly int coinThrowHash = Animator.StringToHash("CoinThrow");

        [SerializeField] private int _damage = 50;
        [SerializeField] private float _cooldown = 1f;
        [SerializeField] private AudioClip _shootSound;
        [SerializeField] private Transform _muzzle;
        [SerializeField] private ParticleSystem _muzzleFlash;
        [SerializeField] private LocomotionController _player;

        private CountdownTimer _cooldownTimer;

        public override void WeaponAwake()
        {
            _cooldownTimer = new(_cooldown);
            _cooldownTimer.Reset();
        }

        public override void OnFireHold(in FireContext context)
        {
            if (_cooldownTimer.IsRunning)
                return;

            _cooldownTimer.Start();

            weaponAnimator.Play(fireHash);
            weaponAudio.PlayOneShot(_shootSound);
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
            CoinManager manager = CoinManager.Instance;
            if (manager == null)
            {
                return;
            }

            manager.ThrowCoin(_player.transform.position + Vector3.up * 0.8f, _player.Velocity, context.Direction);
            armsAnimator.Play(coinThrowHash, 0, 0f);
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
                    DamageType.Piercing);

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
    }
}
