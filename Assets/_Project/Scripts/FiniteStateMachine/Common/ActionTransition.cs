using FiniteStateMachine.Core;

namespace FiniteStateMachine.Common
{
    public abstract class ActionTransition : ITransition
    {
        public readonly ITransition Target;

        public ActionTransition(ITransition target)
        {
            Target = target;
        }

        public bool Evaluate()
        {
            bool targetEvaluated = Target.Evaluate();
            if (targetEvaluated)
                OnTransition();
            return targetEvaluated;
        }

        public abstract void OnTransition();
    }
}
