using PrimeTween;
using UnityEngine;
using UnityEngine.Pool;

namespace Game.Experimental
{
    public class HitTracerManager : MonoBehaviour
    {
        [SerializeField] private Material _tracerMaterial;
        [SerializeField] private int _capacity = 32;
        [SerializeField] private float _duration = 0.1f;
        [SerializeField] private float _width = 0.1f;

        private static HitTracerManager _instance;
        public static HitTracerManager Instance => _instance;

        private ObjectPool<LineRenderer> _tracerPool;

        private void Awake()
        {
            _tracerPool = new ObjectPool<LineRenderer>(
                CreateTracer,
                OnTracerGet,
                OnTracerRelease,
                defaultCapacity: _capacity,
                maxSize: 200
            );

            _instance = this;
        }

        private void OnTracerRelease(LineRenderer renderer)
        {
            renderer.widthMultiplier = 0f;
            gameObject.SetActive(false);
        }

        private void OnTracerGet(LineRenderer renderer)
        {
            gameObject.SetActive(true);
            renderer.widthMultiplier = _width;
        }

        public async void DrawTracer(Vector3 from, Vector3 to)
        {
            var tracer = _tracerPool.Get();
            tracer.SetPosition(0, from);
            tracer.SetPosition(1, to);
            await Tween.Custom(tracer, _width, 0f, _duration, (target, value) =>
            {
                target.widthMultiplier = value;
            });
            _tracerPool.Release(tracer);
        }

        private LineRenderer CreateTracer()
        {
            var go = new GameObject("Tracer");
            var line = go.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.numCapVertices = 3;
            line.useWorldSpace = true;
            line.widthCurve = AnimationCurve.Constant(0f, 1f, 1f);
            line.material = _tracerMaterial;
            line.startWidth = 1f;
            line.endWidth = 1f;
            gameObject.SetActive(false);
            return line;
        }
    }
}