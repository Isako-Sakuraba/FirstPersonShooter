using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[Flags]
public enum PostProcess
{
    Nothing,
    Merge,
    Split
}

public enum ResolutionType
{
    Distance,
    Count
}

[RequireComponent(typeof(SplineContainer))]
public class SplineCollider : MonoBehaviour
{
    private struct Point
    {
        public float t;
        public Vector3 position;
    }

    [SerializeField] private ResolutionType _resolutionType;
    [SerializeField] private PostProcess _postProcess;
    [SerializeField] private bool _isTrigger;

    [SerializeField] private Transform _container;

    [SerializeField] private int _resolution = 10;
    [SerializeField] private float _distance = 10;

    [SerializeField] private float _radius = 0.5f;
    [SerializeField] private float _mergeAngle = 20f;
    [SerializeField] private int _maxSplitLevel = 2;

    

    private SplineContainer _spline;
    private bool _splineNotNull = false;
    private List<Point> _points = new();

    private void OnValidate()
    {
        GetSpline();
    }

    private void GetSpline()
    {
        if (_splineNotNull)
            return;
        _spline = GetComponent<SplineContainer>();
        _splineNotNull = true;
    }

    [ContextMenu("Generate")]
    private void GenerateCollider()
    {
        if (_container == null)
            return;

        DisposeOfChildColliders();

        int limit = 1;
        if (_resolutionType == ResolutionType.Distance)
        {
            float length = _spline.CalculateLength();
            limit = (int)(length / _distance);
            limit = length % _distance == 0 ? limit : limit + 1;
        }
        else if (_resolutionType == ResolutionType.Count)
        {
            limit = _resolution;
        }

        for (int i = 0; i <= limit; i++)
        {
            float t = (float)i / limit;
            Point point = new Point { t = t, position = _spline.EvaluatePosition(t) };
            _points.Add(point);
        }

        // Process merging
        if (_postProcess.HasFlag(PostProcess.Merge))
            for (int point = 1; point < _points.Count - 1; point++)
            {
                Vector3 past = _points[point - 1].position;
                Vector3 current = _points[point].position;
                Vector3 next = _points[point + 1].position;

                float bend = 180f - Vector3.Angle((past - current), (next - current));
                if (bend <= _mergeAngle)
                {
                    _points.RemoveAt(point);
                    point--;
                }

            }

        // Process splitting
        if (_postProcess.HasFlag(PostProcess.Split))
        {
            float maxSplitDistance = (1f / limit) / Mathf.Pow(2f, _maxSplitLevel);
            for (int i = 0; i < _points.Count - 1; i++)
            {
                Point current = _points[i];
                Point next = _points[i + 1];

                if (next.t - current.t <= maxSplitDistance)
                    continue;

                float middleT = (next.t + current.t) / 2f;
                Vector3 middle = _spline.EvaluatePosition(middleT);

                float bend = 180f - Vector3.Angle((current.position - middle), (next.position - middle));
                if (bend > _mergeAngle)
                {
                    Point point = new Point { t = middleT, position = middle };
                    _points.Insert(i + 1, point);
                }
            }
        }

        // Add colliders
        for (int point = 0; point < _points.Count - 1; point++)
        {
            Vector3 start = _points[point].position;
            Vector3 end = _points[point + 1].position;
            CreateCapsuleCollider(start, end, _radius, _container, _isTrigger);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log($"OnEnter: {other.gameObject.name}");
    }

    [ContextMenu("Clear")]
    private void Clear()
    {
        DisposeOfChildColliders();
    }

    private void Reset()
    {
        DisposeOfChildColliders();
    }

    private void DisposeOfChildColliders()
    {
        if (_container == null)
            return;

        while (_container.childCount > 0)
            DestroyImmediate(_container.GetChild(0).gameObject);

        _points.Clear();
    }

    private CapsuleCollider CreateCapsuleCollider(Vector3 start, Vector3 end, float radius, Transform parent, bool isTrigger)
    {
        var go = new GameObject("CapsuleCollider");
        var collider = go.AddComponent<CapsuleCollider>();

        Vector3 middle = (start + end) / 2f;
        go.transform.SetParent(parent);
        go.transform.position = middle;

        Vector3 direction = (end - start).normalized;
        go.transform.rotation = Quaternion.FromToRotation(go.transform.up, direction);

        collider.radius = radius;
        collider.height = (end - start).magnitude + radius * 2;
        collider.isTrigger = isTrigger;
        return collider;
    }
}
