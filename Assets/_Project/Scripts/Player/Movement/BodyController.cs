using ECM2;
using System;

namespace Game.Player.Movement
{
    public class BodyController
    {
        private CharacterMovement _controller;
        private PlayerBodyData _bodyData;
        
        private Stance _stance;

        public Stance Stance => _stance;
        public float Height => _controller.height;

        public event Action<Stance> OnStanceChanged = delegate { };

        public BodyController(CharacterMovement controller, PlayerBodyData bodyData)
        {
            _controller = controller;
            _bodyData = bodyData;

            Reset();
        }

        public void Reset()
        {
            _controller.radius = _bodyData.Radius;
            UpdateCollider(Stance.Standing);
        }

        public void Update(Stance target)
        {
            UpdateCollider(target);
        }

        private void UpdateCollider(Stance target)
        {
            if (_stance == Stance.Standing && target == Stance.Crouched)
            {
                _controller.SetHeight(_bodyData.CrouchHeight);
                _stance = Stance.Crouched;
            }
            else if (_stance == Stance.Crouched && target == Stance.Standing)
            {
                if (_controller.CheckHeight(_bodyData.StandingHeight))
                    return;

                _controller.SetHeight(_bodyData.StandingHeight);
                _stance = Stance.Standing;
            }

            OnStanceChanged.Invoke(_stance);
        }
    }
}