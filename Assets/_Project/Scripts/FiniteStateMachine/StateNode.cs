using System.Collections.Generic;

namespace FiniteStateMachine
{
    public partial class StateMachine<TStateId, TContext, TEvent>
    {
        private class StateNode
        {
            public readonly BaseState<TStateId, TContext, TEvent> State;
            public readonly HashSet<Transition> Transitions;
            public readonly Dictionary<TEvent, TriggerTransition> Triggers;

            public StateNode(BaseState<TStateId, TContext, TEvent> state)
            {
                State = state;
                Transitions = new HashSet<Transition>();
                Triggers = new Dictionary<TEvent, TriggerTransition>();
            }

            public void AddTransition(TStateId to, IPredicate condition, bool immediateUpdate)
            {
                Transitions.Add(new Transition(to, condition, immediateUpdate));
            }

            public void AddTrigger(TStateId to, TEvent trigger, bool immediateUpdate)
            {
                Triggers.Add(trigger, new TriggerTransition(to, immediateUpdate));
            }
        }
    }
}
