using ECM2;

namespace Game.Movement
{
    public class LocomotionSensors
    {
        private CharacterMovement _controller;

        private CollisionResult _wallCollision;
        private bool _wallDetected;

        public CollisionResult WallCollision => _wallCollision;
        public bool WallDetected => _wallDetected;


        public LocomotionSensors(CharacterMovement controller)
        {
            _controller = controller;
        }

        public void Update()
        {
            ProbeWallrunning();
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
    }
}