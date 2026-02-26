using UnityEngine;

namespace Game.Movement.API
{
    public interface ILocomotionInfo
    {
        Vector3 Forward { get; }
        Vector3 Right { get; }

        Vector3 Velocity { get; }

        bool IsSliding { get; }
        bool IsWallrunning { get; }
        bool IsGrounded { get; }

        bool HasWallContact { get; }
        Vector3 WallNormal { get; }
    }
}