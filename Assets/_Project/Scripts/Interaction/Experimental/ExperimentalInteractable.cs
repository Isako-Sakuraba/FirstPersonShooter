using UnityEngine;
using Game.Interaction;
using PrimeTween;

using DisplayInfo = Game.Interaction.DisplayInfo;

namespace Game.Experimental
{
    public class ExperimentalInteractable : MonoBehaviour, IFocusable, IInteractable, IInteractionDisplay
    {
        [SerializeField] private Transform _visualTarget;
        [SerializeField] private string _text;
        private Tween _tween;
        private Vector3 _defaultPos;

        public IInteractable Interactable => this;

        private void Start()
        {
            _defaultPos = transform.position;
        }

        public void OnFocusEnter(in InteractionContext context)
        {
            if (_tween.isAlive) _tween.Complete();
            _tween = Tween.Scale(_visualTarget, 1.2f, 0.5f);
        }

        public void OnFocusExit(in InteractionContext context)
        {
            if (_tween.isAlive) _tween.Complete();
            _tween = Tween.Scale(_visualTarget, 1f, 0.5f);
        }

        public void Interact(in InteractionContext context)
        {
            Tween.Scale(_visualTarget, 1.4f, 0.2f, cycleMode: CycleMode.Yoyo, cycles: 2);
        }

        public bool CanInteract(in InteractionContext context)
        {
            return context.IsDirect;
        }

        public Vector3 GetInteractionPoint(in InteractionContext context)
        {
            return transform.position;
        }

        public DisplayInfo GetInteractionPointDisplay(in InteractionContext context)
        {
            return new DisplayInfo(_visualTarget.position, _text);
        }
    }
}