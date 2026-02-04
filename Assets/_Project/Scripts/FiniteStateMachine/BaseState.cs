using System;
using System.Collections;
using UnityEngine;


namespace FiniteStateMachine
{
    public abstract class BaseState<TStateId, TContext, TEvent>
    {
        public TStateId StateId { get; private set; }
        protected TContext context;

        protected StateMachine<TStateId, TContext, TEvent> stateMachine;

        public virtual void Initalize(TStateId key, TContext context, StateMachine<TStateId, TContext, TEvent> stateMachine)
        {
            this.context = context;
            this.StateId = key;
            this.stateMachine = stateMachine;
        }

        public virtual void EnterState() { }
        public virtual void ExitState() { }
        public virtual void UpdateState() { }
        public virtual void FixedUpdateState() { }

        public virtual string GetActiveHierarchyPath()
        {
            return StateId.ToString();
        }
    }

}
