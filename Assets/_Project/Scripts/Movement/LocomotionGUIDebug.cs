using Game.Movement.Player;
using System;
using UnityEngine;

namespace Game.Movement.Debugging
{
    public class LocomotionGUIDebug : MonoBehaviour
    {
        [SerializeField] private PlayerController _target;
        [SerializeField] private float _sizeMultiplier = 2f;
        [SerializeField] private DebugProperty _debugVelocity = new DebugProperty(Color.red, true, 1f);
        [SerializeField] private DebugProperty _debugState = new DebugProperty(Color.blue, true, 1f);
        [SerializeField] private DebugProperty _debugTimers = new DebugProperty(Color.darkBlue, true, 1f);

        private void OnGUI()
        {
            var pos = Vector3.zero;
            var rot = Quaternion.identity;
            var scale = Vector3.one * _sizeMultiplier;

            if (_debugVelocity.Enabled)
            {
                GUI.color = _debugVelocity.Color;
                GUI.matrix = Matrix4x4.TRS(pos, rot, scale * _debugVelocity.Size);
                GUILayout.Label($"Velocity: {_target.Velocity}");
                var horizontal = new Vector2(_target.Velocity.x, _target.Velocity.z);
                GUILayout.Label($"Horizontal speed: {horizontal.magnitude}");
                GUILayout.Label($"Vertical speed: {_target.Velocity.y}");
            }

            if (_debugState.Enabled)
            {
                GUI.color = _debugState.Color;
                GUI.matrix = Matrix4x4.TRS(pos, rot, scale * _debugState.Size);
                GUILayout.Label($"State Machine State: {_target.Snapshot.MachineState}");
                GUILayout.Label($"Stance: {_target.Snapshot.Stance}");
                GUILayout.Label($"Grounded: {_target.Snapshot.IsGrounded}");
                GUILayout.Label($"IsWallrunning: {_target.IsWallrunning}");
                GUILayout.Label($"IsSliding: {_target.IsSliding}");
                GUILayout.Label($"IsOnRail: {_target.IsOnRail}");
            }

            // TODO: Add timers debug
        }
    }

    [Serializable]
    public struct DebugProperty
    {
        public Color Color;
        public bool Enabled;
        public float Size;

        public DebugProperty(Color color, bool enabled, float size)
        {
            Color = color;
            Enabled = enabled;
            Size = size;
        }
    }
}