using UnityEngine;
using UnityEngine.Events;

namespace SplineCollider.Helpers
{
    [RequireComponent(typeof(SplineCollider))]
    public class SplineColliderUnityEvents : MonoBehaviour
    {
        private SplineCollider _splineCollider;

        [Header("Trigger Events")]
        [SerializeField] private UnityEvent<Collider> _onTriggerEnter = new();
        [SerializeField] private UnityEvent<Collider> _onTriggerStay = new();
        [SerializeField] private UnityEvent<Collider> _onTriggerExit = new();

        [Header("Collision Events")]
        [SerializeField] private UnityEvent<Collision> _onCollisionEnter = new();
        [SerializeField] private UnityEvent<Collision> _onCollisionStay = new();
        [SerializeField] private UnityEvent<Collision> _onCollisionExit = new();

        public UnityEvent<Collider> OnTriggerEnter => _onTriggerEnter;
        public UnityEvent<Collider> OnTriggerStay => _onTriggerStay;
        public UnityEvent<Collider> OnTriggerExit => _onTriggerExit;

        public UnityEvent<Collision> OnCollisionEnter => _onCollisionEnter;
        public UnityEvent<Collision> OnCollisionStay => _onCollisionStay;
        public UnityEvent<Collision> OnCollisionExit => _onCollisionExit;

        private void Awake()
        {
            _splineCollider = GetComponent<SplineCollider>();
        }

        private void OnEnable()
        {
            if (_splineCollider == null)
                return;

            // Subscribe
            _splineCollider.OnTriggerEnter += HandleTriggerEnter;
            _splineCollider.OnTriggerStay += HandleTriggerStay;
            _splineCollider.OnTriggerExit += HandleTriggerExit;

            _splineCollider.OnCollisionEnter += HandleCollisionEnter;
            _splineCollider.OnCollisionStay += HandleCollisionStay;
            _splineCollider.OnCollisionExit += HandleCollisionExit;
        }

        private void OnDisable()
        {
            if (_splineCollider == null)
                return;

            // Unsubscribe
            _splineCollider.OnTriggerEnter -= HandleTriggerEnter;
            _splineCollider.OnTriggerStay -= HandleTriggerStay;
            _splineCollider.OnTriggerExit -= HandleTriggerExit;

            _splineCollider.OnCollisionEnter -= HandleCollisionEnter;
            _splineCollider.OnCollisionStay -= HandleCollisionStay;
            _splineCollider.OnCollisionExit -= HandleCollisionExit;
        }

        #region Forwarders

        private void HandleTriggerEnter(Collider other)
            => _onTriggerEnter.Invoke(other);

        private void HandleTriggerStay(Collider other)
            => _onTriggerStay.Invoke(other);

        private void HandleTriggerExit(Collider other)
            => _onTriggerExit.Invoke(other);

        private void HandleCollisionEnter(Collision collision)
            => _onCollisionEnter.Invoke(collision);

        private void HandleCollisionStay(Collision collision)
            => _onCollisionStay.Invoke(collision);

        private void HandleCollisionExit(Collision collision)
            => _onCollisionExit.Invoke(collision);

        #endregion
    }
}