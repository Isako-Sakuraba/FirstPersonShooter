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
            Vector3 horizontal = new Vector3(v.x, 0f, v.z);

            _slideDirection = horizontal.normalized;

            // Applu boost
            if (context.Data.EnterBoost > 0f)
            {
                horizontal += _slideDirection * context.Data.EnterBoost;
                context.State.Velocity.x = horizontal.x;
                context.State.Velocity.z = horizontal.z;
            }
        }

        public override void Process()
        {
            float dt = Time.fixedDeltaTime;

            Vector3 v = context.State.Velocity;
            Vector3 horizontal = new Vector3(v.x, 0f, v.z);
            float speed = horizontal.magnitude;

            Vector3 wish = context.WishDir;

            // Steering
            if (wish != Vector3.zero)
            {
                float steerFactor = Mathf.InverseLerp(context.Data.SteeringMinSpeed, context.Data.MinEnterSpeed, speed);
                float steerStrength = Mathf.Lerp(0.25f, 1f, steerFactor);

                _slideDirection = Vector3.Slerp(
                    _slideDirection,
                    wish,
                    1f - Mathf.Exp(-context.Data.Steering * steerStrength * dt)
                );

                _slideDirection.y = 0f;

                _slideDirection.Normalize();
            }

            // Friction
            float frictionFactor = Mathf.Exp(-context.Data.Friction * dt);
            horizontal *= frictionFactor;

            // Downhill accel
            if (context.Data.DownhillAccel > 0f)
            {
                Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, context.Controller.groundNormal);
                downhill.Normalize();
                horizontal += downhill * (context.Data.DownhillAccel * dt);
            }

            // Bias velocity to slide dir
            float alignRate = 10f;
            Vector3 alligned = _slideDirection * horizontal.magnitude;
            horizontal = Vector3.Lerp(horizontal, alligned, 1f - Mathf.Exp(-alignRate * dt));

            context.State.Velocity.x = horizontal.x;
            context.State.Velocity.z = horizontal.z;
        }
    }
}