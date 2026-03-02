using System;
using UnityEngine;

namespace Game.Movement.API
{
    public interface IReadOnlyLocomotionController
    {
        Vector3 Forward { get; }
        Vector3 Right { get; }

        Vector3 Velocity { get; }
        Vector3 HorizontalVelocity => new Vector3(Velocity.x, 0f, Velocity.z);
        float VerticalVelocity => Velocity.y;
        float Speed => Velocity.magnitude;

        Vector3 GroundNormal { get; }

        IBodyState Body { get; }

        LocomotionMachineState MachineState { get; }
        bool IsGrounded { get; }
        bool IsSliding { get; }
        bool IsWallrunning { get; }
        bool IsRailgrinding { get; }
        bool IsGrappling { get; }

        bool HasWallContact { get; }
        Vector3 WallNormal { get; }

        Vector3 PivotWorldPoint { get; }
        event Action OnGrappleAttached;
        event Action OnGrappleDetached;

        string GetMachinePath();
    }
}