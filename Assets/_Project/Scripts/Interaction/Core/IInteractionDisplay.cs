using UnityEngine;

namespace Game.Interaction
{
    public interface IInteractionDisplay
    {
        IInteractable Interactable { get; }

        DisplayInfo GetInteractionPointDisplay(in InteractionContext context);
    }

    public readonly struct DisplayInfo
    {
        public readonly Vector3 position;
        public readonly string text;

        public DisplayInfo(Vector3 position, string text)
        {
            this.position = position;
            this.text = text;
        }
    }
}