using FiniteStateMachine.Core;

namespace Game.Player.Movement
{
    public enum AirborneState
    {
        Jump,
        Fall
    }

    public class AirborneStateMachine : StateMachine<AirborneState> { }
}