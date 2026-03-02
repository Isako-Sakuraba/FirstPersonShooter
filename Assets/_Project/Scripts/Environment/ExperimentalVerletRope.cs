using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Experimental
{
    public class ExperimentalVerletRope : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private LineRenderer _line;

        [Header("Rope settings")]
        [SerializeField] private int _segmentsCount = 10;
        [SerializeField] private float _ropeLength = 5f;

        [Header("Physics settings")]
        [SerializeField] private float _gravity = -12f;
        [SerializeField] private float _dampingFactor = 0.98f;

        [Header("Constraints settings")]
        [SerializeField] private int _numOfConstraintRuns = 50;

        private List<RopeSegment> _segments = new();

        private Vector3 _ropeStartPoint;
        private float _segmentLength;

        private void Awake()
        {
            _segmentLength = _ropeLength / _segmentsCount;
            _line.positionCount = _segmentsCount;
            _ropeStartPoint = transform.position;

            for (int i = 0; i < _segmentsCount; i++)
            {
                _segments.Add(new RopeSegment(_ropeStartPoint, _ropeStartPoint));
                _ropeStartPoint.y -= _segmentLength;
            }
        }

        private void Update()
        {
            DrawRope();
        }

        private void FixedUpdate()
        {
            Simulate();

            for (int i = 0; i < _numOfConstraintRuns; i++)
            {
                ApplyConstraints();
            }
        }

        private void Simulate()
        {
            for (int i = 0; i < _segmentsCount; i++)
            {
                var segment = _segments[i];
                Vector3 velocity = (segment.CurrentPosition - segment.PreviousPosition) * _dampingFactor;

                segment.PreviousPosition = segment.CurrentPosition;
                segment.CurrentPosition += velocity;
                segment.CurrentPosition += Vector3.up * _gravity * Time.fixedDeltaTime;

                _segments[i] = segment;
            }
        }

        private void ApplyConstraints()
        {
            var first = _segments[0];
            first.CurrentPosition = transform.position;
            _segments[0] = first;

            for (int i = 0; i < _segmentsCount - 1; i++)
            {
                var current = _segments[i];
                var next = _segments[i + 1];

                float dist = (current.CurrentPosition - next.CurrentPosition).magnitude;
                float diff = (dist - _segmentLength);

                Vector3 changeDir = (current.CurrentPosition - next.CurrentPosition).normalized;
                Vector3 changeVector = changeDir * diff;

                if (i != 0)
                {
                    current.CurrentPosition -= changeVector * 0.5f;
                    next.CurrentPosition += changeVector * 0.5f;
                } 
                else
                {
                    next.CurrentPosition += changeVector;
                }

                _segments[i] = current;
                _segments[i+1] = next;
            } 
        }

        private void DrawRope()
        {
            for (int i = 0; i < _segmentsCount; i++)
            {
                _line.SetPosition(i, _segments[i].CurrentPosition);
            }
        }

        public struct RopeSegment
        {
            public Vector3 CurrentPosition;
            public Vector3 PreviousPosition;

            public RopeSegment(Vector3 currentPosition, Vector3 previousPosition)
            {
                CurrentPosition = currentPosition;
                PreviousPosition = previousPosition;
            }
        }
    }
}