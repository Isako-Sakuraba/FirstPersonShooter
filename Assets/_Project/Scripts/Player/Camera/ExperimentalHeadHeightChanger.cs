using Game.Movement;
using Game.Movement.Player;
using PrimeTween;
using UnityEngine;

namespace Game.Player.Experimental
{
    public class ExperimentalHeadHeightChanger : MonoBehaviour
    {
        [SerializeField] private LocomotionController _player;
        [SerializeField] private float _topOffset = 0.2f;
        [SerializeField] private float _duration = 0.2f;
        [SerializeField] private Ease _ease = Ease.OutBounce;

        private Tween _tween;

        private void Start()
        {
            OnStanceChanged(_player.Snapshot.Stance, _player.Snapshot.Height);
        }

        private void OnEnable()
        {
            _player.OnStanceChanged += OnStanceChanged;
        }

        private void OnDisable()
        {
            _player.OnStanceChanged -= OnStanceChanged;
        }

        private void OnStanceChanged(Stance stance, float height)
        {
            float target = height - _topOffset;
            Vector3 offset = new Vector3(0f, target, 0f);

            _tween.Stop();
            _tween = Tween.LocalPositionY(transform, target, _duration, ease: _ease);
        }
    }
}