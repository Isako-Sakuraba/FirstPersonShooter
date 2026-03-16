using Game.Weapons;
using PrimeTween;
using UnityEngine;

namespace Game.Weapons.Experimental
{
    [RequireComponent(typeof(HealthComponent))]
    public class ExperimentalBloodParticlesOnHit : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _bloodParticlesPrefab;

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
            var blood = Instantiate(_bloodParticlesPrefab, transform.root, true);
            blood.transform.position = point;
            blood.transform.parent = transform.root;
            Destroy(blood.gameObject, 3f);
        } 
    }
}