using Game.Systems;
using UnityEngine;

namespace Game.Weapons.Experimental
{
    [RequireComponent(typeof(Rigidbody))]
    public class ExperimentalProjectile : MonoBehaviour, IParryable
    {
        [SerializeField] private float _gravity = -4f;

        private Rigidbody _rb;

        private float _velocity = 14f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void OnParry(in ParryContext context)
        {
            _rb.linearVelocity = context.Direction * 24f;
            HitstopSystem.Trigger();
        }

        public void SetVelocity(float velocity)
        {
            _velocity = velocity;
        }

        public void Launch(Vector3 direction)
        {
            _rb.AddForce(direction * _velocity, ForceMode.VelocityChange);
        }
    }
}