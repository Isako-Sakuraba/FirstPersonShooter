namespace FiniteStateMachine.Core
{
    public partial class StateMachine<EStateId>
    {
        private class TransitionNode
        {
            public readonly EStateId To;
            public readonly ITransition Transition;
            public readonly bool Immediate;

            public TransitionNode(EStateId to, ITransition transition, bool immediate)
            {
                To = to;
                Transition = transition;
                Immediate = immediate;
            }

            public bool Evaluate()
            {
                return Transition.Evaluate();
            }
        }
    }
}
