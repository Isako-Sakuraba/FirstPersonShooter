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
            var jumpState = new JumpState(context);
            var fallState = new FallState(context);

            // Register HFSMs
            machine.AddState(LocomotionState.Grounded, groundedFSM);
            machine.AddState(LocomotionState.Airborne, airborneFSM);

            // Add grounded states
            groundedFSM.AddState(GroundedState.Move, moveState);
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
            var groundedTransition = 
                new Trans(context, 
                (ctx) =>
                {
                    return ctx.IsGrounded;
                });

            var airTransition =
                new Trans(context,
                (ctx) =>
                {
                    return !ctx.IsGrounded;
                });

            var jumpTransition =
                new Trans(context,
                (ctx) =>
                {
                    return ctx.Input.Jump && ctx.IsGrounded;
                });

            // Any/Enter
            machine.AddEnterTransition(
                LocomotionState.Airborne,
                airTransition);
            machine.AddEnterTransition(
                LocomotionState.Grounded,
                groundedTransition);


            // Grounded - Air
            machine.AddAnyTransition(
                LocomotionState.Airborne,
                jumpTransition
            );

            machine.AddAnyTransition(
                LocomotionState.Airborne,
                airTransition);

            machine.AddAnyTransition(
                LocomotionState.Grounded,
                groundedTransition);

            //machine.AddAnyTransition(
            //    LocomotionState.Airborne,
            //    jumpDebugger);
        }

        private static void CreateGroundedTransitions(GroundedStateMachine machine, PlayerContext context)
        {
            var idleToWalk = new Trans(context,
                (ctx) =>
                {
                    return ctx.Input.Move.sqrMagnitude != 0f;
                });

            var walkToIdle = new ReverseTransition(idleToWalk);

            var jumpTransition =
                new Trans(context,
                (ctx) =>
                {
                    return ctx.Input.Jump;
                });

            //machine.AddAnyExitTransition(jumpTransition);

            //var anyToExit = new Trans(context, 
            //    (ctx) =>
            //    {
            //        return !ctx.State.IsGrounded;
            //    });

            //machine.AddAnyExitTransition(anyToExit);
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