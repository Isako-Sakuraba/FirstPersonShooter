using FiniteStateMachine.Core;

namespace Game.Player.Movement
{
    public enum GroundedState
    {
        Move, // Idle, walking, running, crouching
        Slide
    }

    public class GroundedStateMachine : StateMachine<GroundedState> { }
}