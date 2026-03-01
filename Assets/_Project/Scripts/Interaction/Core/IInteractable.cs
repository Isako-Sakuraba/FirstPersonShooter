using UnityEngine;

namespace Game.Interaction
{
    public interface IInteractable
    {
        bool CanInteract(in InteractionContext context);
        void Interact(in InteractionContext context);
        Vector3 GetInteractionPoint(in InteractionContext context);
    }
}