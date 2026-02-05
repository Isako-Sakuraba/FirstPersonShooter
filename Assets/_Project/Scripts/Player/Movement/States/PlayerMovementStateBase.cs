using FiniteStateMachine.Common;

namespace Game.Player.Movement.States
{
    public abstract class PlayerMovementStateBase : State
    {
        protected PlayerContext context;

        public PlayerMovementStateBase(PlayerContext context)
        {
            this.context = context;
        }
    }
}