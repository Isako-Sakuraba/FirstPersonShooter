using Game.Movement;
using Game.Movement.API.Requests;
using UnityEngine;

namespace Game.Experimental
{
    public class ExperimentalGrappler : MonoBehaviour
    {
        [SerializeField] private LocomotionController _controller;
        [SerializeField] private CharacterOrientation _orientation;
        [SerializeField] private Transform _camera;
        [SerializeField] private float _maxDistance = 40f;
        [SerializeField] private float _radius = 20f;
        [SerializeField] private Transform _pointer;
        [SerializeField] private LayerMask _grappableLayers;

        private InputService _inputService;
        private RaycastHit hit;
        private bool _success;

        private void Awake()
        {
            _inputService = InputService.Instance;
        }

        private void Update()
        {
            Probe();

            _pointer.gameObject.SetActive(_success);
            if (_success)
            {
                _pointer.position = hit.point;
            }

            if (_inputService.FirePressed && _success)
            {
                var transform = hit.transform;
                var local = transform.InverseTransformPoint(hit.point);
                _controller.TryGrappleAttach(new GrappleAttachRequest(GrappleType.Hook, local, transform, _maxDistance));
            }
        }

        private void Probe()
        {
            _success = Physics.Raycast(_camera.position, _orientation.Forward, out hit, _maxDistance, _grappableLayers);

            if (_success)
                return;

            _success = Physics.SphereCast(_camera.position, _radius, _orientation.Forward, out hit, _maxDistance, _grappableLayers);
        }
    }
}