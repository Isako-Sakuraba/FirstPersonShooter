using Game.Player.Movement;
using System;
using UnityEngine;

namespace Game.Player.Experimental
{
    public class ExperimentalHeadHeightChanger : MonoBehaviour
    {
        [SerializeField] private PlayerMovement _player;
        [SerializeField] private float _topOffset = 0.2f;

        private BodyController _body;

        private void Start()
        {
            _body = _player.Conext.Body;
            _body.OnStanceChanged += OnStanceChanged;
        }

        private void OnStanceChanged(Stance stance)
        {
            float target = _body.Height - _topOffset;
            Vector3 offset = new Vector3(0f, target, 0f);
            transform.localPosition = offset;
        }
    }
}