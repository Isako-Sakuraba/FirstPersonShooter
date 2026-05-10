using Game.Interaction;
using PrimeTween;
using UnityEngine;
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

        private string[] _messagePool = new string[] { "Interact", "I", "can", "change", "messages", "YIPPEE", ">:D", "<3" };
        private int _last = 0;

        private void Start()
        {
            _defaultPos = transform.position;
        }

        public void OnFocusEnter(in InteractionContext context)
        {
            if (_tween.isAlive) _tween.Complete();
            _tween = Tween.Scale(_visualTarget, 0.68f, 0.5f);
        }

        public void OnFocusExit(in InteractionContext context)
        {
            if (_tween.isAlive) _tween.Complete();
            _tween = Tween.Scale(_visualTarget, 0.5f, 0.5f);
        }

        public void Interact(in InteractionContext context)
        {
            Tween.Scale(_visualTarget, 0.8f, 0.2f, cycleMode: CycleMode.Yoyo, cycles: 2);
            _last++;
            _last %= _messagePool.Length;
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
            return new DisplayInfo(_visualTarget.position, _messagePool[_last]);
        }
    }
}