using UnityEngine;
using UnityEngine.Pool;

namespace Game.Weapons.Experimental
{
    [DefaultExecutionOrder(-120)]
    public class ExperimentalBloodSplatManager : MonoBehaviour
    {
        private readonly struct ActiveBloodParticles
        {
            public readonly ParticleSystem Particles;
            public readonly float ReleaseTime;

            public ActiveBloodParticles(ParticleSystem particles, float releaseTime)
            {
                Particles = particles;
                ReleaseTime = releaseTime;
            }
        }

        private static ExperimentalBloodSplatManager _instance;
        public static ExperimentalBloodSplatManager Instance => _instance;

        [SerializeField] private float _heightThreshold = 0.005f;
        [SerializeField] private LayerMask _bloodLayer;
        [SerializeField] private float _scaleDuration = 0.5f;
        [SerializeField] private float _startScale = 0.2f;
        [SerializeField] private float _endScale = 1.2f;

        [Header("Rendering")]
        [SerializeField] Mesh mesh;                 // quad
        [SerializeField] Material material;         // Enable GPU Instancing
        [SerializeField] int submeshIndex = 0;

        [Header("Pool")]
        [SerializeField] int capacity = 1024;

        [Header("Blood Particles")]
        [SerializeField] private ParticleSystem _bloodSplatParticlesPrefab;
        [SerializeField] private int _bloodParticlesPoolDefaultCapacity = 16;
        [SerializeField] private int _bloodParticlesPoolMaxSize = 128;
        [SerializeField] private float _bloodParticlesLifetime = 3f;

        Vector3[] _pos;
        Quaternion[] _rot;
        float[] _start;
        Matrix4x4[] _matrices;
        int _head;     // oldest index
        int _count;    // number of live splats (<= capacity)

        RenderParams _rp;
        private ObjectPool<ParticleSystem> _bloodParticlesPool;
        private readonly System.Collections.Generic.List<ActiveBloodParticles> _activeBloodParticles = new(64);

        // Safe default. Docs explain practical limits can be 511 depending on _instance payload/shader.
        const int kBatchMax = 511;

        public int Capacity => capacity;
        public int Count => _count;

        void Awake()
        {
            _instance = this;

            _matrices = new Matrix4x4[capacity];
            _start = new float[capacity];
            _pos = new Vector3[capacity];
            _rot = new Quaternion[capacity];

            _rp = new RenderParams(material)
            {
                // Optional knobs (defaults are fine for decals)
                shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off,
                receiveShadows = false
            };

            if (_bloodSplatParticlesPrefab != null)
            {
                _bloodParticlesPool = new ObjectPool<ParticleSystem>(
                    CreateBloodParticles,
                    OnBloodParticlesGet,
                    OnBloodParticlesRelease,
                    OnBloodParticlesDestroy,
                    collectionCheck: true,
                    defaultCapacity: Mathf.Max(1, _bloodParticlesPoolDefaultCapacity),
                    maxSize: Mathf.Max(1, _bloodParticlesPoolMaxSize));
            }
        }

        /// Adds a splat. If pool is full, overwrites the oldest (FIFO).
        public void Spawn(in Vector3 pos, in Vector3 normal, in LayerMask layer)
        {
            // Align quad to surface normal + random spin around normal.
            var rot = Quaternion.FromToRotation(Vector3.forward, -normal);
            var actualPos = pos + normal * _heightThreshold;

            var m = Matrix4x4.TRS(actualPos, rot, new Vector3(1.2f, 1.2f, 1.2f));
   
            int writeIndex;
            if (_count < capacity)
            {
                writeIndex = _head + _count;
                if (writeIndex >= capacity) writeIndex -= capacity;
                _count++;
            }
            else
            {
                writeIndex = _head;
                _head++;
                if (_head == capacity) _head = 0;
            }

            _matrices[writeIndex] = m;
            _start[writeIndex] = Time.time;
            _pos[writeIndex] = actualPos;
            _rot[writeIndex] = rot;
        }

        public void Clear()
        {
            _head = 0;
            _count = 0;

            int activeCount = _activeBloodParticles.Count;
            for (int i = 0; i < activeCount; i++)
            {
                ActiveBloodParticles active = _activeBloodParticles[i];
                if (active.Particles != null)
                {
                    _bloodParticlesPool?.Release(active.Particles);
                }
            }

            _activeBloodParticles.Clear();
        }

        public void SpawnBloodParticles(Vector3 position)
        {
            if (_bloodParticlesPool == null)
            {
                return;
            }

            ParticleSystem particles = _bloodParticlesPool.Get();
            particles.transform.position = position;
            particles.transform.rotation = Quaternion.identity;

            particles.Clear(true);
            particles.Play(true);

            float releaseTime = Time.time + Mathf.Max(0.01f, _bloodParticlesLifetime);
            _activeBloodParticles.Add(new ActiveBloodParticles(particles, releaseTime));
        }

        void LateUpdate()
        {
            UpdateBloodParticlesPool();

            if (_count == 0) return;

            float time = Time.time;

            // Ring may wrap; draw in up to two contiguous ranges.
            int firstLen = Mathf.Min(_count, capacity - _head);
            UpdateRange(_head, firstLen, time);
            DrawRange(_head, firstLen);

            int remaining = _count - firstLen;
            if (remaining > 0)
            {
                UpdateRange(0, remaining, time);
                DrawRange(0, remaining);
            }
        }

        void UpdateRange(int start, int length, float time)
        {
            for (int i = 0; i < length; i++)
            {
                int idx = start + i;
                var s = _matrices[idx];

                float duration = _scaleDuration;
                float t = duration <= 0f ? 1f : Mathf.Clamp01((time - _start[idx]) / duration);
                float k = t; // 0..1

                float scale = Mathf.LerpUnclamped(_startScale, _endScale, k);

                _matrices[idx] = Matrix4x4.TRS(_pos[idx], _rot[idx], new Vector3(scale, scale, scale));
            }
        }

        void DrawRange(int start, int length)
        {
            while (length > 0)
            {
                int batch = length > kBatchMax ? kBatchMax : length;


                Graphics.RenderMeshInstanced(_rp, mesh, submeshIndex, _matrices, batch, start);

                start += batch;
                length -= batch;
            }
        }

        private void UpdateBloodParticlesPool()
        {
            if (_activeBloodParticles.Count == 0 || _bloodParticlesPool == null)
            {
                return;
            }

            float now = Time.time;
            for (int i = _activeBloodParticles.Count - 1; i >= 0; i--)
            {
                ActiveBloodParticles active = _activeBloodParticles[i];
                if (active.Particles == null)
                {
                    _activeBloodParticles.RemoveAt(i);
                    continue;
                }

                if (now < active.ReleaseTime)
                {
                    continue;
                }

                _bloodParticlesPool.Release(active.Particles);
                _activeBloodParticles.RemoveAt(i);
            }
        }

        private ParticleSystem CreateBloodParticles()
        {
            ParticleSystem particles = Instantiate(_bloodSplatParticlesPrefab, transform, true);
            particles.gameObject.SetActive(false);
            return particles;
        }

        private static void OnBloodParticlesGet(ParticleSystem particles)
        {
            particles.gameObject.SetActive(true);
        }

        private static void OnBloodParticlesRelease(ParticleSystem particles)
        {
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particles.gameObject.SetActive(false);
        }

        private static void OnBloodParticlesDestroy(ParticleSystem particles)
        {
            if (particles != null)
            {
                Destroy(particles.gameObject);
            }
        }
    }
}
