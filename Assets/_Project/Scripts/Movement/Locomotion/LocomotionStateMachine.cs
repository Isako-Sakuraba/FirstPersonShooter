using FiniteStateMachine.Common;
using FiniteStateMachine.Core;
using Game.Movement.States;
using UnityEngine;
using LMS = Game.Movement.LocomotionMachineState;
using Trans = FiniteStateMachine.Common.LambdaTransition<Game.Movement.LocomotionContext>;
using ATrans = FiniteStateMachine.Common.LambdaActionTransition<Game.Movement.LocomotionContext>;

namespace Game.Movement
{
    public enum LocomotionMachineState
    {
        Grounded,
        Air,
        Jump,
        Wall,
        Rail
    }

    public class LocomotionStateMachine : StateMachine<LocomotionMachineState>
    {
        private LocomotionContext _context;

        private GroundedStateMachine _groundMachine;

        public GroundedState GroundedState => _groundMachine.CurrentState;

        public LocomotionStateMachine(LocomotionContext context)
        {
            _context = context;
        }

        public void CreateDefault(LocomotionContext context)
        {
            CreateStatesAndTransition(context);
        }

        private void CreateStatesAndTransition(LocomotionContext context)
        {
            var machine = this;

            var groundedFSM = new GroundedStateMachine();
            _groundMachine = groundedFSM;

            var moveState = new MoveState(context);
            var slideState = new SlideState(context);
            var jumpState = new JumpState(context);
            var fallState = new AirState(context);
            var wallRunState = new WallState(context);
            var railState = new RailState(context);

            // Register HFSMs
            machine.AddState(LMS.Grounded, groundedFSM);
            machine.AddState(LMS.Air, fallState);
            machine.AddState(LMS.Jump, jumpState);
            machine.AddState(LMS.Wall, wallRunState);
            machine.AddState(LMS.Rail, railState);

            // Add grounded states
            groundedFSM.AddState(GroundedState.Move, moveState);
            groundedFSM.AddState(GroundedState.Slide, slideState);
            groundedFSM.Run(GroundedState.Move);

            CreateRootTransitions(context);
            CreateGroundedTransitions(context);

            machine.Run(LMS.Air);
        }

        private void CreateRootTransitions(
            LocomotionContext context)
        {
            var machine = this;

            var toWall = new Trans(context, ctx =>
            {
                bool sameWallCondition = true;
                if (ctx.State.Wallrun.LastWallNormal == ctx.Sensors.WallCollision.normal)
                    sameWallCondition = !ctx.State.Wallrun.BeginCooldown.IsRunning;

                return !ctx.IsGrounded && ctx.Sensors.WallDetected && sameWallCondition;
            });
            var toAir = new Trans(context, ctx => !ctx.IsGrounded);
            var toGround = new Trans(context, ctx => ctx.IsGrounded);

            // Coyote time
            var groundToAir = new Trans(context, ctx => !ctx.IsGrounded);
            var coyoteTimeActionTrans = new ATrans(groundToAir, context, ctx => ctx.State.Jump.CoyoteTimer.Start());
            context.State.Jump.CoyoteTimer.OnTimerFinished += () => context.State.Jump.JumpsLeft--;


            var toAirFromWall = new Trans(context, ctx => !ctx.IsGrounded && !ctx.Sensors.WallDetected);


            var trueTrans = new LambdaTransition(() => true);

            // Enter transitions (initial selection)
            machine.AddEnterTransition(LMS.Air, toAir);
            machine.AddEnterTransition(LMS.Grounded, toGround);

            // Any transitions (runtime)
            var jumpToAir = new Trans(context, ctx => ctx.Input.JumpBufferTimer.IsRunning && ctx.State.Jump.JumpsLeft > 0);
            var jumpTransition = new ATrans(jumpToAir, context, ctx => ctx.State.Jump.Payload.Set(new JumpPayload(JumpKind.Normal)));
            machine.AddTransition(LMS.Grounded, LMS.Jump, jumpTransition, true);
            machine.AddTransition(LMS.Air, LMS.Jump, jumpTransition, true);
            machine.AddTransition(LMS.Grounded, LMS.Air, coyoteTimeActionTrans);
            machine.AddTransition(LMS.Air, LMS.Grounded, toGround);

            // Wallrun transition
            var wallToJump = new Trans(context, ctx => ctx.Input.JumpBufferTimer.IsRunning && ctx.State.Jump.JumpsLeft > 0);
            var wallJumpTransition = new ATrans(wallToJump, context, 
                ctx => ctx.State.Jump.Payload.Set(new JumpPayload(JumpKind.Wall, ctx.State.Wallrun.LastWallNormal)));
            var wallToFall = new Trans(context, ctx => !ctx.Sensors.WallDetected);
            machine.AddTransition(LMS.Air, LMS.Wall, toWall);
            machine.AddTransition(LMS.Wall, LMS.Jump, wallJumpTransition, true);
            machine.AddTransition(LMS.Wall, LMS.Grounded, toGround);
            machine.AddTransition(LMS.Wall, LMS.Air, wallToFall);

            // JumpPressed transitions

            machine.AddTransition(LMS.Jump, LMS.Air, trueTrans);

            var toRail = new Trans(context, ctx => ctx.State.Rail.AttachmentPayload.IsPresent);
            var fromRailToJump = new Trans(context, ctx => ctx.Input.JumpBufferTimer.IsRunning);
            var fromRailEnded = new Trans(context, ctx => ctx.State.Rail.T == 1f || ctx.State.Rail.T == 0f);

            var railJumpTransition = new ATrans(fromRailToJump, context, ctx => ctx.State.Jump.Payload.Set(new(JumpKind.Rail)));

            machine.AddAnyTransition(LMS.Rail, toRail);

            machine.AddTransition(LMS.Rail, LMS.Air, fromRailEnded);
            machine.AddTransition(LMS.Rail, LMS.Jump, railJumpTransition, true);
        }

        private void CreateGroundedTransitions(LocomotionContext context)
        {
            var machine = _groundMachine;

            // Helpers
            float HorizontalSpeed(LocomotionContext ctx)
            {
                var v = ctx.State.Kinematics.Velocity;
                return new Vector3(v.x, 0f, v.z).magnitude;
            }

            // Move -> Slide
            var toSlide = new Trans(context, ctx =>
                ctx.IsGrounded &&
                ctx.Input.CrouchHeld &&                              // edge-triggered
                HorizontalSpeed(ctx) >= ctx.Data.Slide.RequiredEnterSpeed
            );

            // Slide -> Move
            var toMove = new Trans(context, ctx =>
                {
                    if (!ctx.IsGrounded) return false;

                    Vector3 v = ctx.State.Kinematics.Velocity;
                    float speed = new Vector3(v.x, 0f, v.z).magnitude;

                    if (speed < ctx.Data.Slide.MinExitSpeed || ctx.Body.Stance == Stance.Standing) return true;

                    return false;
                }
            );

            machine.AddTransition(GroundedState.Move, GroundedState.Slide, toSlide);
            machine.AddTransition(GroundedState.Slide, GroundedState.Move, toMove);

            // Note: Slide -> Air is handled by root transitions (jump press or !grounded).
        }
    }
}