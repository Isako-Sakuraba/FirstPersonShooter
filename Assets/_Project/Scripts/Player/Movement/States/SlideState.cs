using UnityEngine;

namespace Game.Player.Movement.States
{
    public class SlideState : PlayerMovementStateBase
    {
        private Vector3 _slideDirection;

        public SlideState(PlayerContext context) : base(context) { }

        public override void Enter()
        {
            Vector3 v = context.State.Velocity;
            float previousSpeed = context.State.PreviousVelocity.magnitude;
            Vector3 horizontal = new Vector3(v.x, 0f, v.z);

            horizontal.Normalize();
            _slideDirection = horizontal;
            horizontal *= previousSpeed;
            

            // Apply boost
            if (context.Data.SlideEnterBoost > 0f)
            {
                horizontal += _slideDirection * context.Data.SlideEnterBoost;
                context.State.Velocity.x = horizontal.x;
                context.State.Velocity.z = horizontal.z;
            }

            context.State.IsSliding = true;
        }

        public override void Exit()
        {
            context.State.IsSliding = false;
        }

        public override void Process()
        {
            float dt = Time.fixedDeltaTime;

            Vector3 v = context.State.Velocity;
            Vector3 horizontal = new Vector3(v.x, 0f, v.z);
            float speed = horizontal.magnitude;

            Vector3 wish = context.WishDir;

            // SlideSteering
            if (wish != Vector3.zero)
            {
                float steerFactor = Mathf.InverseLerp(context.Data.SlideSteeringMinSpeed, context.Data.MinSlideEnterSpeed, speed);
                float steerStrength = Mathf.Lerp(0.25f, 1f, steerFactor);

                _slideDirection = Vector3.Slerp(
                    _slideDirection,
                    wish,
                    1f - Mathf.Exp(-context.Data.SlideSteering * steerStrength * dt)
                );

                _slideDirection.y = 0f;

                _slideDirection.Normalize();
            }

            // SlideFriction
            float frictionFactor = Mathf.Exp(-context.Data.SlideFriction * dt);
            horizontal *= frictionFactor;

            // Downhill accel
            if (context.Data.SlideDownhillAcceleration > 0f)
            {
                Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, context.Controller.groundNormal);
                downhill.Normalize();
                horizontal += downhill * (context.Data.SlideDownhillAcceleration * dt);
            }

            // Bias velocity to slide dir
            Vector3 alligned = _slideDirection * horizontal.magnitude;
            horizontal = Vector3.Lerp(horizontal, alligned, 1f - Mathf.Exp(-context.Data.SlideAllignRate * dt));

            context.State.Velocity.x = horizontal.x;
            context.State.Velocity.z = horizontal.z;
        }
    }
}