using Game.Utils;
using UnityEngine;

namespace Game.Experimental
{
    public class ExperimentalPendulum : MonoBehaviour
    {
        [SerializeField] private Transform _pivot;
        [SerializeField] private Transform _visuals;
        [SerializeField] private Vector3 _initialVelocity;
        [SerializeField] private float _gravity = -10f;
        [SerializeField] private float _drag = 0.5f;

        private float _length;
        private Vector3 _velocity;
        private Vector3 _position;

        private void Start()
        {
            _velocity = _initialVelocity;
            _position = _visuals.position;
            _length = (_pivot.position - _position).magnitude;
        }

        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
                return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(_visuals.position, _initialVelocity);
        }

        private void Update()
        {
            _visuals.position = Vector3.Lerp(_visuals.position, _position, Time.deltaTime * 40f);
        }

        private void FixedUpdate()
        {
            var dt = Time.fixedDeltaTime;

            MovementMath.StepPendulum3D(
                _pivot.position,
                _length,
                _gravity * Vector3.up,
                dt,
                _position,
                _velocity,
                out var nextP,
                out var nextV,
                _drag);

            _position = nextP;
            _velocity = nextV;
        }
    }
}