using ECM2;
using Game.Data.Movement;
using UnityEngine;

namespace Game.Movement
{
    public class LocomotionSensors
    {
        private CharacterMovement _controller;
        private MovementConfig _config;
        private Transform _pointer;

        private CollisionResult _wallCollision;
        private bool _wallDetected;

        public CollisionResult WallCollision => _wallCollision;
        public bool WallDetected => _wallDetected;

        private RaycastHit _pointerHit;
        private bool _pointerHasHit;

        public RaycastHit PointerHit => _pointerHit;
        public bool PointerHasHit => _pointerHasHit;

        public LocomotionSensors(CharacterMovement controller, MovementConfig config, Transform pointer)
        {
            _controller = controller;
            _config = config;
            _pointer = pointer;
        }

        public void Update()
        {
            ProbeWallrunning();
            ProbePointer();
        }

        private void ProbeWallrunning()
        {
            _wallDetected = false;
            for (int i = 0; i < _controller.GetCollisionCount(); i++)
            {
                CollisionResult result = _controller.GetCollisionResult(i);
                if (result.hitLocation == HitLocation.Sides)
                {
                    _wallCollision = result;
                    _wallDetected = true;
                    return;
                }
            }
        }

        private void ProbePointer()
        {
            _pointerHasHit = Physics.Raycast(_pointer.position, _pointer.forward, out _pointerHit, _config.Grapple.Distance, _config.Grapple.GrappableLayers);
        }
    }
}