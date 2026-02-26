using ECM2;
using Game.Data.Movement;
using System;

namespace Game.Movement
{
    public enum Stance
    {
        Standing,
        Crouched
    }

    public class BodyController : IBodyState
    {
        private CharacterMovement _controller;
        private BodyConfig _bodyData;

        private Stance _stance;

        public Stance Stance => _stance;
        public float Height => _controller.height;

        public event Action<Stance> OnStanceChanged = delegate { };

        public BodyController(CharacterMovement controller, BodyConfig bodyData)
        {
            _controller = controller;
            _bodyData = bodyData;

            Reset();
        }

        public void Reset()
        {
            _controller.SetDimensions(_bodyData.Radius, _bodyData.StandingHeight);
            _stance = Stance.Standing;
            OnStanceChanged.Invoke(Stance);
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
                OnStanceChanged.Invoke(_stance);
            }
            else if (_stance == Stance.Crouched && target == Stance.Standing)
            {
                if (_controller.CheckHeight(_bodyData.StandingHeight))
                    return;

                _controller.SetHeight(_bodyData.StandingHeight);
                _stance = Stance.Standing;
                OnStanceChanged.Invoke(_stance);
            }
        }
    }

    // Read only interface
    public interface IBodyState
    {
        public Stance Stance { get; }
        public float Height { get; }
    }
}