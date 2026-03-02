using Game.Movement;
using UnityEngine;

namespace Game.Experimental
{
    public class ExperimentalGrappleVisuals : MonoBehaviour
    {
        [SerializeField] private LineRenderer _ropePivot;
        [SerializeField] private VerletRope _ropePlayer;
        [SerializeField] private LocomotionController _controller;
        [SerializeField] private Transform _playerVisuals;

        bool track = false;

        private void Awake()
        {
            GrappleDetached(_playerVisuals.position);
            _ropePivot.useWorldSpace = true;
            _ropePivot.positionCount = 2;
            _ropePlayer.TryPin(0);
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

            var pivot = _controller.PivotWorldPoint;
            var player = _playerVisuals.position;
            _ropePivot.SetPosition(0, pivot);
            _ropePivot.SetPosition(1, player);
            _ropePlayer.UpdateVisuals();
            _ropePlayer.SetPosition(0, in player, updateRenderer: true);
        }

        private void GrappleDetached(Vector3 player)
        {
            track = false;
            _ropePivot.enabled = false;
            _ropePlayer.enabled = false;
        }

        private void GrappleAttached()
        {
            track = true;
            _ropePivot.enabled = true;
            _ropePlayer.enabled = true;
            _ropePlayer.RopeLength = _controller.MaxLength - _controller.CurrentLength;
            _ropePlayer.SegmentsCount = Mathf.Max(VerletRope.CalculateSegmentCount(_ropePlayer.RopeLength, 0.5f), 3);
        }
    }
}