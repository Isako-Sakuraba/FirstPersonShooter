using UnityEngine;

namespace Game.Enemies
{
    public class PlayerLocator : MonoBehaviour
    {
        [SerializeField] private Transform _player;

        private static Transform _playerTransform;
        private static Vector3 _playerPosition;

        public static bool HasPlayer => _playerTransform != null;
        public static Vector3 Position => _playerPosition;
        public static Transform PlayerTransform => _playerTransform;

        private void Awake()
        {
            if (_player == null)
            {
                _player = transform;
            }

            _playerTransform = _player;
            _playerPosition = _player.position;
        }

        private void Update()
        {
            if (_player == null)
            {
                return;
            }

            if (_playerTransform != _player)
            {
                _playerTransform = _player;
            }

            _playerPosition = _player.position;
        }

        private void OnDisable()
        {
            if (_playerTransform == _player)
            {
                _playerTransform = null;
            }
        }
    }
}
