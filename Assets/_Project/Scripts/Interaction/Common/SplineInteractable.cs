using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Game.Interaction
{
    public class SplineInteractable : MonoBehaviour, IInteractable, IInteractionDisplay
    {
        private SplineContainer _splineContainer;

        public IInteractable Interactable => this;

        private void Awake()
        {
            _splineContainer = GetComponent<SplineContainer>();
        }

        public bool CanInteract(in InteractionContext context)
        {
            return true;
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

        }
    }
}