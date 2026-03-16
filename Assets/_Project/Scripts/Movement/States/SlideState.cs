using UnityEngine;

namespace Game.Movement.States
{
    public class SlideState : LocomotionStateBase
    {
        private Vector3 _slideDirection;

        public SlideState(LocomotionContext context) : base(context) { }

        public override void Enter()
        {
            Vector3 v = context.State.Kinematics.Velocity;
            float previousSpeed = context.State.Kinematics.PreviousVelocity.magnitude;
            Vector3 horizontal = new Vector3(v.x, 0f, v.z);

            horizontal.Normalize();
            _slideDirection = horizontal;
            horizontal *= previousSpeed;


            // Apply boost
            if (context.Data.Slide.EnterVelocityMultiplier > 0f)
            {
                horizontal += _slideDirection * context.Data.Slide.EnterVelocityMultiplier;
                context.State.Kinematics.Velocity.x = horizontal.x;
                context.State.Kinematics.Velocity.z = horizontal.z;
            }

            //context.State.IsSliding = true;
        }

        public override void Exit()
        {
            //context.State.IsSliding = false;
        }

        public override void Process()
        {
            float dt = Time.fixedDeltaTime;

            Vector3 v = context.State.Kinematics.Velocity;
            Vector3 horizontal = new Vector3(v.x, 0f, v.z);
            float speed = horizontal.magnitude;

            Vector3 wish = context.WishDir;

            // SlideSteering
            if (wish != Vector3.zero)
            {
                float steerFactor = Mathf.InverseLerp(context.Data.Slide.SteeringDisableSpeed, context.Data.Slide.RequiredEnterSpeed, speed);
                float steerStrength = Mathf.Lerp(0.25f, 1f, steerFactor);

                _slideDirection = Vector3.Slerp(
                    _slideDirection,
                    wish,
                    1f - Mathf.Exp(-context.Data.Slide.Steering * steerStrength * dt)
                );

                _slideDirection.y = 0f;

                _slideDirection.Normalize();
            }

            // SlideFriction
            float frictionFactor = Mathf.Exp(-context.Data.Slide.Friction * dt);
            horizontal *= frictionFactor;

            // Downhill accel
            if (context.Data.Slide.DownhillAcceleration > 0f)
            {
                Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, context.Motor.groundNormal);
                downhill.Normalize();
                horizontal += downhill * (context.Data.Slide.DownhillAcceleration * dt);
            }

            // Bias velocity to slide dir
            Vector3 alligned = _slideDirection * horizontal.magnitude;
            horizontal = Vector3.Lerp(horizontal, alligned, 1f - Mathf.Exp(-context.Data.Slide.AllignRate * dt));

            context.State.Kinematics.Velocity.x = horizontal.x;
            context.State.Kinematics.Velocity.z = horizontal.z;
        }
    }
}