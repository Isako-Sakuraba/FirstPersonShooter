using FiniteStateMachine.Common;

namespace Game.Movement
{
    public abstract class LocomotionStateBase : State
    {
        protected LocomotionContext context;

        public LocomotionStateBase(LocomotionContext context)
        {
            this.context = context;
        }
    }
}