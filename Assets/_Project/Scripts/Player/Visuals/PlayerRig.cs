using Game.Movement;
using UnityEngine;

namespace Game.Player
{
    public class PlayerRig : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private CharacterOrientation _orientation;
        [SerializeField] private Transform _target;
        [SerializeField] private Transform _player;

        [Header("Settings")]
        [SerializeField] private float _positionLerpFactor = 40;
        [SerializeField] private float _rotationLerpFactor = 40;

        private void Update()
        {
            LerpPositionAndRotationToPlayer();
        }

        private void LerpPositionAndRotationToPlayer()
        {
            transform.position = Vector3.Lerp(transform.position, _player.position, _positionLerpFactor * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, _orientation.RotationFlat, _rotationLerpFactor * Time.deltaTime);
        }
    }
}