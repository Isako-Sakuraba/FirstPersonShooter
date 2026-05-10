using UnityEngine;
using Game.Player;

namespace Game.Weapons.Experimental
{
    public class Fists : WeaponBase
    {
        private const float HitDistance = 2.8f;
        private const float ParryRadius = 0.24f;
        private const int Damage = 20;

        [SerializeField] private int _healthRestoreOnParry = 5;

        private RaycastHit _hit;
        private Collider[] _cache = new Collider[1];

        public override void OnFireStart(in FireContext context)
        {
            armsAnimator.Play("Punch");
            if (Physics.Raycast(context.Position, context.Direction, out _hit, HitDistance, context.LayerMask))
            {
                if (!TryParry(_hit.collider, in context))
                    TryDamage(_hit.collider, in context, _hit.point, _hit.normal);

                return;
            }
            int hits = Physics.OverlapCapsuleNonAlloc(context.Position, context.Position + context.Direction * HitDistance, ParryRadius, _cache, context.LayerMask);
            if (hits > 0)
            {
                TryParry(_cache[0], in context);
            }
        }

        private bool TryParry(Collider collider, in FireContext ctx)
        {
            if (collider.TryGetComponent<IParryable>(out var parryable))
            {
                armsAnimator.Play("Parry", -1, 0.17f);
                var parryContext = new ParryContext(ctx.Direction);
                parryable.OnParry(parryContext);
                RestoreHealthOnParry();
                return true;
            }
            return false;
        }

        private void RestoreHealthOnParry()
        {
            if (_healthRestoreOnParry <= 0)
            {
                return;
            }

            PlayerHealth playerHealth = PlayerHealth.Instance;
            if (playerHealth == null)
            {
                return;
            }

            playerHealth.RestoreHealth(_healthRestoreOnParry);
        }

        private bool TryDamage(Collider collider, in FireContext ctx, Vector3 point, Vector3 normal)
        {
            if (collider.TryGetComponent<IDamageable>(out var damageable))
            {
                armsAnimator.Play("Punch", -1, 0.17f);
                var damageContext = new DamageContext(
                    Damage,
                    point,
                    normal,
                    DamageSender.Player,
                    DamageType.Melee);

                damageable.TakeDamage(in damageContext);
                return true;
            }
            return false;
        }
    }
}
