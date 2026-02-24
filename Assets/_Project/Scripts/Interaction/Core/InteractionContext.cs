using UnityEngine;

namespace Game.Interaction
{
    public readonly struct InteractionContext
    {
        public readonly GameObject Interactor;
        public readonly Vector3 Position;
        public readonly Vector3 Forward;
        public readonly bool IsDirect   ;

        public InteractionContext(GameObject interactor, Vector3 position, Vector3 forward, bool direct)
        {
            Interactor = interactor;
            Position = position;
            Forward = forward;
            IsDirect = direct;
        }

        public InteractionContext WithDirect(bool direct)
            => new InteractionContext(this.Interactor, this.Position, this.Forward, true);
    }
}