using UnityEngine;
using UnityEngine.Splines;

namespace Game.Movement.API.Requests
{
    public readonly struct RailAttachRequest
    {
        public readonly SplineContainer Spline;
        public readonly float StartT;

        public RailAttachRequest(SplineContainer spline, float startT)
        {
            Spline = spline;
            StartT = startT;
        }
    }

    public interface IRailAttachable
    {
        bool TryAttachRail(in RailAttachRequest request);
        void DetachRail();
        bool IsOnRail { get; } // optional for interact logic
    }
}