using FiniteStateMachine.Core;

namespace Game.Player.Movement
{
    public enum WallState
    {
        Run,
        Jump
    }

    public class WallrunStateMachine : StateMachine<WallState> { }
}