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

        public static void StepPendulum3D(
            in Vector3 pivotPosition,
            float ropeLength,
            in Vector3 gravityAcceleration,
            float deltaTime,
            in Vector3 bobPosition,
            in Vector3 bobVelocity,
            out Vector3 nextPosition,
            out Vector3 nextVelocity,
            float drag = 0f)
        {
            Vector3 r = bobPosition - pivotPosition;
            float currentDistance = r.magnitude;

                // --- 1) Predict free motion ---
            Vector3 predictedVelocity = bobVelocity + gravityAcceleration * deltaTime;
            Vector3 predictedPosition = bobPosition + predictedVelocity * deltaTime;

            float predictedDistance = (predictedPosition - pivotPosition).magnitude;

            // --- 2) If rope slack, accept free motion ---
            if (predictedDistance <= ropeLength)
            {
                nextVelocity = predictedVelocity;
                nextPosition = predictedPosition;
                return;
            }

            // --- 3) Rope taut: spherical constraint ---

            // Radial unit vector
            Vector3 n = r.normalized;

            // Remove radial velocity (constraint: v · n = 0)
            float radialSpeed = Vector3.Dot(in predictedVelocity, in n);
            Vector3 tangentialVelocity = predictedVelocity - radialSpeed * n;

            // Apply drag
            float damping = Mathf.Exp(-drag * deltaTime);
            tangentialVelocity *= damping;

            // Tangential component of gravity
            Vector3 tangentialGravity = gravityAcceleration - Vector3.Dot(in gravityAcceleration, in n) * n;

            // Symplectic velocity update in tangent plane
            tangentialVelocity += tangentialGravity* deltaTime;

            // Compute angular velocity vector ω = (r × v) / |r|²
            Vector3 angularVelocity =
                Vector3.Cross(in r, in tangentialVelocity) / (ropeLength * ropeLength);

            float angularSpeed = angularVelocity.magnitude;

            if (angularSpeed > 0.0f)
            {
                Vector3 axis = angularVelocity / angularSpeed;
                float angleStep = angularSpeed * deltaTime;

                // Rodrigues rotation formula
                Vector3 newR =
                    r * Mathf.Cos(angleStep) +
                    Vector3.Cross(in axis, in r) * Mathf.Sin(angleStep) +
                    axis * Vector3.Dot(in axis, in r) * (1 - Mathf.Cos(angleStep));

                newR = newR.normalized * ropeLength;

                nextPosition = pivotPosition + newR;

                    // New velocity = ω × r
                nextVelocity = Vector3.Cross(in angularVelocity, in newR);
            }
            else
            {
                // Edge case: almost no motion
                Vector3 newR = r.normalized * ropeLength;
                nextPosition = pivotPosition + newR;
                nextVelocity = tangentialVelocity;
            }
        }
    }
}