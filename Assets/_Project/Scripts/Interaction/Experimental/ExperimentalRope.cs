using Game.Interaction;
using Game.Movement.API.Requests;
using UnityEngine;

using DisplayInfo = Game.Interaction.DisplayInfo;

namespace Game.Experimental
{
    public class ExperimentalRope : MonoBehaviour, IInteractable, IInteractionDisplay
    {
        [SerializeField] private Transform _pivot;
        [SerializeField] private float _length = 4f;
        [SerializeField] private CapsuleCollider _capsule;

        public IInteractable Interactable => this;

        public Vector3 Top => _pivot.position;
        public Vector3 Bottom => _pivot.position - transform.up * _length;

        private EntityId _playerId;
        private IGrappleAttachable _attachable;

        private void Awake()
        {
            _capsule.height = _length;
            _capsule.center = -Vector3.up * (_length / 2);
        }

        public bool CanInteract(in InteractionContext context)
        {
            if (_attachable == null)
                return true;

            return !_attachable.IsGrappleAttached;
        }

        public Vector3 GetInteractionPoint(in InteractionContext context)
        {
            return GetClosestPointOnRope(context.Position, Top, Bottom);
        }

        public DisplayInfo GetInteractionPointDisplay(in InteractionContext context)
        {
            var position = GetClosestPointOnRope(context.Position, Top, Bottom);
            var info = new DisplayInfo(position, "Grab");
            return info;
        }

        public void Interact(in InteractionContext context)
        {
            var attachable = context.Interactor.GetComponentInChildren<IGrappleAttachable>();

            if (attachable == null)
                return;

            var sent = attachable.TryGrappleAttach(new GrappleAttachRequest(GrappleType.Rope, _pivot.position));
            if (sent)
            {
                _playerId = context.Interactor.GetEntityId();
                _attachable = attachable;
            }
        }

        public static Vector3 GetClosestPointOnRope(Vector3 worldPos, Vector3 top, Vector3 bottom)
        {
            // Calculate the rope direction and length
            Vector3 ropeDirection = bottom - top;
            float ropeLength = ropeDirection.magnitude;

            // Handle case where top and bottom are the same point
            if (ropeLength < 0.0001f)
                return top;

            // Normalize the rope direction
            Vector3 ropeDirectionNormalized = ropeDirection / ropeLength;

            // Vector from top to world position
            Vector3 topToWorld = worldPos - top;

            // Project worldPos onto the rope line (dot product gives the parameter t)
            float t = Vector3.Dot(topToWorld, ropeDirectionNormalized);

            // Clamp t to the rope segment [0, ropeLength]
            t = Mathf.Clamp(t, 0.0f, ropeLength);

            // Calculate the closest point on the rope
            Vector3 closestPoint = top + ropeDirectionNormalized * t;

            return closestPoint;
        }
    }
}