using Game.Interaction;
using Game.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Player.Interaction
{
    public class PlayerInteractor : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private InteractionPromptUI _interactionPrompt;
        [SerializeField] private SphereCollider _interactionSphere;
        [SerializeField] private Transform _interactorForward;
        [SerializeField] private float _interactionRadius = 3f;
        [SerializeField] private float _interactionDistance = 2f;
        [SerializeField] private LayerMask _interactionLayer;

        private TriggerEvents _trigger;
        private InputService _inputService;

        private readonly Dictionary<EntityId, IInteractable> _candidates = new();
        private readonly List<int> _removeBuffer = new(); // For removing null candidates if they were destroyed

        private IInteractable _focused;
        private bool _directlyFocused = false;

        private void Awake()
        {
            _inputService = InputService.Instance;

            if (!_interactionSphere.TryGetComponent(out _trigger))
            {
                Debug.LogError("Could not find TriggerEvents on the SphereCollider!");
            }

            _interactionSphere.radius = _interactionRadius;
            _interactionSphere.isTrigger = true;
            _interactionSphere.excludeLayers = ~_interactionLayer;
        }

        private void Update()
        {
            RemoveNullInteractibles();
            UpdateFocus();
        }

        private void RemoveNullInteractibles()
        {
            
        }

        private void UpdateFocus()
        {
            IInteractable nextFocused = null;

            InteractionContext context = new InteractionContext(_interactorForward.position, _interactorForward.forward, false);
            InteractionContext directContext = context.WithDirect(true);

            if (Physics.Raycast(_interactorForward.position, _interactorForward.forward, out var hit, _interactionDistance, _interactionLayer))
            {
                var id = hit.collider.gameObject.GetEntityId();
                if (_candidates.TryGetValue(id, out var rayTarget) && rayTarget.CanInteract(directContext))
                {
                    nextFocused = rayTarget;
                    _directlyFocused = true;
                }
            }

            if (nextFocused == null)
            {
                float bestSqr = _interactionRadius * _interactionRadius;

                foreach (var pair in _candidates)
                {
                    IInteractable candidate = pair.Value;

                    if (candidate == null || (candidate is UnityEngine.Object obj && obj == null))
                    {
                        _removeBuffer.Add(pair.Key);
                        continue;
                    }

                    if (!candidate.CanInteract(context))
                        continue;

                    Vector3 point = candidate.GetInteractionPoint(context);
                    float distanceSqr = (point - _interactorForward.position).sqrMagnitude;

                    if (distanceSqr < bestSqr)
                    {
                        bestSqr = distanceSqr;
                        nextFocused = candidate;
                        _directlyFocused = false;
                    }
                }
            }


            InteractionContext actualContext = _directlyFocused ? directContext : context;
            if (!ReferenceEquals(_focused, nextFocused))
            {
                if (_focused is IFocusable oldFocusable)
                    oldFocusable.OnFocusExit(actualContext);
                if (nextFocused is IFocusable nextFocusable)
                    nextFocusable.OnFocusEnter(actualContext);
                _focused = nextFocused;
            }

            bool hasFocus = _focused != null;
            bool hasDisplay = false;
            if (hasFocus && _focused is IInteractionDisplay display)
            {
                var info = display.GetInteractionPointDisplay(actualContext);
                var point = info.position;
                var text = info.text;
                _interactionPrompt.transform.position = point;
                _interactionPrompt.SetText(text);
                _interactionPrompt.gameObject.SetActive(true);
            }
            else
                _interactionPrompt.gameObject.SetActive(false);


            for (int i = 0; i < _removeBuffer.Count; i++)
                _candidates.Remove(_removeBuffer[i]);

            _removeBuffer.Clear();
        }

        private void OnEnable()
        {
            _trigger.TriggerEntered += ObjectEntered;
            _trigger.TriggerExited += ObjectExited;
            _inputService.OnInteractPressed += OnInteract;
        }

        private void OnDisable()
        {
            _trigger.TriggerEntered -= ObjectEntered;
            _trigger.TriggerExited -= ObjectExited;
            _inputService.OnInteractPressed -= OnInteract;
        }

        private void OnInteract()
        {
            InteractionContext context = new InteractionContext(_interactorForward.position, _interactorForward.forward, _directlyFocused);
            if (_focused != null)
                _focused.Interact(context);
        }

        private void ObjectExited(Collider collider)
        {
            if (collider.TryGetComponent<IInteractable>(out var interactable))
            {
                _candidates.Remove(collider.gameObject.GetEntityId());
            }
        }

        private void ObjectEntered(Collider collider)
        {
            if (collider.TryGetComponent<IInteractable>(out var interactable))
            {
                _candidates.Add(collider.gameObject.GetEntityId(), interactable);
            }
        }
    }
}