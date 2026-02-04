using System;
using System.Collections.Generic;
using UnityEngine;

namespace FiniteStateMachine
{
    public partial class StateMachine<TStateId, TContext, TEvent> : BaseState<TStateId, TContext, TEvent>
    {
        public enum DefaultState
        {
            Any,
            Entry,
            Exit
        }

        private int transitionDepth = 0;
        private const int MAX_TRANSITION_DEPTH = 10;

        private bool isRoot = true;
        public bool IsRoot => isRoot;

        private StateNode currentNode;
        private TStateId initialState;
        private Dictionary<TStateId, StateNode> states = new ();
        private HashSet<Transition> anyTransitions = new ();

        public StateMachine(TContext context)
        {
            this.context = context;
        }

        public override void Initalize(TStateId key, TContext context, StateMachine<TStateId, TContext, TEvent> stateMachine)
        {
            base.Initalize(key, context, stateMachine);
            isRoot = false;
        }

        public override void EnterState()
        {
            if (!isRoot)
                SetStartState(initialState);
        }

        public StateMachine<TStateId, TContext, TEvent> AddState(TStateId key, BaseState<TStateId, TContext, TEvent> state)
        {
            StateNode node = new StateNode(state);
            states.Add(key, node);
            state.Initalize(key, context, this);
            return this;
        }

        public StateMachine<TStateId, TContext, TEvent> AddTransition(TStateId from, TStateId to, IPredicate condition, bool immediateUpdate = true)
        {
            if (!states.TryGetValue(from, out StateNode fromNode))
                throw new Exception($"State {from.ToString()} does not exist!");
            if (!states.ContainsKey(to))
                throw new Exception($"State {to.ToString()} does not exist!");

            fromNode.AddTransition(to, condition, immediateUpdate);
            return this;
        }

        public StateMachine<TStateId, TContext, TEvent> AddTransition(DefaultState from, TStateId to, IPredicate condition, bool immediateUpdate = true)
        {
            //TODO: Handle this.
            return this;
        }

        public StateMachine<TStateId, TContext, TEvent> AddTrigger(TStateId from, TStateId to, TEvent trigger, bool immediateUpdate = true)
        {
            if (!states.TryGetValue(from, out StateNode fromNode))
                throw new Exception($"State {from.ToString()} does not exist!");
            if (!states.ContainsKey(to))
                throw new Exception($"State {to.ToString()} does not exist!");

            fromNode.AddTrigger(to, trigger, immediateUpdate);
            return this;
        }

        public StateMachine<TStateId, TContext, TEvent> AddAnyTransition(TStateId to, IPredicate condition, bool immediateUpdate = true)
        {
            if (!states.ContainsKey(to))
                throw new Exception($"State {to.ToString()} does not exist!");

            anyTransitions.Add(new Transition(to, condition, immediateUpdate));
            return this;
        }

        public void Trigger(TEvent trigger)
        {
            if (!currentNode.Triggers.TryGetValue(trigger, out TriggerTransition value))
                return;

            ChangeState(value.To, value.ImmediateUpdate);
        }

        public void SetStartState(TStateId initialStateKey)
        {
            initialState = initialStateKey;
            currentNode = states[initialStateKey];
            currentNode.State.EnterState();
        }

        private void ChangeState(TStateId stateKey, bool immediateUpdate = true)
        {
            if (currentNode.State.StateId.Equals(stateKey))
                return;

            if (transitionDepth > MAX_TRANSITION_DEPTH)
                throw new Exception("Max transition depth reached! Possible infinite loop!");

            transitionDepth++;

            currentNode.State.ExitState();
            currentNode = states[stateKey];
            currentNode.State.EnterState();

            if (immediateUpdate)
                currentNode.State.UpdateState();

            transitionDepth--;
        }

        public override void UpdateState()
        {
            if (TryTransition(out Transition transition))
                ChangeState(transition.To);

            currentNode.State?.UpdateState();
        }

        public override void FixedUpdateState()
        {
            currentNode.State?.FixedUpdateState();
        }

        private bool TryTransition(out Transition transition)
        {
            foreach (Transition availableTransition in anyTransitions)
                if (availableTransition.Condition.Evaluate())
                {
                    transition = availableTransition;
                    return true;
                }

            foreach (Transition availableTransition in currentNode.Transitions)
                if (availableTransition.Condition.Evaluate())
                {
                    transition = availableTransition;
                    return true;
                }

            transition = null;
            return false;
        }

        public void FixedUpdate()
        {
            currentNode.State.FixedUpdateState();
        }

        public override string GetActiveHierarchyPath()
        {
            string name = isRoot ? GetType().Name : StateId.ToString();
            return $"{name}/{currentNode.State.GetActiveHierarchyPath()}";
        }
    }
}
