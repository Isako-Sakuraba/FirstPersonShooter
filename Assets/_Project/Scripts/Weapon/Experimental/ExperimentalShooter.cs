using UnityEngine;

namespace Game.Weapons.Experimental
{
    public class ExperimentalShooter : MonoBehaviour
    {
        [SerializeField] private ExperimentalProjectile _projectile;
        [SerializeField] private Transform _directon;
        [SerializeField] private float _velocity = 15f;
        [SerializeField] private float _startTime = 1f;
        [SerializeField] private float _delay = 2f;

        private ExperimentalProjectile _last;

        private void Start()
        {
            InvokeRepeating(nameof(Shoot), _startTime, _delay);
        }

        public void Shoot()
        {
            if (_last != null)
            {
                Destroy(_last.gameObject);
            }

            var projectile = Instantiate(_projectile, _directon);
            projectile.transform.position = _directon.position;
            projectile.SetVelocity(_velocity);
            projectile.Launch(_directon.forward);
            _last = projectile;
        }
    }
}