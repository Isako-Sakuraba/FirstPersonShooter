using FiniteStateMachine.Core;
using System;
using System.Text;

namespace FiniteStateMachine.Common
{
    public class StateBundle : IState
    {
        private readonly IState[] _bundle;
        private StringBuilder pathBuilder = new();

        public StateBundle(params IState[] states)
        {
            _bundle = states != null ? new IState[states.Length] : Array.Empty<IState>();

            if (states != null)
                Array.Copy(states, _bundle, states.Length);
        }

        public void Enter()
        {
            foreach (var state in _bundle)
                state.Enter();
        }

        public void Exit()
        {
            foreach (var state in _bundle)
                state.Enter();
        }

        public string GetPath<EStateId>(EStateId idInParent)
        {
            pathBuilder.Clear();

            pathBuilder.Append(idInParent.ToString());
            pathBuilder.Append('(');
            foreach (var state in _bundle)
                pathBuilder.Append(state.GetType());
            pathBuilder.Append(')');

            return pathBuilder.ToString();
        }

        public void Initialize(IStateMachine machine)
        {
            foreach (var state in _bundle)
                state.Initialize(machine);
        }

        public void Process()
        {
            foreach (var state in _bundle)
                state.Process();
        }
    }
}
