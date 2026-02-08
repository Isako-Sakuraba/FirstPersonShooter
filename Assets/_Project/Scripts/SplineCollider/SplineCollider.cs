using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace SplineCollider
{
    public enum SegmentShape
    {
        Capsule,
        Box
    }

    public enum SamplingMode
    {
        Distance,
        Count
    }

    [Flags]
    public enum ColliderPostProcess
    {
        None = 0,
        MergeShallowBends = 1 << 0,
        SubdivideSharpBends = 1 << 1,
    }

    [AddComponentMenu("Physics/SplineCollider")]
    [RequireComponent(typeof(SplineContainer))]
    public class SplineCollider : MonoBehaviour, ISplineColliderProxyReciever
    {
        private struct SamplePoint
        {
            public float t;
            public Vector3 worldPosition;
        }

        #region Settings
        [SerializeField] private Transform _colliderRoot;

        [SerializeField] private SegmentShape _segmentShape;
        [SerializeField] private SamplingMode _samplingMode;
        [SerializeField] private ColliderPostProcess _postProcessing;
        [SerializeField] private bool _isTrigger;

        [SerializeField] private int _segmentCount = 20;
        [SerializeField] private float _segmentLength = 2f;

        [SerializeField] private float _radius = 0.5f;
        [SerializeField] private float _minBendAngle = 5f;
        [SerializeField] private int _maxSubdivisionDepth = 1;
        #endregion

        private SplineContainer _splineContainer;

        private List<SamplePoint> _samplePoints = new();

        private Stack<GameObject> _spawnedSegments = new Stack<GameObject>();
        private readonly Dictionary<Collider, int> _triggerContactCounts = new();
        private readonly Dictionary<Collider, int> _collisionContactCounts = new();
        private readonly Dictionary<Collider, Collision> _lastCollisionByCollider = new();

        #region Exposed(public) fields
        /// <summary>
        /// Gets the spline container used as the source for collider generation.
        /// </summary>
        public SplineContainer SplineContainer => _splineContainer;

        /// <summary>
        /// Gets whether segment colliders have been generated and are currently present.
        /// </summary>
        /// <remarks>
        /// This property determines the baked state by searching for
        /// <see cref="SplineColliderSegmentProxy"/> components in the children
        /// of <see cref="_colliderRoot"/>.
        ///
        /// This operation performs a hierarchy traversal and may allocate memory.
        /// It is therefore relatively expensive and should not be called
        /// every frame (for example, from <c>Update</c> or <c>FixedUpdate</c>).
        ///
        /// For performance-critical code, prefer <see cref="HasBakedCollidersCache"/>,
        /// which reflects the state of the internal cache rebuilt in <c>OnEnable</c>.
        /// </remarks>t
        public bool HasBakedColliders => 
            _colliderRoot != null &&
            _colliderRoot.GetComponentInChildren<SplineColliderSegmentProxy>(true) != null;

        /// <summary>
        /// Gets whether segment colliders have been generated and are currently present
        /// in the internal cache.
        /// </summary>
        /// <remarks>
        /// This property reflects only the state of the in-memory
        /// <see cref="_spawnedSegments"/> cache.
        ///
        /// The cache is not serialized and is cleared on domain reload
        /// (for example, when exiting Play Mode or recompiling scripts).
        /// As a result, this value may be false even if baked segment
        /// GameObjects still exist in the scene hierarchy.
        ///
        /// For a persistent baked-state check, prefer querying the hierarchy
        /// or using <see cref="HasBakedColliders"/>.
        /// </remarks>
        public bool HasBakedCollidersCached =>
            _colliderRoot != null &&
            _spawnedSegments.Count > 0;

        #region Collider events
        /// <summary>
        /// Invoked when a collider first begins overlapping any spline segment trigger.
        /// </summary>
        /// <remarks>
        /// This event is raised once per collider when the first segment overlap occurs,
        /// and is not invoked again until all overlaps have ended.
        /// </remarks>
        public event Action<Collider> OnTriggerEnter = delegate { };

        /// <summary>
        /// Invoked once per physics step while a collider remains overlapping
        /// any spline segment trigger.
        /// </summary>
        /// <remarks>
        /// This event is aggregated across all segments and is raised at most once
        /// per physics step per collider.
        /// </remarks>
        public event Action<Collider> OnTriggerStay = delegate { };

        /// <summary>
        /// Invoked when a collider is no longer overlapping any spline segment trigger.
        /// </summary>
        /// <remarks>
        /// This event is raised after the final segment overlap has ended.
        /// </remarks>
        public event Action<Collider> OnTriggerExit = delegate { };

        /// <summary>
        /// Invoked when a collider first begins colliding with any spline segment.
        /// </summary>
        /// <remarks>
        /// This event is raised when the first segment establishes physical contact.
        /// </remarks>
        public event Action<Collision> OnCollisionEnter = delegate { };

        /// <summary>
        /// Invoked once per physics step while a collider remains in contact
        /// with any spline segment.
        /// </summary>
        /// <remarks>
        /// This event is aggregated across all segments and is raised at most once
        /// per physics step per collider.
        /// </remarks>
        public event Action<Collision> OnCollisionStay = delegate { };

        /// <summary>
        /// Invoked when a collider is no longer colliding with any spline segment.
        /// </summary>
        /// <remarks>
        /// This event is raised after the final segment contact has ended.
        /// </remarks>
        public event Action<Collision> OnCollisionExit = delegate { };
        #endregion
        #endregion

        private void OnEnable()
        {
            RebuildSpawnedSegmentsCache();
        }

        private void OnValidate()
        {
            EnsureSplineReference();
            ValidateSettingsValues();
        }

        private void Reset()
        {
            EnsureSplineReference();
            SetDefaultColliderRootIfNull();
            ClearBakedSegments();
        }

        #region Caching/References/Validation
        private void EnsureSplineReference()
        {
            if (_splineContainer != null)
                return;

            _splineContainer = GetComponent<SplineContainer>();
        }

        private void SetDefaultColliderRootIfNull()
        {
            if (_colliderRoot == null)
                _colliderRoot = transform;
        }

        private void RebuildSpawnedSegmentsCache()
        {
            _spawnedSegments.Clear();

            if (_colliderRoot == null)
                return;

            var relays = _colliderRoot.GetComponentsInChildren<SplineColliderSegmentProxy>(true);
            foreach (var relay in relays)
                _spawnedSegments.Push(relay.gameObject);
        }

        private void ValidateSettingsValues()
        {
            EnsureClamped(ref _segmentCount, 1);
            EnsureClamped(ref _segmentLength, 0.2f);

            EnsureClamped(ref _minBendAngle, 0.5f, 179f);
            EnsureClamped(ref _maxSubdivisionDepth, 1, 5);

            EnsureClamped(ref _radius, float.Epsilon);
        }

        private static void EnsureClamped(ref float value, float min, float max = float.MaxValue)
        {
            value = Mathf.Clamp(value, min, max);
        }

        private static void EnsureClamped(ref int value, int min, int max = int.MaxValue)
        {
            value = Mathf.Clamp(value, min, max);
        }
        #endregion

        #region Main features
        [ContextMenu("Bake")]
        public void Bake()
        {
            SetDefaultColliderRootIfNull();

            ClearBakedSegments();

            int segmentCount = ComputeSegmentCount();

            SamplePoints(segmentCount);

            if (_postProcessing.HasFlag(ColliderPostProcess.MergeShallowBends))
                MergeShallowBends();

            if (_postProcessing.HasFlag(ColliderPostProcess.SubdivideSharpBends))
                SubdivideSharpBends(segmentCount);

            GenerateColliders();
        }

        [ContextMenu("Clear")]
        public void ClearBakedSegments()
        {
            if (_colliderRoot == null)
                return;

            var relays = _colliderRoot.GetComponentsInChildren<SplineColliderSegmentProxy>(true);
            foreach ( var relay in relays) 
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Undo.DestroyObjectImmediate(relay.gameObject);
                else
                    Destroy(relay.gameObject);
#else
                Destroy(relay.gameObject);
#endif
            }

            _spawnedSegments.Clear();
            _samplePoints.Clear();
        }
        #endregion

        #region Baking process
        private int ComputeSegmentCount()
        {
            int count = 1;
            if (_samplingMode == SamplingMode.Distance)
            {
                float length = _splineContainer.CalculateLength();
                count = (int)(length / _segmentLength);
                count = length % _segmentLength == 0 ? count : count + 1;
            }
            else if (_samplingMode == SamplingMode.Count)
            {
                count = _segmentCount;
            }

            return count;
        }

        private void SamplePoints(int limit)
        {
            for (int i = 0; i <= limit; i++)
            {
                float t = (float)i / limit;
                SamplePoint point = new SamplePoint { t = t, worldPosition = _splineContainer.EvaluatePosition(t) };
                _samplePoints.Add(point);
            }
        }

        private void GenerateColliders()
        {
            for (int point = 0; point < _samplePoints.Count - 1; point++)
            {
                Vector3 start = _samplePoints[point].worldPosition;
                Vector3 end = _samplePoints[point + 1].worldPosition;
                var collider = CreateSegment(_segmentShape, start, end, _radius, _isTrigger);
                collider.transform.SetParent(_colliderRoot);
                _spawnedSegments.Push(collider.gameObject);
            }
        }

        private void SubdivideSharpBends(int limit)
        {
            float maxSplitDistance = (1f / limit) / Mathf.Pow(2f, _maxSubdivisionDepth);
            for (int i = 0; i < _samplePoints.Count - 1; i++)
            {
                SamplePoint current = _samplePoints[i];
                SamplePoint next = _samplePoints[i + 1];

                if (next.t - current.t <= maxSplitDistance)
                    continue;

                float middleT = (next.t + current.t) / 2f;
                Vector3 middle = _splineContainer.EvaluatePosition(middleT);

                float bend = 180f - Vector3.Angle((current.worldPosition - middle), (next.worldPosition - middle));
                if (bend > _minBendAngle)
                {
                    SamplePoint point = new SamplePoint { t = middleT, worldPosition = middle };
                    _samplePoints.Insert(i + 1, point);
                }
            }
        }

        private void MergeShallowBends()
        {
            for (int point = 1; point < _samplePoints.Count - 1; point++)
            {
                Vector3 past = _samplePoints[point - 1].worldPosition;
                Vector3 current = _samplePoints[point].worldPosition;
                Vector3 next = _samplePoints[point + 1].worldPosition;

                float bend = 180f - Vector3.Angle((past - current), (next - current));
                if (bend <= _minBendAngle)
                {
                    _samplePoints.RemoveAt(point);
                    point--;
                }

            }
        }
        #endregion

        private void FixedUpdate()
        {
            foreach (var kvp in _triggerContactCounts)
            {
                var other = kvp.Key;
                SplineTriggerStay(other);
            }

            foreach (var kvp in _collisionContactCounts)
            {
                var other = kvp.Key;
                if (_lastCollisionByCollider.TryGetValue(other, out var collision))
                    SplineCollisionStay(collision);
            }
        }

        #region Spline collider events
        private void SplineTriggerEnter(Collider other)
            => OnTriggerEnter.Invoke(other);

        private void SplineTriggerStay(Collider other)
            => OnTriggerStay.Invoke(other);

        private void SplineTriggerExit(Collider other)
            => OnTriggerExit.Invoke(other);

        private void SplineCollisionEnter(Collision collision)
            => OnCollisionEnter.Invoke(collision);

        private void SplineCollisionStay(Collision collision)
            => OnCollisionStay.Invoke(collision);

        private void SplineCollisionExit(Collision collision)
            => OnCollisionExit.Invoke(collision);
        #endregion

        #region Checking for triggers/collision API
        /// <summary>
        /// Checks whether this spline collider is currently colliding with the specified collider
        /// using non-trigger segment colliders.
        /// </summary>
        /// <param name="other">
        /// The external collider to test against.
        /// </param>
        /// <returns>
        /// True if at least one spline segment collider is in physical (non-trigger) contact
        /// with <paramref name="other"/>; otherwise, false.
        /// </returns>
        /// <remarks>
        /// This method aggregates contacts across all generated spline segments.
        /// If multiple segments are simultaneously colliding with the same collider,
        /// the result remains true until all contacts have ended.
        /// </remarks>
        public bool IsCollidingWith(Collider other)
            => _collisionContactCounts.TryGetValue(other, out int count) && count > 0;

        /// <summary>
        /// Checks whether this spline collider is currently overlapping the specified collider
        /// using trigger-based segment colliders.
        /// </summary>
        /// <param name="other">
        /// The external collider to test against.
        /// </param>
        /// <returns>
        /// True if at least one spline segment trigger collider is overlapping
        /// <paramref name="other"/>; otherwise, false.
        /// </returns>
        /// <remarks>
        /// This method reflects aggregated trigger overlaps from all spline segments.
        /// Multiple simultaneous overlaps are treated as a single logical contact.
        /// </remarks>
        public bool IsTriggerOverlapping(Collider other)
            => _triggerContactCounts.TryGetValue(other, out int count) && count > 0;

        /// <summary>
        /// Checks whether this spline collider is currently in any physical contact
        /// with the specified collider, either through collisions or trigger overlaps.
        /// </summary>
        /// <param name="other">
        /// The external collider to test against.
        /// </param>
        /// <returns>
        /// True if <paramref name="other"/> is currently colliding with or overlapping
        /// any spline segment collider; otherwise, false.
        /// </returns>
        /// <remarks>
        /// This method combines the results of <see cref="IsCollidingWith"/> and
        /// <see cref="IsTriggerOverlapping"/> to provide a unified contact query.
        /// </remarks>
        public bool IsInContactWith(Collider other)
            => IsCollidingWith(other) || IsTriggerOverlapping(other);
        #endregion

        #region Proxy events
        #region OnTrigger
        /// <inheritdoc />
        public void OnSegmentTriggerEnter(Collider other)
        {
            if (!_triggerContactCounts.TryGetValue(other, out var contacts))
                contacts = 0;

            contacts++;
            _triggerContactCounts[other] = contacts;

            if (contacts == 1)
                SplineTriggerEnter(other);
        }

        /// <inheritdoc />
        public void OnSegmentTriggerExit(Collider other)
        {
            if (!_triggerContactCounts.TryGetValue(other, out var contacts))
                return;

            contacts--;
            if (contacts <= 0)
            {
                _triggerContactCounts.Remove(other);
                SplineTriggerExit(other);
            }
            else
            {
                _triggerContactCounts[other] = contacts;
            }
        }
        #endregion

        #region OnCollision
        /// <inheritdoc/>
        public void OnSegmentCollisionEnter(Collision collision)
        {
            var other = collision.collider;
            _lastCollisionByCollider[other] = collision;

            if (!_collisionContactCounts.TryGetValue(other, out int count))
                count = 0;

            count++;
            _collisionContactCounts[other] = count;

            if (count == 1)
                SplineCollisionEnter(collision);
        }

        /// <inheritdoc/>
        public void OnSegmentCollisionStay(Collision collision)
        {
            var other = collision.collider;

            if (_collisionContactCounts.TryGetValue(other, out int count) && count > 0)
                _lastCollisionByCollider[other] = collision;
        }

        /// <inheritdoc/>
        public void OnSegmentCollisionExit(Collision collision)
        {
            var other = collision.collider;

            if (!_collisionContactCounts.TryGetValue(other, out int count))
                return;

            count--;
            if (count <= 0)
            {
                _collisionContactCounts.Remove(other);
                SplineCollisionExit(collision);
            }
            else
            {
                _collisionContactCounts[other] = count;
            }
        }
        #endregion
        #endregion

        #region Segment Creation
        private Collider CreateSegment(SegmentShape shape, Vector3 start, Vector3 end, float radius, bool isTrigger)
        {
            var segment = new GameObject("SplineColliderSegment");

#if UNITY_EDITOR
            // For undo support
            if (!Application.isPlaying)
                Undo.RegisterCreatedObjectUndo(segment, "Bake Spline Colliders");
#endif
            // Add collider event relay
            var relay = segment.AddComponent<SplineColliderSegmentProxy>();
            relay.SetOwner(this);

            // Set position
            Vector3 middle = (start + end) / 2f;
            segment.transform.position = middle;

            // Set rotation
            Vector3 direction = (end - start).normalized;
            segment.transform.rotation = Quaternion.FromToRotation(segment.transform.up, direction);

            // Add collider
            Collider collider;
            if (shape == SegmentShape.Capsule)
                collider = MakeCapsuleSegment(segment, start, end, radius);
            else
                collider = MakeBoxSegment(segment, start, end, radius);

            collider.isTrigger = isTrigger;

            return collider;
        }

        private CapsuleCollider MakeCapsuleSegment(GameObject gameObject, Vector3 start, Vector3 end, float radius)
        {
            var collider = gameObject.AddComponent<CapsuleCollider>();

            collider.radius = radius;
            collider.height = (end - start).magnitude + radius * 2;
            return collider;
        }

        private BoxCollider MakeBoxSegment(GameObject gameObject, Vector3 start, Vector3 end, float radius)
        {
            var collider = gameObject.AddComponent<BoxCollider>();

            float sizeY = (end - start).magnitude;
            float sizeXZ = radius*2;
            collider.size = new Vector3(sizeXZ, sizeY, sizeXZ);
            return collider;
        }
        #endregion
    }
}