using Game.Movement.API.Requests;
using Game.Utils;
using ImprovedTimers;
using System;
using UnityEngine;
using UnityEngine.Splines;

namespace Game.Movement
{
    public class LocomotionState
    {
        public readonly KinematicsRuntime Kinematics = new();
        public readonly JumpRuntime Jump = new();
        public readonly WallrunRuntime Wallrun = new();
        public readonly RailRuntime Rail = new();
        public readonly GrappleRuntime Grapple = new();

        public sealed class KinematicsRuntime
        {
            public Vector3 Velocity;
            public Vector3 PreviousVelocity;
        }

        public sealed class JumpRuntime
        {
            public int JumpsLeft;
            public CountdownTimer CoyoteTimer;

            public Pending<JumpPayload> Payload;
        }

        public sealed class WallrunRuntime
        {
            public Vector3 LastWallNormal;
            public bool WallJump;
            public float WallJumpCurrentHeight;
            public CountdownTimer BeginCooldown;
            public CountdownTimer DurationTimer;
        }

        public sealed class RailRuntime
        {
            public bool IsAttached;
            public SplineContainer Spline;

            // Tracking is still manual (not fully velocity-based)
            public float T;
            public float Speed;

            public Pending<RailAttachPayload> AttachmentPayload;
            public Pending<RailDetachPayload> DetachmentPayload;
        }

        public sealed class GrappleRuntime
        {
            public bool IsGrappled;
            public Vector3 LocalPoint;
            public Transform Target;
            public Vector3 WorldPoint;

            public Action<bool> OnGrappleStatusChanged = delegate { };

            public Pending<GrappleAttachPayload> AttachmentPayload;
            public Pending<GrappleDetachPayload> DetachmentPayload;
        }
    }

    public enum JumpKind
    {
        Normal,
        Wall,
        Rail
    }

    public readonly struct JumpPayload
    {
        public readonly JumpKind Kind;
        public readonly Vector3 WallNormal;
        public readonly bool HasWallNormal;
        public readonly UnityEngine.Splines.SplineContainer RailSpline; // optional
        public readonly float RailT;                                   // optional

        public JumpPayload(JumpKind kind,
            Vector3 wallNormal = default, bool hasWallNormal = false,
            UnityEngine.Splines.SplineContainer railSpline = null, float railT = 0f)
        {
            Kind = kind;
            WallNormal = wallNormal;
            HasWallNormal = hasWallNormal;
            RailSpline = railSpline;
            RailT = railT;
        }
    }

    public readonly struct RailAttachPayload
    {
        public readonly SplineContainer Spline;
        public readonly float StartT;

        public RailAttachPayload(SplineContainer spline, float startT)
        {
            Spline = spline;
            StartT = startT;
        }
    }

    public readonly struct RailDetachPayload { }

    public readonly struct GrappleAttachPayload
    {
        public readonly GrappleType Type;
        public readonly Vector3 LocalPoint;
        public readonly Transform Target;
        public readonly float MaxLength;

        public GrappleAttachPayload(GrappleType type, Vector3 localPoint, Transform target, float maxLength)
        {
            Type = type;
            LocalPoint = localPoint;
            Target = target;
            MaxLength = maxLength;
        }
    }

    public readonly struct GrappleDetachPayload { }
}