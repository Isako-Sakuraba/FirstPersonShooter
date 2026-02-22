using FiniteStateMachine.Common;
using FiniteStateMachine.Core;
using Game.Player.Movement.States;
using UnityEngine;
using Trans = FiniteStateMachine.Common.LambdaTransition<Game.Player.Movement.PlayerContext>;
using ActionTrans = FiniteStateMachine.Common.LambdaActionTransition<Game.Player.Movement.PlayerContext>;
using TBundle = FiniteStateMachine.Common.TransitionBundle;
using System;

namespace Game.Player.Movement
{
    public enum LocomotionState
    {
        Grounded,
        Airborne,
        Wall
    }

    public class LocomotionStateMachine : StateMachine<LocomotionState> 
    {
        private PlayerContext _context;

        public GroundedStateMachine GFSM;
        public AirborneStateMachine AFSM;

        public LocomotionStateMachine(PlayerContext context)
        {
            _context = context;
        }

        public static LocomotionStateMachine CreateDefault(PlayerContext context)
        {
            LocomotionStateMachine machine = new(context);

            CreateStatesAndTransition(machine, context);

            return machine;
        }

        private static void CreateStatesAndTransition(LocomotionStateMachine machine, PlayerContext context)
        {
            var groundedFSM = new GroundedStateMachine();
            var airborneFSM = new AirborneStateMachine();
            var wallrunFSM = new WallrunStateMachine();

            machine.GFSM = groundedFSM;
            machine.AFSM = airborneFSM;

            var moveState = new MoveState(context);
            var slideState = new SlideState(context);
            var jumpState = new JumpState(context);
            var fallState = new FallState(context);
            var wallRunState = new WallRunState(context);
            var wallJumpState = new WallJumpState(context);

            // Register HFSMs
            machine.AddState(LocomotionState.Grounded, groundedFSM);
            machine.AddState(LocomotionState.Airborne, airborneFSM);
            machine.AddState(LocomotionState.Wall, wallrunFSM);


            // Add grounded states
            groundedFSM.AddState(GroundedState.Move, moveState);
            groundedFSM.AddState(GroundedState.Slide, slideState);
            groundedFSM.Run(GroundedState.Move);

            // Add airborne states
            airborneFSM.AddState(AirborneState.Jump, jumpState);
            airborneFSM.AddState(AirborneState.Fall, fallState);
            airborneFSM.Run(AirborneState.Fall);

            // Add wallrun states
            wallrunFSM.AddState(WallState.Run, wallRunState);
            wallrunFSM.AddState(WallState.Jump, wallJumpState);
            wallrunFSM.Run(WallState.Run);


            CreateRootTransitions(machine, groundedFSM, airborneFSM, context);
            CreateGroundedTransitions(groundedFSM, context);
            CreateAirborneTransitions(airborneFSM, context);
            CreateWallrunTransitions(wallrunFSM, context);

            machine.Run(LocomotionState.Airborne);
        }

        private static void CreateRootTransitions(
            LocomotionStateMachine machine,
            GroundedStateMachine groundFSM,
            AirborneStateMachine airFSM,
            PlayerContext context)
        {
            var toWall = new Trans(context, ctx =>
            {
                bool sameWallCondition = true;
                if (ctx.State.LastWallNormal == ctx.Sensors.WallCollision.normal)
                    sameWallCondition = !ctx.State.WallrunBeginTimer.IsRunning;

                return !ctx.IsGrounded && ctx.Sensors.WallDetected && sameWallCondition;
            });
            var toAir = new Trans(context, ctx => !ctx.IsGrounded);
            var toGround = new Trans(context, ctx => ctx.IsGrounded);


            var toAirFromWall = new Trans(context, ctx => !ctx.IsGrounded && !ctx.Sensors.WallDetected);

            var jumpToAir = new Trans(context, ctx => ctx.Input.JumpBufferTimer.IsRunning && ctx.State.JumpsLeft > 0 && ctx.IsGrounded);

            // Enter transitions (initial selection)
            machine.AddEnterTransition(LocomotionState.Airborne, toAir);
            machine.AddEnterTransition(LocomotionState.Grounded, toGround);

            // Any transitions (runtime)
            machine.AddTransition(LocomotionState.Grounded, LocomotionState.Airborne, jumpToAir); // higher priority than normal ground -> air
            machine.AddTransition(LocomotionState.Grounded, LocomotionState.Airborne, toAir);
            machine.AddTransition(LocomotionState.Airborne, LocomotionState.Grounded, toGround);

            // Wallrun transition
            machine.AddTransition(LocomotionState.Airborne, LocomotionState.Wall, toWall);
            machine.AddTransition(LocomotionState.Wall, LocomotionState.Grounded, toGround);
            machine.AddTransition(LocomotionState.Wall, LocomotionState.Airborne, toAirFromWall);
        }

        private static void CreateGroundedTransitions(GroundedStateMachine machine, PlayerContext context)
        {
            // Helpers
            float HorizontalSpeed(PlayerContext ctx)
            {
                var v = ctx.State.Velocity;
                return new Vector3(v.x, 0f, v.z).magnitude;
            }

            // Move -> Slide
            var toSlide = new Trans(context, ctx =>
                ctx.IsGrounded &&
                ctx.Input.Crouch &&                              // edge-triggered
                HorizontalSpeed(ctx) >= ctx.Data.MinSlideEnterSpeed
            );

            // Slide -> Move
            var toMove = new Trans(context, ctx =>
                {
                    if (!ctx.IsGrounded) return false;

                    Vector3 v = ctx.State.Velocity;
                    float speed = new Vector3(v.x, 0f, v.z).magnitude;

                    if (speed < ctx.Data.MinSlideSpeed || ctx.Body.Stance == Stance.Standing) return true;

                    return false;
                }
            );

            machine.AddTransition(GroundedState.Move, GroundedState.Slide, toSlide);
            machine.AddTransition(GroundedState.Slide, GroundedState.Move, toMove);

            // Note: Slide -> Airborne is handled by root transitions (jump press or !grounded).
        }

        private static void CreateAirborneTransitions(AirborneStateMachine machine, PlayerContext context)
        {
            //var anyToExit = new Trans(context,
            //(ctx) =>
            //{
            //    return ctx.State.IsGrounded;
            //});

            //machine.AddAnyToExitTransition(anyToExit);

            var fromJump = new Trans(context,
                (ctx) =>
                {
                    return ctx.Input.JumpBufferTimer.IsRunning && !ctx.State.WallJump;
                });



            var notFromJump = new Trans(context, ctx => !ctx.Input.Jump && !ctx.State.WallJump);

            // COYOTE TIME IS HERE
            var coyoteTimeStartWrapper = new ActionTrans(notFromJump, context, ctx => ctx.State.CoyoteTimer.Start());


            machine.AddEnterTransition(AirborneState.Fall, coyoteTimeStartWrapper);
            machine.AddEnterTransition(AirborneState.Jump, fromJump);

            var trueTransition = new LambdaTransition(() => true);

            // Does not accout for coyote time
            // TODO: account for coyote time, decrease jump count when elapsed
            var coyoteJump = new Trans(context, ctx =>
            {
                return ctx.Input.JumpBufferTimer.IsRunning && ctx.State.JumpsLeft > 0;
            });

            machine.AddTransition(AirborneState.Jump, AirborneState.Fall, trueTransition);
            machine.AddTransition(AirborneState.Fall, AirborneState.Jump, coyoteJump);
        }

        private static void CreateWallrunTransitions(WallrunStateMachine machine, PlayerContext context)
        {
            var fromJump = new Trans(context,
            (ctx) =>
            {
                return ctx.Input.JumpBufferTimer.IsRunning && ctx.State.JumpsLeft > 0;
            });

            var wallRunTimerEnded = new Trans(context, ctx => ctx.State.WallrunEndTimer.IsFinished);


            machine.AddTransition(WallState.Run, WallState.Jump, fromJump);
            machine.AddTransition(WallState.Run, WallState.Jump, wallRunTimerEnded);
        }
    }
}