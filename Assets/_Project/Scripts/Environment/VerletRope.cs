using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Experimental
{
    public class VerletRope : MonoBehaviour
    {
        #region SERIALIZED FIELDS
        [Header("Dependencies")]
        [SerializeField] private LineRenderer _line;

        [Header("Visuals")]
        [SerializeField] private bool _handleVisuals = true;
        [SerializeField] private bool _lerpPositions = true;
        [Min(0.1f)]
        [SerializeField] private float _lerpFactor = 40f;
        [SerializeField] private bool _enableDisableLineRenderer = true;

        [Header("Rope settings")]
        [Min(2)]
        [SerializeField] private int _segmentsCount = 10;
        [Min(0.5f)]
        [SerializeField] private float _ropeLength = 5f;
        [SerializeField] private bool _pinTop = true;

        [Header("Physics settings")]
        [SerializeField] private float _gravity = -12f;
        [SerializeField] private float _dampingFactor = 0.98f;

        [Header("Constraints settings")]
        [Min(1)]
        [SerializeField] private int _constraintIterations = 50;
        #endregion

        #region PROPERTIES
        public float RopeLength
        {
            get => _ropeLength; 
            set
            {
                _ropeLength = math.max(value, 0.5f);
                CalculateCachedData();
            }
        }

        public int SegmentsCount
        {
            get => _segmentsCount;
            set
            {
                _segmentsCount = math.max(value, 2);
                CalculateCachedData();
                if (_segments.IsCreated && _segments.Length < value)
                    InitializeArrays();
            }
        }

        public float Gravity
        {
            get => _gravity;
            set => _gravity = value;
        }

        public float DampingFactor
        {
            get => _dampingFactor;
            set => _dampingFactor = value;
        }

        public float LerpFactor
        {
            get => _lerpFactor;
            set => _lerpFactor = math.max(value, 0.1f);
        }

        public int ConstraintIterations
        {
            get => _constraintIterations;
            set => _constraintIterations = math.max(value, 1);
        }

        public float SegmentLength
        {
            get => _segmentLength;
        }
        #endregion

        private NativeArray<RopeSegment> _segments;
        private Vector3[] _lineRendererPoints;

        private float _segmentLength;

        private JobHandle _lastHandle;

        #region MONOBEHAVIOUR
        private void Awake()
        {
            _line.useWorldSpace = true;
            CalculateCachedData();
            InitializeArrays();
        }

        private void OnDisable()
        {
            if (_enableDisableLineRenderer)
                _line.enabled = false;
        }

        private void OnEnable()
        {
            if (_enableDisableLineRenderer)
                _line.enabled = true;
        }

        private void LateUpdate()
        {
            if (!_handleVisuals)
                return;

            UpdateVisuals();
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;

            var simulateJob = new SimulateJob
            {
                segments = _segments,
                dampingFactor = _dampingFactor,
                dt = dt,
                gravity = _gravity
            };

            var constraintsJob = new ConstraintsJob
            {
                segments = _segments,
                iterations = _constraintIterations,
                segmentLength = _segmentLength,
                segmentsCount = _segmentsCount,
            };

            int length = math.max(1, (int)_ropeLength);
            int batchLength = _segmentsCount / length;
            batchLength = math.max(batchLength, 1);
            var simulateHandle = simulateJob.Schedule(_segmentsCount, batchLength, dependsOn: _lastHandle);
            _lastHandle = constraintsJob.Schedule(dependsOn: simulateHandle);
        }

        private void OnDestroy()
        {
            _lastHandle.Complete();
            if (_segments.IsCreated)
                _segments.Dispose();
        }
        #endregion

        #region PUBLIC API METHODS
        public void Reinitialize()
        {
            CalculateCachedData();
            InitializeArrays();
        }

        public void UpdateVisuals()
        {
            _lastHandle.Complete();
            DrawRope();
        }

        public void SetPosition(int index, in Vector3 position, bool zeroVelocity = true, bool updateRenderer = false)
        {
            _lastHandle.Complete();

            var s = _segments[index];
            s.CurrentPosition = position;
            if (zeroVelocity)
                s.PreviousPosition = position;

            _segments[index] = s;

            if (updateRenderer)
                _line.SetPosition(index, position);
        }

        public Vector3 GetPosition(int index)
        {
            return _segments[index].CurrentPosition;
        }

        public bool TryGetPosition(int index, out Vector3 position, bool updateRenderer = false)
        {
            position = default(Vector3);
            if ((uint)index >= _segmentsCount)
                return false;

            position = _segments[index].CurrentPosition;
            return true;
        }

        public bool TrySetPosition(int index, in Vector3 position, bool zeroVelocity = true, bool updateRenderer = false)
        {
            if ((uint)index >= _segmentsCount)
                return false;

            _lastHandle.Complete();

            var s = _segments[index];
            s.CurrentPosition = position;
            if (zeroVelocity)
                s.PreviousPosition = position;

            _segments[index] = s;

            if (updateRenderer)
                _line.SetPosition(index, position);

            return true;
        }

        public bool TryPin(int index)
        {
            if ((uint)index >= _segmentsCount)
                return false;

            var s = _segments[index];
            s.Pinned = true;
            _segments[index] = s;

            return true;
        }

        public bool TrySetPinned(int index, bool pinned)
        {
            if ((uint)index >= _segmentsCount)
                return false;

            var s = _segments[index];
            s.Pinned = pinned;
            _segments[index] = s;

            return true;
        }
        #endregion

        #region PRIVATE METHODS
        private void DrawRope()
        {
            float dt = Time.deltaTime;

            for (int i = 0; i < _segmentsCount; i++)
            {
                var previous = _lineRendererPoints[i];
                Vector3 current = _segments[i].CurrentPosition;
                if (_lerpPositions)
                    current = Vector3.Lerp(in previous, in current, dt * _lerpFactor);
                _lineRendererPoints[i] = current;
            }
            _line.SetPositions(_lineRendererPoints);
        }

        private void CalculateCachedData()
        {
            _segmentLength = _ropeLength / _segmentsCount;
            _line.positionCount = _segmentsCount;
        }

        private void InitializeArrays()
        {
            if (_segments.IsCreated)
                _segments.Dispose();

            _segments = new NativeArray<RopeSegment>(_segmentsCount, Allocator.Persistent);
            _lineRendererPoints = new Vector3[_segmentsCount];

            var startPoint = transform.position;
            for (int i = 0; i < _segmentsCount; i++)
            {
                _segments[i] = new RopeSegment(startPoint, startPoint);
                _lineRendererPoints[i] = startPoint;
                startPoint.y -= _segmentLength;
            }

            if (_pinTop)
            {
                var s = _segments[0];
                s.Pinned = true;
                s.CurrentPosition = transform.position;
                s.PreviousPosition = transform.position;
                _segments[0] = s;
            }
        }

        public static int CalculateSegmentCount(float totalLength, float segmentLength)
        {
            totalLength = math.max(totalLength, 0f);
            segmentLength = math.max(segmentLength, 1e-5f);

            return math.max((int)math.floor(totalLength / segmentLength), 1);
        }
        #endregion

        [BurstCompile]
        public struct RopeSegment
        {
            public float3 CurrentPosition;
            public float3 PreviousPosition;
            public bool Pinned;

            public RopeSegment(float3 currentPosition, float3 previousPosition)
            {
                CurrentPosition = currentPosition;
                PreviousPosition = previousPosition;
                Pinned = false;
            }
        }

        #region JOBS
        [BurstCompile]
        public struct SimulateJob : IJobParallelFor
        {
            public NativeArray<RopeSegment> segments;
            [ReadOnly] public float dt;
            [ReadOnly] public float dampingFactor;
            [ReadOnly] public float gravity;

            public void Execute(int index)
            {
                var segment = segments[index];
                if (segment.Pinned)
                    return;

                float3 velocity = (segment.CurrentPosition - segment.PreviousPosition) * dampingFactor;

                segment.PreviousPosition = segment.CurrentPosition;
                segment.CurrentPosition += velocity;
                segment.CurrentPosition += new float3(0f, 1f, 0f) * gravity * dt;

                segments[index] = segment;
            }
        }

        [BurstCompile]
        public struct ConstraintsJob : IJob
        {
            public NativeArray<RopeSegment> segments;

            [ReadOnly] public float segmentLength;
            [ReadOnly] public int segmentsCount;
            [ReadOnly] public int iterations;

            public void Execute()
            {
                for (int iter = 0; iter < iterations; iter++)
                {
                    for (int i = 0; i < segmentsCount - 1; i++)
                    {
                        var current = segments[i];
                        var next = segments[i + 1];

                        if (current.Pinned && next.Pinned)
                            continue;

                        float3 delta = current.CurrentPosition - next.CurrentPosition;

                        float dist = math.lengthsq(delta);
                        float invLength = math.rsqrt(dist + 1e-12f);
                        float len = dist * invLength;
                        float diff = (len - segmentLength);

                        float3 change = delta * (invLength * diff);

                        if (!current.Pinned && !next.Pinned)
                        {
                            current.CurrentPosition -= change * 0.5f;
                            next.CurrentPosition += change * 0.5f;
                        }
                        else if (current.Pinned && !next.Pinned)
                        {
                            next.CurrentPosition += change;
                        }
                        else if (!current.Pinned && next.Pinned)
                        {
                            current.CurrentPosition -= change;
                        }

                        segments[i] = current;
                        segments[i + 1] = next;
                    }
                }
            }
        }
        #endregion
    }
}