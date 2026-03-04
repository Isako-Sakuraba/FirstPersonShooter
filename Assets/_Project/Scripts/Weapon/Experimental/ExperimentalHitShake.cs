using PrimeTween;
using UnityEngine;

namespace Game.Weapons.Experimental
{
    [RequireComponent(typeof(HealthComponent))]
    public class ExperimentalHitShake : MonoBehaviour
    {
        [SerializeField] private Transform _visuals;
        [SerializeField] private ShakeSettings _shakeSettings;

        private HealthComponent _healthComponent;

        private Tween _shakeTween;

        private void Awake()
        {
            _healthComponent = GetComponent<HealthComponent>();
        }

        private void OnEnable()
        {
            _healthComponent.OnDamaged += HandleDamage;
        }

        private void OnDisable()
        {
            _healthComponent.OnDamaged -= HandleDamage;
        }

        private void HandleDamage(float damage)
        {
            if (!_shakeTween.isAlive)
                _shakeTween = Tween.ShakeLocalPosition(_visuals, _shakeSettings);
        }
    }
}