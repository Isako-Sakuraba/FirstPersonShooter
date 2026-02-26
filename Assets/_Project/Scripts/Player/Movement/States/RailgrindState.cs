using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Game.Movement.States
{
    public class RailgrindState : LocomotionStateBase
    {
        public RailgrindState(LocomotionContext context) : base(context) { }
        private enum State
        {
            Attaching,
            Grinding
        }

        private SplineContainer _railSpline;
        private float _currentT;
        private float _length;

        private State _state;

        private float _currentSpeed = 0f;
        private float _maxSpeed = 38f;
        private float _acceleration = 0.6f;

        public override void Enter()
        {
            var consumed = context.State.Rail.AttachmentPayload.TryConsume(out var payload);
            if (!consumed)
                Debug.LogError("Entered rail grinding state, but no payload?");

            context.State.Rail.IsAttached = true;
            context.State.Rail.Spline = payload.Spline;
            context.State.Rail.T = payload.StartT;

            _railSpline = context.State.Rail.Spline;

            float3 localQuery = _railSpline.transform.InverseTransformPoint(context.Motor.transform.position);

            SplineUtility.GetNearestPoint(_railSpline.Spline, localQuery, out float3 localNearest, out float t);

            Vector3 worldPoint = _railSpline.EvaluatePosition(t);

            Vector3 tangent = _railSpline.EvaluateTangent(t);
            tangent.Normalize();


            float dot = Vector3.Dot(context.State.Kinematics.Velocity.normalized, tangent.normalized);
            dot = Mathf.Sign(dot);
            if (dot == 0f) dot = 1f;

            Vector3 projectedVelocity = Vector3.Project(context.State.Kinematics.Velocity, tangent);


            _state = State.Attaching;

            _currentSpeed = dot * projectedVelocity.magnitude * 1.2f;
            context.State.Kinematics.Velocity = Vector3.zero;
            _currentT = t;
            context.State.Rail.T = Mathf.Clamp01(_currentT);
            _length = _railSpline.CalculateLength();
            context.Motor.PauseGroundConstraint(0.6f);
        }

        public override void Process()
        {
            //if (_state == State.Attaching)
            //{
            //    Vector3 worldPoint = _railSpline.EvaluatePosition(_currentT);
            //    Vector3 next = Vector3.Lerp(context.Controller.transform.position, worldPoint, Time.fixedDeltaTime * 12f );
            //    context.Controller.SetPosition(worldPoint);
            //    if ((next-worldPoint).sqrMagnitude <= 0.001f)
            //        _state = State.Grinding;
            //}
            //else if (_state == State.Grinding)
            //{
            Vector3 tangent = _railSpline.EvaluateTangent(_currentT);
            float dot = Vector3.Dot(context.Orientation.Forward, tangent.normalized);
            dot = Mathf.Sign(dot);
            if (dot == 0f) dot = 1f;
            float input = context.Input.Move.y;
            float unnormalizedT = _currentT * _length;
            _currentSpeed = Mathf.Lerp(_currentSpeed, _maxSpeed * input * dot, _acceleration * Time.fixedDeltaTime);
            float nextUT = unnormalizedT + (_currentSpeed * Time.fixedDeltaTime);
            float nextT = nextUT / _length;
            _currentT = nextT;

            Vector3 worldPoint = _railSpline.EvaluatePosition(_currentT);


            context.Motor.SetPosition(worldPoint);
            //}
            context.State.Rail.T = Mathf.Clamp01(_currentT);
        }

        public override void Exit()
        {
            context.State.Rail.IsAttached = false;
            context.State.Rail.Spline = null;
            Vector3 tangent = _railSpline.Spline.EvaluateTangent(_currentT);
            context.State.Kinematics.Velocity += tangent.normalized * _currentSpeed;
        }
    }
}