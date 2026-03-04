using UnityEngine;

namespace Game.Weapons.Experimental
{
    public sealed class ExperimentalWeapon : MonoBehaviour
    {
        [Header("Raycast")]
        [SerializeField] private Transform _muzzle;
        [SerializeField] private float _range = 100f;
        [SerializeField] private LayerMask _hitMask = ~0;

        [Header("Damage")]
        [SerializeField] private int _damage = 10;

        private InputService _inputService;

        private void Awake()
        {
            _inputService = InputService.Instance;
        }

        private void Update()
        {
            if (_inputService.FirePressed)
                Fire();
        }

        public void Fire()
        {
            var origin = (_muzzle != null) ? _muzzle.position : transform.position;
            var direction = (_muzzle != null) ? _muzzle.forward : transform.forward;

            if (!Physics.Raycast(origin, direction, out var hit, _range, _hitMask))
                return;

            if (hit.collider.TryGetComponent<IDamageable>(out var damageable) ||
                hit.collider.GetComponentInParent<IDamageable>() is { } parentDamageable && (damageable = parentDamageable) != null)
            {
                damageable.TakeDamage(_damage, hit.point, hit.normal);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            var origin = (_muzzle != null) ? _muzzle.position : transform.position;
            var direction = (_muzzle != null) ? _muzzle.forward : transform.forward;

            Gizmos.DrawLine(origin, origin + direction * _range);
        }
#endif
    }
}