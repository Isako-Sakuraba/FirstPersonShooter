using FiniteStateMachine.Core;

namespace FiniteStateMachine.Common
{
    public class ReverseTransition : ITransition
    {
        private ITransition _target;

        public ReverseTransition(ITransition target)
        {
            _target = target;
        }

        public bool Evaluate()
            => !_target.Evaluate();
    }
}
