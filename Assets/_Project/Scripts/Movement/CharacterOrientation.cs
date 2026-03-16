using UnityEngine;
using static UnityEngine.InputSystem.Controls.AxisControl;

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
        public float Pitch => _yaw;

        public Vector3 ForwardFlat => Quaternion.Euler(0f, _yaw, 0f) * Vector3.forward;
        public Vector3 RightFlat => Quaternion.Euler(0f, _yaw, 0f) * Vector3.right;

        public Vector3 Forward => Quaternion.Euler(_pitch, _yaw, 0f) * Vector3.forward;
        public Vector3 Right => Quaternion.Euler(_pitch, _yaw, 0f) * Vector3.right;

        public void ResetRotation()
        {
            _yaw = 0f;
            _pitch = 0f;
        }

        public void UpdateYaw(float delta)
        {
            _yaw += delta;
        }

        public void UpdatePitch(float delta)
        {
            _pitch += delta;
        }

        public void UpdatePitch(float delta, Vector2 clamp)
        {
            _pitch += delta;
            _pitch = Mathf.Clamp(_pitch, clamp.x, clamp.y);
        }

        public void UpdateAll(float deltaYaw, float deltaPitch, Vector2 pitchClamp)
        {
            UpdateYaw(deltaYaw);
            UpdatePitch(deltaPitch, pitchClamp);
        }
    }
}