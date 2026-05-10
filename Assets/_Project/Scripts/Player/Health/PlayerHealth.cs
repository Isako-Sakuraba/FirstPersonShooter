using Game.Weapons;
using System;
using UnityEngine;

namespace Game.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(HealthComponent))]
    public class PlayerHealth : MonoBehaviour
    {
        private static PlayerHealth _instance;
        public static PlayerHealth Instance => _instance;

        [SerializeField] private HealthComponent _healthComponent;

        public int CurrentHealth => _healthComponent != null ? _healthComponent.CurrentHealth : 0;
        public int MaxHealth => _healthComponent != null ? _healthComponent.MaxHealth : 0;
        public float Health01 => MaxHealth > 0 ? (float)CurrentHealth / MaxHealth : 0f;
        public bool IsDead => _healthComponent != null && _healthComponent.IsDead;

        public event Action Died = delegate { };

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            if (_healthComponent == null)
            {
                _healthComponent = GetComponent<HealthComponent>();
            }
        }

        private void OnEnable()
        {
            if (_healthComponent != null)
            {
                _healthComponent.OnDied += OnDied;
            }
        }

        private void OnDisable()
        {
            if (_healthComponent != null)
            {
                _healthComponent.OnDied -= OnDied;
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnDied()
        {
            Died.Invoke();
        }

        public void RestoreHealth(int amount)
        {
            if (_healthComponent == null)
            {
                return;
            }

            _healthComponent.RestoreHealth(amount);
        }
    }
}
