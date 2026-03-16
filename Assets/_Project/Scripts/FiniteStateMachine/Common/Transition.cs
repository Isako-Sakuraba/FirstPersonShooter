using FiniteStateMachine.Core;

namespace FiniteStateMachine.Common
{
    public abstract class Transition : ITransition
    {
        public abstract bool Evaluate();
    }
}
