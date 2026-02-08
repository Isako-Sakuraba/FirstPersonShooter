using UnityEngine;

namespace SplineCollider
{
    internal interface ISplineColliderProxyReciever
    {
        /// <summary>
        /// Internal relay method invoked by a spline segment when a collider enters its trigger.
        /// Do not call this method directly.
        /// </summary>
        /// <param name="other">
        /// The external collider that entered the segment trigger.
        /// </param>
        /// <remarks>
        /// This method is part of the internal event aggregation pipeline.
        /// It is called exclusively by segment proxy components to forward
        /// Unity trigger callbacks to the owning <see cref="SplineCollider"/>.
        /// 
        /// Manual invocation may result in invalid contact state.
        /// </remarks>
        public void OnSegmentTriggerEnter(Collider other);

        /// <summary>
        /// Internal relay method invoked by a spline segment when a collider exits its trigger.
        /// Do not call this method directly.
        /// </summary>
        /// <param name="other">
        /// The external collider that exited the segment trigger.
        /// </param>
        /// <remarks>
        /// This method is used by segment proxy components to maintain
        /// aggregated trigger contact counts. It must not be called by user code.
        /// </remarks>
        public void OnSegmentTriggerExit(Collider other);

        /// <summary>
        /// Internal relay method invoked by a spline segment when a collision begins.
        /// Do not call this method directly.
        /// </summary>
        /// <param name="collision">
        /// The collision information reported by the segment collider.
        /// </param>
        /// <remarks>
        /// This method is called by segment proxy components to forward
        /// Unity collision callbacks to the owning <see cref="SplineCollider"/>.
        /// 
        /// It updates internal contact tracking and dispatches a unified
        /// collision enter event when the first segment makes contact.
        /// </remarks>
        public void OnSegmentCollisionEnter(Collision collision);

        /// <summary>
        /// Internal relay method invoked by a spline segment while a collision is ongoing.
        /// Do not call this method directly.
        /// </summary>
        /// <param name="collision">
        /// The collision information reported by the segment collider.
        /// </param>
        /// <remarks>
        /// This method refreshes cached collision data for aggregated
        /// stay event dispatch. It is not intended for external use.
        /// </remarks>
        public void OnSegmentCollisionStay(Collision collision);

        /// <summary>
        /// Internal relay method invoked by a spline segment when a collision ends.
        /// Do not call this method directly.
        /// </summary>
        /// <param name="collision">
        /// The collision information reported by the segment collider.
        /// </param>
        /// <remarks>
        /// This method updates internal collision tracking and dispatches
        /// a unified collision exit event when the final segment contact ends.
        /// 
        /// Calling this method manually may corrupt the internal contact state.
        /// </remarks>
        public void OnSegmentCollisionExit(Collision collision);

    }
}