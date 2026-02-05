using System.Collections.Generic;

namespace FiniteStateMachine.Core
{
    public abstract partial class StateMachine<EStateId>
    {
        private class StateNode
        {
            public readonly EStateId Id;
            public readonly IState State;
            public readonly List<TransitionNode> Transitions;

            public StateNode(EStateId id, IState state)
            {
                Id = id;
                State = state;
                Transitions = new List<TransitionNode>();
            }

            public void AddTransition(EStateId to, ITransition transition, bool immediate)
            {
                Transitions.Add(new TransitionNode(to, transition, immediate));
            }
        }
    }
}
