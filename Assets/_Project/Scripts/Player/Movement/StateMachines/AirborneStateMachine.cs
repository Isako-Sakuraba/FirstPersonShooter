using FiniteStateMachine.Core;

namespace Game.Player.Movement
{
    public enum AirborneStateId
    {
        Jump,
        Fall
    }

    public class AirborneStateMachine : StateMachine<AirborneStateId> { }
}