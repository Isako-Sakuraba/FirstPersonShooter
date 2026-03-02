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
        public readonly GrappleType Type;
        public readonly Vector3 LocalPoint;
        public readonly Transform Target;
        public readonly float MaxLength;

        public GrappleAttachRequest(GrappleType type, Vector3 localPoint, Transform target, float maxLength)
        {
            Type = type;
            LocalPoint = localPoint;
            Target = target;
            MaxLength = maxLength;
        }
    }

    public interface IGrappleAttachable
    {
        bool TryGrappleAttach(in GrappleAttachRequest request);
        void DetachGrapple();
        bool IsGrappleAttached { get; }
    }
}