using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Systems
{
    public static class TimeSystem
    {
        private const float DefaultTimeScale = 1f;
        private const float DefaultFixedDeltaTime = 0.02f;
        private const int MaxActiveRequests = 8;

        private static float _currentScale = DefaultTimeScale;
        private static bool _initialized = false;

        private static int _nextId = 1;
        private static List<Request> _requests = new(MaxActiveRequests);

        public static float DeltaTime => Time.unscaledDeltaTime * _currentScale;
        public static float FixedDeltaTime => Time.fixedUnscaledDeltaTime * _currentScale;

        public static float Scale => _currentScale;
        public static bool IsSlowed => _currentScale < DefaultTimeScale;
        public static bool IsFrozed => _currentScale < 0.01f;
        public static int ActiveRequestCount => _requests.Count;

        public static void Initialize(float defaultFixedDeltaTime = DefaultFixedDeltaTime)
        {
            _initialized = true;
            SetTimeScaleInternal(DefaultTimeScale, defaultFixedDeltaTime);
        }

        public static void Reset(float defaultFixedDeltaTime = DefaultFixedDeltaTime)
        {
            _requests.Clear();
            _nextId = 1;
            _initialized = true;
            SetTimeScaleInternal(DefaultTimeScale, defaultFixedDeltaTime);
        }

        public static void Tick(float unscaledDeltaTime, float defaultFixedDeltaTime = DefaultFixedDeltaTime)
        {
            float targetScale = DefaultTimeScale;

            for (int i = _requests.Count - 1; i >= 0; i--)
            {

                Request request = _requests[i];

                request.Remaining -= unscaledDeltaTime;

                if (request.Remaining <= 0f)
                {
                    _requests.RemoveAt(i);
                    request.Callback?.Invoke();
                    continue;
                }

                _requests[i] = request;

                if (request.Scale < targetScale)
                    targetScale = request.Scale;
            }

            if (!Mathf.Approximately(targetScale, _currentScale))
                SetTimeScaleInternal(targetScale, defaultFixedDeltaTime);
        }

        public static int Hitstop(float duration, Action callback = default)
            => AddRequest(0f, duration, callback);


        public static int Slow(float scale, float duration, Action callback = default)
            => AddRequest(scale, duration, callback);

        private static int AddRequest(float scale, float remaining, Action callback)
        {
            if (ActiveRequestCount == MaxActiveRequests)
                return -1;

            int id = _nextId++;
            _requests.Add(new Request
            {
                Id = id,
                Scale = scale,
                Remaining = remaining,
                Callback = callback
            });

            return id;
        }

        public static bool Cancel(int id)
        {
            for (int i = 0; i < _requests.Count; i++)
            {
                if (_requests[i].Id != id)
                    continue;

                _requests.RemoveAt(i);
                return true;
            }

            return false;
        }

        public static int CancelAllAtOrBelow(float scale)
        {
            int removed = 0;

            for (int i = _requests.Count - 1; i >= 0; i--)
            {
                if (_requests[i].Scale > scale)
                    continue;

                _requests.RemoveAt(i);
                removed++;
            }

            return removed;
        }

        private static void SetTimeScaleInternal(float scale, float defaultFixedDeltaTime)
        {
            _currentScale = Mathf.Clamp01(scale);
            Time.timeScale = scale;
            //Time.fixedDeltaTime = defaultFixedDeltaTime * _currentScale;
        }

        private struct Request
        {
            public int Id;
            public float Scale;
            public float Remaining;
            public Action Callback;
        }
    }
}