using UnityEngine;

namespace Game.Weapons.Experimental
{
    public class ExperimentalWeaponSystem : MonoBehaviour
    {
        [SerializeField] private Transform _fireDirection;
        [SerializeField] private ExperimentalParryWeapon _parryPrefab;

        private IWeapon _currentWeapon;
        private bool _hasWeapon;

        private InputService _inputService;

        private void Awake()
        {
            _inputService = InputService.Instance;

            var parry = Instantiate(_parryPrefab, transform);
            _currentWeapon = parry;
            _hasWeapon = true;
        }

        private void Update()
        {
            if (!_hasWeapon)
                return;

            var context = new FireContext(_fireDirection);

            if (_inputService.FirePressed)
                _currentWeapon.OnFireStart(in context);

            if (_inputService.FireHeld)
                _currentWeapon.OnFireHold(in context);

            if (_inputService.FireReleased)
                _currentWeapon.OnFireEnd(in context);
        }
    }
}