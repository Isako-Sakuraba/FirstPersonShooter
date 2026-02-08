using UnityEngine;

namespace SplineCollider
{
    /// <summary>
    /// Internal proxy component attached to each generated spline segment collider.
    /// </summary>
    /// <remarks>
    /// This component forwards Unity physics callbacks (trigger and collision events)
    /// from individual segment colliders to the owning <see cref="SplineCollider"/>.
    ///
    /// It exists solely to support event aggregation across multiple segments and
    /// is automatically added and managed by <see cref="SplineCollider"/> during baking.
    ///
    /// This class is not intended for direct use or interaction by user code.
    /// Removing, disabling, or invoking it manually may break internal contact tracking.
    /// </remarks>
    public sealed class SplineColliderSegmentProxy : MonoBehaviour
    {
        private SplineCollider _owner;

        public void SetOwner(SplineCollider owner)
        {
            _owner = owner;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_owner != null)
                _owner.OnSegmentTriggerEnter(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (_owner != null)
                _owner.OnSegmentTriggerExit(other);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_owner != null)
                _owner.OnSegmentCollisionEnter(collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            if (_owner != null)
                _owner.OnSegmentCollisionStay(collision);
        }

        private void OnCollisionExit(Collision collision)
        {
            if (_owner != null)
                _owner.OnSegmentCollisionExit(collision);
        }
    }
}