using Game.Weapons;
using UnityEngine;

namespace Game.Weapons.Experimental
{
    [RequireComponent(typeof(HealthComponent))]
    public class ExperimentalBloodParticlesOnHit : MonoBehaviour
    {
        private HealthComponent _healthComponent;

        private void Awake()
        {
            _healthComponent = GetComponent<HealthComponent>();
        }

        private void OnEnable()
        {
            _healthComponent.OnDamagedAdvanced += HandleDamage;
        }

        private void OnDisable()
        {
            _healthComponent.OnDamagedAdvanced -= HandleDamage;
        }

        private void HandleDamage(float damage, Vector3 point, Vector3 normal)
        {
            ExperimentalBloodSplatManager manager = ExperimentalBloodSplatManager.Instance;
            if (manager == null)
            {
                return;
            }

            manager.SpawnBloodParticles(point);
        } 
    }
}
