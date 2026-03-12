using UnityEngine;

namespace Game.Weapons.Experimental
{
    public class ExperimentalParryWeapon : MonoBehaviour, IWeapon
    {
        private const float HitDistance = 2.8f;
        private const float ParryRadius = 0.24f;
        private const int Damage = 20;

        public Animator Animator;

        private RaycastHit _hit;
        private Collider[] _cache = new Collider[1];

        public void OnFireStart(in FireContext context)
        {
            Animator.Play("Punch");
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
                Animator.Play("Parry", -1, 0.17f);
                var parryContext = new ParryContext(ctx.Direction);
                parryable.OnParry(parryContext);
                return true;
            }
            return false;
        }

        private bool TryDamage(Collider collider, in FireContext ctx, Vector3 point, Vector3 normal)
        {
            if (collider.TryGetComponent<IDamageable>(out var damageable))
            {
                Animator.Play("Punch", -1, 0.17f);
                damageable.TakeDamage(Damage, point, normal);
                return true;
            }
            return false;
        }

        public void OnFireEnd(in FireContext context) { }
        public void OnFireHold(in FireContext context) { }

        public void OnAltFireEnd(in FireContext context) { }
        public void OnAltFireHold(in FireContext context) { }
        public void OnAltFireStart(in FireContext context) { }

        public void OnReload() { }
    }
}