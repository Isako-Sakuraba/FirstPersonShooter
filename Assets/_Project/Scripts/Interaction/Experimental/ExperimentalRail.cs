using Game.Interaction;
using Game.Movement.API.Requests;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

using DisplayInfo = Game.Interaction.DisplayInfo;

namespace Game.Experimental
{
    public class ExperimentalRail : MonoBehaviour, IInteractable, IInteractionDisplay
    {
        private SplineContainer _splineContainer;
        private EntityId _playerId;
        private IRailAttachable _attachable;

        public IInteractable Interactable => this;

        private void Awake()
        {
            _splineContainer = GetComponent<SplineContainer>();
        }

        public bool CanInteract(in InteractionContext context)
        {
            if (_attachable == null)
                return true;

            return !_attachable.IsAttachedToRail;
        }

        public Vector3 GetInteractionPoint(in InteractionContext context)
        {
            // World -> local (important)
            float3 localQuery = _splineContainer.transform.InverseTransformPoint(context.Position + context.Forward);

            // nearest + t are in spline local space
            SplineUtility.GetNearestPoint(_splineContainer.Spline, localQuery, out float3 localNearest, out float t);

            // Convert using container (world space)
            return _splineContainer.EvaluatePosition(t);
        }

        public DisplayInfo GetInteractionPointDisplay(in InteractionContext context)
        {
            float3 localQuery = _splineContainer.transform.InverseTransformPoint(context.Position + context.Forward);

            SplineUtility.GetNearestPoint(_splineContainer.Spline, localQuery, out float3 localNearest, out float t);

            Vector3 worldPoint = _splineContainer.EvaluatePosition(t);
            return new DisplayInfo(worldPoint, "Attach");
        }

        public void Interact(in InteractionContext context)
        {
            // TODO: remove this monstrocity
            var attachable = context.Interactor.GetComponentInChildren<IRailAttachable>();
            if (attachable == null)
                return;

            _playerId = context.Interactor.GetEntityId();
            _attachable = attachable;

            // World -> local (important)
            float3 localQuery = _splineContainer.transform.InverseTransformPoint(context.Position + context.Forward);

            // nearest + t are in spline local space
            SplineUtility.GetNearestPoint(_splineContainer.Spline, localQuery, out float3 localNearest, out float t);

            if (!_attachable.IsAttachedToRail)
                _attachable.TryAttachRail(new RailAttachRequest(_splineContainer, t));
        }
    }
}