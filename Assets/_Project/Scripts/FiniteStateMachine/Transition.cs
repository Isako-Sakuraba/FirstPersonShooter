using System;

namespace FiniteStateMachine
{
    public partial class StateMachine<TStateId, TContext, TEvent>
    {
        private class Transition
        {
            public readonly TStateId To;
            public readonly IPredicate Condition;
            public bool ImmediateUpdate;

            public Transition(TStateId to, IPredicate condition, bool immediateUpdate)
            {
                To = to;
                Condition = condition;
                ImmediateUpdate = immediateUpdate;
            }
        }

        private class TriggerTransition
        {
            public readonly TStateId To;
            public bool ImmediateUpdate;

            public TriggerTransition(TStateId to, bool immediateUpdate)
            {
                To = to;
                ImmediateUpdate = immediateUpdate;
            }
        }
    }
}
