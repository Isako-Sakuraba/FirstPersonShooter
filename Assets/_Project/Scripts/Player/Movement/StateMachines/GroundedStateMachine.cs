using FiniteStateMachine.Core;

namespace Game.Player.Movement
{
    public enum GroundedStateId
    {
        Idle,
        Walk
    }

    public class GroundedStateMachine : StateMachine<GroundedStateId> { }
}