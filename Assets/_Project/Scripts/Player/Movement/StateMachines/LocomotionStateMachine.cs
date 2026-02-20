using FiniteStateMachine.Common;
using FiniteStateMachine.Core;
using Game.Player.Movement.States;
using UnityEngine;
using Trans = FiniteStateMachine.Common.LambdaTransition<Game.Player.Movement.PlayerContext>;
using TBundle = FiniteStateMachine.Common.TransitionBundle;

namespace Game.Player.Movement
{
    public enum LocomotionState
    {
        Grounded,
        Airborne
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

            machine.GFSM = groundedFSM;
            machine.AFSM = airborneFSM;

            var moveState = new MoveState(context);
            var slideState = new SlideState(context);
            var jumpState = new JumpState(context);
            var fallState = new FallState(context);

            // Register HFSMs
            machine.AddState(LocomotionState.Grounded, groundedFSM);
            machine.AddState(LocomotionState.Airborne, airborneFSM);

            // Add grounded states
            groundedFSM.AddState(GroundedState.Move, moveState);
            groundedFSM.AddState(GroundedState.Slide, slideState);
            groundedFSM.Run(GroundedState.Move);

            // Add airborne states
            airborneFSM.AddState(AirborneState.Jump, jumpState);
            airborneFSM.AddState(AirborneState.Fall, fallState);
            airborneFSM.Run(AirborneState.Fall);


            CreateRootTransitions(machine, groundedFSM, airborneFSM, context);
            CreateGroundedTransitions(groundedFSM, context);
            CreateAirborneTransitions(airborneFSM, context);

            machine.Run(LocomotionState.Airborne);
        }

        private static void CreateRootTransitions(
            LocomotionStateMachine machine,
            GroundedStateMachine groundFSM,
            AirborneStateMachine airFSM,
            PlayerContext context)
        {
            var toAir = new Trans(context, ctx => !ctx.IsGrounded);
            var toGround = new Trans(context, ctx => ctx.IsGrounded && !ctx.Input.Jump);
            // ^ prevent instant "snap back" to Grounded on the jump press frame

            var jumpToAir = new Trans(context, ctx => ctx.Input.Jump && ctx.IsGrounded);

            // Enter transitions (initial selection)
            machine.AddEnterTransition(LocomotionState.Airborne, toAir);
            machine.AddEnterTransition(LocomotionState.Grounded, toGround);

            // Any transitions (runtime)
            machine.AddAnyTransition(LocomotionState.Airborne, jumpToAir); // priority
            machine.AddAnyTransition(LocomotionState.Airborne, toAir);
            machine.AddAnyTransition(LocomotionState.Grounded, toGround);
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
                HorizontalSpeed(ctx) >= ctx.Data.MinEnterSpeed
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
                    return ctx.Input.Jump;
                });

            var notFromJump = new ReverseTransition(fromJump);


            machine.AddEnterTransition(AirborneState.Fall, notFromJump);
            machine.AddEnterTransition(AirborneState.Jump, fromJump);

            var trueTransition = new LambdaTransition(() => true);

            machine.AddTransition(AirborneState.Jump, AirborneState.Fall, trueTransition);
        }
    }
}