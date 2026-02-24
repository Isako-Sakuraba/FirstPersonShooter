using Game.Player.Movement;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Game.Interaction
{
    public class SplineInteractable : MonoBehaviour, IInteractable, IInteractionDisplay
    {
        private SplineContainer _splineContainer;
        private EntityId _playerId;
        private PlayerContext _context;

        public IInteractable Interactable => this;

        private void Awake()
        {
            _splineContainer = GetComponent<SplineContainer>();
        }

        public bool CanInteract(in InteractionContext context)
        {
            if(_context == null)
                return true;

            return !_context.State.IsAttached;
        }

        public Vector3 GetInteractionPoint(in InteractionContext context)
        {
            // World -> local (important)
            float3 localQuery = _splineContainer.transform.InverseTransformPoint(context.Position);

            // nearest + t are in spline local space
            SplineUtility.GetNearestPoint(_splineContainer.Spline, localQuery, out float3 localNearest, out float t);

            // Convert using container (world space)
            return _splineContainer.EvaluatePosition(t);
        }

        public DisplayInfo GetInteractionPointDisplay(in InteractionContext context)
        {
            float3 localQuery = _splineContainer.transform.InverseTransformPoint(context.Position);

            SplineUtility.GetNearestPoint(_splineContainer.Spline, localQuery, out float3 localNearest, out float t);

            Vector3 worldPoint = _splineContainer.EvaluatePosition(t);
            return new DisplayInfo(worldPoint, "Attach");
        }

        public void Interact(in InteractionContext context)
        {
            // TODO: remove this monstrocity
            var movement = context.Interactor.GetComponentInChildren<PlayerMovement>();
            if (movement == null)
                return;

            _playerId = context.Interactor.GetEntityId();
            _context = movement.Conext;

            var state = movement.Conext.State;

            if (!state.IsAttached)
                Attach(movement.Conext);
                
        }

        public void Attach(PlayerContext context)
        {
            context.State.IsAttached = true;
            context.State.RailSplineContainer = _splineContainer;
        }
    }
}