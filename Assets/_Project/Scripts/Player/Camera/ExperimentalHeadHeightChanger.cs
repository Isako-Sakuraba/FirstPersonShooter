using Game.Player.Movement;
using PrimeTween;
using UnityEngine;

namespace Game.Player.Experimental
{
    public class ExperimentalHeadHeightChanger : MonoBehaviour
    {
        [SerializeField] private PlayerMovement _player;
        [SerializeField] private float _topOffset = 0.2f;
        [SerializeField] private float _duration = 0.2f;
        [SerializeField] private Ease _ease = Ease.OutBounce;

        private BodyController _body;

        private Tween _tween;

        private void Start()
        {
            _body = _player.Conext.Body;
            _body.OnStanceChanged += OnStanceChanged;
            OnStanceChanged(_body.Stance);
        }

        private void OnStanceChanged(Stance stance)
        {
            float target = _body.Height - _topOffset;
            Vector3 offset = new Vector3(0f, target, 0f);

            _tween.Stop();
            _tween = Tween.LocalPositionY(transform, target, _duration, ease: _ease);
        }
    }
}