using Game.Weapons;
using UnityEngine;

namespace Game.Player
{
    [RequireComponent(typeof(HealthComponent))]
    public class AddHealthOnDeath : MonoBehaviour
    {
        [SerializeField] private HealthComponent _healthComponent;
        [SerializeField] private int _healthToRestore = 10;

        private void Awake()
        {
            if (_healthComponent == null)
            {
                _healthComponent = GetComponent<HealthComponent>();
            }
        }

        private void OnEnable()
        {
            if (_healthComponent != null)
            {
                _healthComponent.OnDied += HandleDied;
            }
        }

        private void OnDisable()
        {
            if (_healthComponent != null)
            {
                _healthComponent.OnDied -= HandleDied;
            }
        }

        private void HandleDied()
        {
            if (_healthToRestore <= 0)
            {
                return;
            }

            PlayerHealth playerHealth = PlayerHealth.Instance;
            if (playerHealth == null)
            {
                return;
            }

            playerHealth.RestoreHealth(_healthToRestore);
        }
    }
}
