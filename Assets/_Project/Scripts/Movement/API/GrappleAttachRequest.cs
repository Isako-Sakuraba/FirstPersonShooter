using UnityEngine;

namespace Game.Movement.API.Requests
{
    public enum GrappleType
    {
        Rope, // For generic rope
        Hook // For grapple-hook
    }

    public readonly struct GrappleAttachRequest
    {
        public readonly GrappleType type;
        public readonly Vector3 point;

        public GrappleAttachRequest(GrappleType type, Vector3 point)
        {
            this.type = type;
            this.point = point;
        }
    }

    public interface IGrappleAttachable
    {
        bool TryGrappleAttach(in GrappleAttachRequest request);
        void DetachGrapple();
        bool IsGrappleAttached { get; }
    }
}