using Game.Movement;
using PrimeTween;
using UnityEngine;

namespace Game.Experimental
{
    public class ExperimentalHeadHeightChanger : MonoBehaviour
    {
        [SerializeField] private LocomotionController _player;
        [SerializeField] private float _topOffset = 0.2f;
        [SerializeField] private float _duration = 0.2f;
        [SerializeField] private Ease _ease = Ease.OutBounce;

        private Tween _tween;

        private IBodyState _bodyState;

        private void Start()
        {
            _bodyState = _player.Body;
            OnStanceChanged(_player.Body.Stance);
            _player.Body.OnStanceChanged += OnStanceChanged;
        }

        private void OnEnable()
        {
            if (_player.Body != null)
                _player.Body.OnStanceChanged += OnStanceChanged;
        }

        private void OnDisable()
        {
            _player.Body.OnStanceChanged -= OnStanceChanged;
        }

        private void OnStanceChanged(Stance stance)
        {
            float target = _bodyState.Height - _topOffset;
            Vector3 offset = new Vector3(0f, target, 0f);

            _tween.Stop();
            _tween = Tween.LocalPositionY(transform, target, _duration, ease: _ease);
        }
    }
}