using UnityEngine;

namespace Game.Interaction
{
    public readonly struct InteractionContext
    {
        public readonly Vector3 Interactor;
        public readonly Vector3 Forward;
        public readonly bool IsDirect   ;

        public InteractionContext(Vector3 interactor, Vector3 forward, bool direct)
        {
            Interactor = interactor;
            Forward = forward;
            IsDirect = direct;
        }

        public InteractionContext WithDirect(bool direct)
            => new InteractionContext(this.Interactor, this.Forward, true);
    }
}