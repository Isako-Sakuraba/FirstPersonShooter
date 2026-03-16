using System;
using UnityEngine;

namespace Game.Weapons
{
    public class HealthComponent : MonoBehaviour, IDamageable, IDurable
    {
        [SerializeField] private int _initialHealth = 100; // Initial health

        private int _currentHealth;

        public int Durability => _currentHealth; // just for IDurable support
        public int CurrentHealth => _currentHealth;
        public bool IsDead => _currentHealth <= 0;
        public bool IsAlive => _currentHealth > 0;

        public event Action OnDied = delegate { };
        public event Action<float, Vector3, Vector3> OnDamagedAdvanced = delegate { };
        public event Action<float> OnDamaged = delegate { };

        private void Awake()
        {
            _currentHealth = _initialHealth;
        }

        public void TakeDamage(int damage, Vector3 point, Vector3 normal)
        {
            if (IsDead)
                return;

            _currentHealth -= damage;
            OnDamaged.Invoke(damage);
            OnDamagedAdvanced.Invoke(damage, point, normal);

            if (_currentHealth <= 0)
            {
                _currentHealth = 0;
                OnDied.Invoke();
            }
        }
    }
}