using Game.Movement;
using UnityEngine;

namespace Game.Experimental
{
    public class ExperimentalGrappleVisuals : MonoBehaviour
    {
        [SerializeField] private LineRenderer _line;
        [SerializeField] private LocomotionController _controller;
        [SerializeField] private Transform _playerVisuals;

        bool track = false;

        private void Awake()
        {
            GrappleDetached();
        }

        private void OnEnable()
        {
            _controller.OnGrappleAttached += GrappleAttached;
            _controller.OnGrappleDetached += GrappleDetached;
        }

        private void OnDisable()
        {
            _controller.OnGrappleAttached -= GrappleAttached;
            _controller.OnGrappleDetached -= GrappleDetached;
        }

        private void LateUpdate()
        {
            if (!track)
                return;

            _line.SetPosition(0, _playerVisuals.position);
            _line.SetPosition(1, _controller.PivotWorldPoint);
        }

        private void GrappleDetached()
        {
            track = false;
            _line.SetPosition(0, _playerVisuals.position);
            _line.SetPosition(1, _playerVisuals.position);
        }

        private void GrappleAttached()
        {
            track = true;
        }
    }
}