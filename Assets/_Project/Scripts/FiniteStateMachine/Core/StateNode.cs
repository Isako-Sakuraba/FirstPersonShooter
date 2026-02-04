using System.Collections.Generic;

namespace FiniteStateMachine.Core
{
    public partial class StateMachine<EStateId>
    {
        private class StateNode
        {
            public readonly EStateId Id;
            public readonly IState State;
            public readonly HashSet<TransitionNode> Transitions;

            public StateNode(EStateId id, IState state)
            {
                Id = id;
                State = state;
                Transitions = new HashSet<TransitionNode>();
            }

            public void AddTransition(EStateId to, ITransition transition, bool immediate)
            {
                Transitions.Add(new TransitionNode(to, transition, immediate));
            }
        }
    }
}
