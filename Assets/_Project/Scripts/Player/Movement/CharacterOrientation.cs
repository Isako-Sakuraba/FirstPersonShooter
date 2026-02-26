using UnityEngine;

namespace Game.Movement
{
    public class CharacterOrientation : MonoBehaviour
    {
        private float _yaw;
        private float _pitch;

        public Quaternion Rotation => transform.rotation;
        public Quaternion RotationFlat => Quaternion.Euler(0f, _yaw, 0f);
        public Vector3 Euler => transform.eulerAngles;
        public float Yaw => _yaw;

        public Vector3 Forward => Quaternion.Euler(0f, _yaw, 0f) * Vector3.forward;
        public Vector3 Right => Quaternion.Euler(0f, _yaw, 0f) * Vector3.right;

        public void ResetRotation()
        {
            _yaw = 0f;
            _pitch = 0f;
        }

        public void UpdateYaw(float delta)
        {
            _yaw += delta;
            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }
    }
}