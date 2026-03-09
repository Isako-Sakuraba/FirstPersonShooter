using UnityEngine;

namespace Game.Weapons.Experimental
{
    public class ExperimentalParryWeapon : MonoBehaviour, IWeapon
    {
        [SerializeField] private LayerMask _layermask;

        private Collider[] _cache = new Collider[3];

        public void OnFireStart(in FireContext context)
        {
            var position = context.Direction.position + context.Direction.forward;
            int hits = Physics.OverlapSphereNonAlloc(position, 1.2f, _cache, _layermask);
            if (hits == 0)
                return;

            for (int i = 0; i < hits; i++)
            {
                var collider = _cache[i];
                if (collider.TryGetComponent<IParryable>(out var parryable))
                {
                    var parryContext = new ParryContext(context.Direction.forward);
                    parryable.OnParry(parryContext);
                }
            }
            
        }

        public void OnFireEnd(in FireContext context) { }
        public void OnFireHold(in FireContext context) { }

        public void OnAltFireEnd(in FireContext context) { }
        public void OnAltFireHold(in FireContext context) { }
        public void OnAltFireStart(in FireContext context) { }

        public void OnReload() { }
    }
}