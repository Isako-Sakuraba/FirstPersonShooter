using UnityEngine;

namespace Game.Utils
{
    public static class MovementMath
    {
        public static Vector3 Accelerate(Vector3 velocity, Vector3 wishDir, float wishSpeed, float accel, float deltaTime)
        {
            if (wishSpeed <= 0f || wishDir.sqrMagnitude < 0.0001f) return velocity;

            float currentSpeed = Vector3.Dot(velocity, wishDir);
            float addSpeed = wishSpeed - currentSpeed;
            if (addSpeed <= 0f) return velocity;

            float accelSpeed = accel * wishSpeed * deltaTime;
            if (accelSpeed > addSpeed) accelSpeed = addSpeed;

            return velocity + wishDir * accelSpeed;
        }

        public static Vector3 ApplyFriction(Vector3 velocity, float friction, float stopSpeed, float deltaTime)
        {
            float speed = velocity.magnitude;
            if (speed < 0.001f) return Vector3.zero;

            float control = speed < stopSpeed ? stopSpeed : speed;
            float drop = control * friction * deltaTime;
            float newSpeed = Mathf.Max(speed - drop, 0f);

            return velocity * (newSpeed / speed);
        }

        public static float ToJumpForce(float jumpHeight, float gravity)
            => Mathf.Sqrt(jumpHeight * -2f * gravity);

        public static void StepRopeSpring3D(
            in Vector3 pivotPos,
            in Vector3 pivotVel,
            in Vector3 pivotAccel,
            float restLength,
            float restLengthRate,          // dL/dt (negative when climbing up)
            float maxStretch,              // e.g. 0.5f
            float stiffnessK,              // e.g. 200f..2000f (tune)
            float dampingC,                // e.g. 10f..200f   (tune)
            in Vector3 gravity,
            float dt,
            in Vector3 wishDirHorizontal,  // y should be 0
            float inputAccel,              // e.g. 10..50
            float airDragWhenEngaged,      // per-second (0 = none)
            in Vector3 bobPos,
            in Vector3 bobVel,
            out Vector3 nextPos,
            out Vector3 nextVel)
        {
            // Relative state (constraint lives here)
            Vector3 r = bobPos - pivotPos;
            float dist = r.magnitude;

            // Relative velocity
            Vector3 vRel = bobVel - pivotVel;

            // Effective acceleration in pivot frame
            // If you omit pivotAccel, moving pivot will feel "floaty" / wrong under fast pivot motion.
            Vector3 aEff = gravity - pivotAccel;

            // Default: slack/free-fall => only gravity (NO drag, as requested)
            Vector3 aRel = aEff;

            // Variable rest length (climbing): advance rest length externally each frame.
            // restLengthRate is used to bias radial motion when engaged (optional but helps climbing feel).
            // We'll incorporate it as a Target radial speed when engaged.

            bool engaged = dist > restLength;

            if (engaged && dist > 1e-6f)
            {
                Vector3 n = r / dist;

                // Stretch (unilateral)
                float stretch = dist - restLength;

                // Clamp stretch (prevents extreme elongation)
                if (stretch > maxStretch)
                {
                    stretch = maxStretch;
                    dist = restLength + maxStretch;
                    r = n * dist;
                }

                // Spring-damper along rope
                float vRad = Vector3.Dot(vRel, n);

                // Optional: make climbing feel correct by biasing radial speed toward restLengthRate when engaged.
                // If player shortens rope, desired radial speed is ~restLengthRate (negative).
                float vRadError = vRad - restLengthRate;

                Vector3 aSpring =
                    (-stiffnessK * stretch) * n +
                    (-dampingC * vRadError) * n;

                aRel += aSpring;

                // Tangential input (project wishdir onto tangent plane)
                Vector3 wish = wishDirHorizontal;
                wish.y = 0f;

                Vector3 wishTan = Vector3.ProjectOnPlane(wish, n);
                float wishTanMag = wishTan.magnitude;
                if (wishTanMag > 1e-5f)
                {
                    wishTan /= wishTanMag;
                    aRel += wishTan * inputAccel;
                }

                // Air drag only when engaged (and only on tangential component is usually best)
                if (airDragWhenEngaged > 0f)
                {
                    Vector3 vTan = Vector3.ProjectOnPlane(vRel, n);
                    float damping = Mathf.Exp(-airDragWhenEngaged * dt);
                    vTan *= damping;

                    // Recompose vRel with original radial part (radial handled by spring/damper)
                    vRel = vTan + vRad * n;
                }
            }

            // Semi-implicit Euler in relative frame
            vRel += aRel * dt;
            r += vRel * dt;

            // Recompose world state
            nextPos = pivotPos + r;
            nextVel = pivotVel + vRel;
        }
    }
}