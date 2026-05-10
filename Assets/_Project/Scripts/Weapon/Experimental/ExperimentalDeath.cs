using UnityEngine;

namespace Game.Weapons.Experimental
{
    [RequireComponent(typeof(HealthComponent))]
    public class ExperimentalDeath : MonoBehaviour
    {
        private HealthComponent _healthComponent;

        private void Awake()
        {
            _healthComponent = GetComponent<HealthComponent>();
        }

        private void OnEnable()
        {
            _healthComponent.OnDied += HandleDeath;
        }

        private void OnDisable()
        {
            _healthComponent.OnDied -= HandleDeath;
        }

        private void HandleDeath()
        {
            Destroy(gameObject);
        }
    }
}