using Game.Interaction;
using Game.Movement.API.Requests;
using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using DisplayInfo = Game.Interaction.DisplayInfo;

namespace Game.Experimental
{
    public class ExperimentalRope : MonoBehaviour, IInteractable, IInteractionDisplay
    {
        [SerializeField] private VerletRope _rope;
        [SerializeField] private Transform _pivot;
        [SerializeField] private float _length = 4f;
        [SerializeField] private CapsuleCollider _capsule;

        public IInteractable Interactable => this;

        public Vector3 Top => _pivot.position;
        public Vector3 Bottom => _pivot.position - transform.up * _length;

        private IGrappleAttachable _attachable;
        private bool _attached = false;

        private void Awake()
        {
            _capsule.height = _length;
            _capsule.center = -Vector3.up * (_length / 2);
        }

        public bool CanInteract(in InteractionContext context)
        {
            return !_attached;
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

            var point = transform.InverseTransformPoint(_pivot.position);
            var sent = attachable.TryGrappleAttach(new GrappleAttachRequest(GrappleType.Rope, point, transform, _length));

            if (sent)
            {
                _rope.enabled = false;
                _attached = true;
                _attachable = attachable;
                _attachable.OnGrappleDetached += GrappleDetached;
            }
        }

        private void GrappleDetached(Vector3 player)
        {
            _attachable.OnGrappleDetached -= GrappleDetached;
            _rope.enabled = true;
            _attached = false;

            // Assume rope pivot is the rope object's transform.
            // If your pivot is elsewhere, replace pivotPos accordingly.
            Vector3 pivotPos = _pivot.position;
            Vector3 playerPos = player;

            Vector3 delta = playerPos - pivotPos;
            float dist = delta.magnitude;

            Debug.DrawRay(pivotPos, delta, Color.yellow, 10f);

            // Avoid NaNs when player == pivot
            Vector3 dir = dist > 1e-6f ? (delta / dist) : Vector3.forward;


            int count = _rope.SegmentsCount;
            float step = (dist) / (count - 1);

            for (int i = 0; i < count; i++)
            {
                // Place point i at pivot + i * step along direction
                Vector3 p = pivotPos + dir * (i * step);

                // Optionally clamp so we don't go past the player
                // (useful if rope length > distance pivot->player)
                if (i > 0 && (p - pivotPos).sqrMagnitude > (playerPos - pivotPos).sqrMagnitude)
                    p = playerPos;

                // zeroVelocity = true so you don't inject velocity impulses
                _rope.SetPosition(i, p, zeroVelocity: true, updateRenderer: true);
            }

            // Force the last point to be exactly at the player (common for grapples)
            _rope.SetPosition(count - 1, playerPos, zeroVelocity: true, updateRenderer: true);
        }

        public static Vector3 GetClosestPointOnRope(Vector3 worldPos, Vector3 top, Vector3 bottom)
        {
            // Calculate the rope direction and length
            Vector3 ropeDirection = bottom - top;
            float ropeLength = ropeDirection.magnitude;

            // Handle case where top and bottom are the same LocalPoint
            if (ropeLength < 0.0001f)
                return top;

            // Normalize the rope direction
            Vector3 ropeDirectionNormalized = ropeDirection / ropeLength;

            // Vector from top to world currentPosition
            Vector3 topToWorld = worldPos - top;

            // Project worldPos onto the rope line (dot product gives the parameter t)
            float t = Vector3.Dot(topToWorld, ropeDirectionNormalized);

            // Clamp t to the rope segment [0, ropeLength]
            t = Mathf.Clamp(t, 0.0f, ropeLength);

            // Calculate the closest LocalPoint on the rope
            Vector3 closestPoint = top + ropeDirectionNormalized * t;

            return closestPoint;
        }
    }
}