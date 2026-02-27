using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Game.Movement.States
{
    public class RailState : LocomotionStateBase
    {
        public RailState(LocomotionContext context) : base(context) { }

        private SplineContainer _railSpline;
        private float _length; // cache OK per your request

        public override void Enter()
        {
            if (!context.State.Rail.AttachmentPayload.TryConsume(out var payload))
                Debug.LogError("Entered rail grinding state without payload!");

            var rail = context.State.Rail;
            var cfg = context.Data.Rail;

            rail.IsAttached = true;
            rail.Spline = payload.Spline;

            _railSpline = payload.Spline;
            _length = Mathf.Max(_railSpline.CalculateLength(), 1e-4f);

            // Snap to nearest spline point from motor position (robust attach)
            float3 localQuery = _railSpline.transform.InverseTransformPoint(context.Motor.transform.position);
            SplineUtility.GetNearestPoint(_railSpline.Spline, localQuery, out _, out float t);

            rail.T = Mathf.Clamp01(t);

            Vector3 entryPoint = _railSpline.EvaluatePosition(rail.T);

            // Initialize signed speed from current velocity projected onto tangent
            float3 tan3 = _railSpline.EvaluateTangent(rail.T);
            tan3 = math.normalizesafe(tan3, new float3(0, 0, 1));

            Vector3 tangent = (Vector3)tan3;

            float signedSpeed = Vector3.Dot(context.State.Kinematics.Velocity, tangent);

            // Keep your old "boost on entry" behavior (1.2f)
            rail.Speed = signedSpeed * 1.2f;

            // Your design: rail motion is fully driven by computed velocity
            context.State.Kinematics.Velocity = Vector3.zero;

            context.Motor.PauseGroundConstraint(0.6f);

            if (cfg.SnapPosition)
                context.Motor.SetPosition(entryPoint);
        }

        public override void Process()
        {
            float dt = Time.fixedDeltaTime;
            if (dt <= 0f) return;

            float invDt = 1f / dt;

            var rail = context.State.Rail;
            var cfg = context.Data.Rail;

            // Fail-safe
            if (rail.Spline == null)
                return;

            // Keep local cache in sync (in case runtime spline changed)
            if (_railSpline != rail.Spline)
            {
                _railSpline = rail.Spline;
                _length = Mathf.Max(_railSpline.CalculateLength(), 1e-4f);
            }

            // Optional detach request: let the FSM transition logic handle leaving
            if (rail.DetachmentPayload.TryConsume(out _))
                return;

            // Evaluate spline at current T
            Vector3 railPos = _railSpline.EvaluatePosition(rail.T);

            float3 tan3 = _railSpline.EvaluateTangent(rail.T);
            tan3 = math.normalizesafe(tan3, new float3(0, 0, 1));
            Vector3 tangent = (Vector3)tan3; // unit tangent

            // Optional safety snap (kept, but gated by delta)
            if (cfg.SnapPosition)
            {
                Vector3 motorPosSnap = context.Motor.transform.position;
                if ((railPos - motorPosSnap).sqrMagnitude > cfg.SnapPositionDelta)
                    context.Motor.SetPosition(railPos);
            }

            // Input: full wish dir projected onto tangent
            float wishAlong = Mathf.Clamp(Vector3.Dot(context.WishDir, tangent), -1f, 1f);

            // Target speed from input
            float targetSpeed = cfg.Speed * wishAlong;

            // Smooth speed toward target (your original logic)
            rail.Speed = Mathf.Lerp(rail.Speed, targetSpeed, cfg.Acceleration * dt);

            // --- SLOPE GRAVITY (affects speed only) ---
            // cfg.Gravity == 0 => off
            if (cfg.Gravity != 0f)
            {
                // Gravity vector: cfg.Gravity < 0 means downward gravity
                Vector3 gravity = Vector3.up * cfg.Gravity; // e.g. -9.81 => (0, -9.81, 0)

                // Acceleration along rail tangent (m/s^2)
                float aSlope = Vector3.Dot(gravity, tangent);

                if (cfg.AlwaysApplyGravity)
                {
                    // Always applies, even from standstill
                    rail.Speed += aSlope * dt;
                }
                else
                {
                    // Only meaningfully applies when you're already moving (or trying to move),
                    // so it behaves more like a speed multiplier than "accelerate from rest".
                    const float minSpeedForGravity = 1.0f; // move to config if you want tuning

                    float speed01 = Mathf.Clamp01(Mathf.Abs(rail.Speed) / minSpeedForGravity);

                    // Optional: also require some player intent (prevents slope nudging when fully idle)
                    // If you want gravity even with no input (but only once moving), remove this multiplier.
                    float input01 = Mathf.Clamp01(Mathf.Abs(wishAlong));

                    float gravityScale = speed01 * input01;

                    rail.Speed += aSlope * gravityScale * dt;
                }
            }

            // --- DRAG / "FRICTION" (recommended) ---
            // Prevents endless acceleration on long downhills.
            if (cfg.Drag > 0f)
                rail.Speed *= 1f / (1f + cfg.Drag * dt);

            // Keep things controllable
            rail.Speed = Mathf.Clamp(rail.Speed, -cfg.Speed, cfg.Speed);

            // Advance along rail using your approximation: distance = T * length
            float distance = (rail.T * _length) + (rail.Speed * dt);
            rail.T = Mathf.Clamp01(distance / _length);

            // Convert next rail point into velocity
            Vector3 nextPoint = _railSpline.EvaluatePosition(rail.T);

            // Re-read motor pos in case we snapped
            Vector3 motorPos = context.Motor.transform.position;

            context.State.Kinematics.Velocity = (nextPoint - motorPos) * invDt;
        }

        public override void Exit()
        {
            var rail = context.State.Rail;

            rail.IsAttached = false;
            rail.Spline = null;

            if (_railSpline != null)
            {
                float3 tan3 = _railSpline.Spline.EvaluateTangent(rail.T);
                tan3 = math.normalizesafe(tan3, new float3(0, 0, 1));
                Vector3 tangent = (Vector3)tan3;

                context.State.Kinematics.Velocity = tangent * rail.Speed;
            }

            _railSpline = null;
        }
    }
}