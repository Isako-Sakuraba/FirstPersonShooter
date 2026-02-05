using FiniteStateMachine.Common;
using FiniteStateMachine.Core;
using Game.Player.Movement.States;
using UnityEngine;
using Trans = FiniteStateMachine.Common.LambdaTransition<Game.Player.Movement.PlayerContext>;
using TBundle = FiniteStateMachine.Common.TransitionBundle;

namespace Game.Player.Movement
{
    public enum PlayerMovementStateId
    {
        Grounded,
        Airborne
    }

    public class PlayerMovementStateMachine : StateMachine<PlayerMovementStateId> 
    {
        private PlayerContext _context;

        public GroundedStateMachine GFSM;
        public AirborneStateMachine AFSM;

        public PlayerMovementStateMachine(PlayerContext context)
        {
            _context = context;
        }

        public static PlayerMovementStateMachine CreateDefault(PlayerContext context)
        {
            PlayerMovementStateMachine machine = new(context);

            CreateStatesAndTransition(machine, context);

            return machine;
        }

        private static void CreateStatesAndTransition(PlayerMovementStateMachine machine, PlayerContext context)
        {
            var groundedFSM = new GroundedStateMachine();
            var airborneFSM = new AirborneStateMachine();

            machine.GFSM = groundedFSM;
            machine.AFSM = airborneFSM;

            var idleState = new IdleState(context);
            var walkState = new WalkState(context);
            var jumpState = new JumpState(context);
            var fallState = new FallState(context);


            machine.AddState(PlayerMovementStateId.Grounded, groundedFSM);
            machine.AddState(PlayerMovementStateId.Airborne, airborneFSM);

            groundedFSM.AddState(GroundedStateId.Idle, idleState);
            groundedFSM.AddState(GroundedStateId.Walk, walkState);
            groundedFSM.Run(GroundedStateId.Idle);

            airborneFSM.AddState(AirborneStateId.Jump, jumpState);
            airborneFSM.AddState(AirborneStateId.Fall, fallState);
            airborneFSM.Run(AirborneStateId.Jump);


            CreateRootTransitions(machine, groundedFSM, airborneFSM, context);
            CreateGroundedTransitions(groundedFSM, context);
            CreateAirborneTransitions(airborneFSM, context);

            machine.Run(PlayerMovementStateId.Airborne);
        }

        private static void CreateRootTransitions(
            PlayerMovementStateMachine machine,
            GroundedStateMachine groundFSM,
            AirborneStateMachine airFSM,
            PlayerContext context)
        {
            var groundedTransition = 
                new Trans(context, 
                (ctx) =>
                {
                    Debug.Log("Checked Any if grounded!");
                    return ctx.State.IsGrounded;
                });

            var airTransition =
                new Trans(context,
                (ctx) =>
                {
                    Debug.Log("Checked Any if not grounded!");
                    return !ctx.State.IsGrounded;
                });

            var jumpTransition =
                new Trans(context,
                (ctx) =>
                {
                    return ctx.Input.Jump;
                });

            var jumpDebugger = new LambdaActionTransition(jumpTransition, () => Debug.Log("Jump evaluated!"));



            // Any/Enter
            machine.AddEnterTransition(
                PlayerMovementStateId.Airborne,
                airTransition);
            machine.AddEnterTransition(
                PlayerMovementStateId.Grounded,
                groundedTransition);


            // Grounded - Air
            machine.AddAnyTransition(
                PlayerMovementStateId.Airborne,
                jumpTransition
            );

            machine.AddAnyTransition(
                PlayerMovementStateId.Grounded,
                groundedTransition);

            machine.AddAnyTransition(
                PlayerMovementStateId.Airborne,
                airTransition);

            //machine.AddAnyTransition(
            //    PlayerMovementStateId.Airborne,
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

            machine.AddTransition(
                GroundedStateId.Idle,
                GroundedStateId.Walk,
                idleToWalk);

            machine.AddTransition(
                GroundedStateId.Walk,
                GroundedStateId.Idle,
                walkToIdle);

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

            machine.AddEnterTransition(AirborneStateId.Jump, fromJump);
            machine.AddEnterTransition(AirborneStateId.Fall, notFromJump);

            var jumpToFall = new Trans(context,
            (ctx) =>
            {
                return ctx.State.Velocity.y <= 0;
            });

            machine.AddTransition(AirborneStateId.Jump, AirborneStateId.Fall, jumpToFall);
        }
    }
}