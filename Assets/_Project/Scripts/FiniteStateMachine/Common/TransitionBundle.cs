using FiniteStateMachine.Core;
using System;

namespace FiniteStateMachine.Common
{
    public class TransitionBundle : ITransition
    {
        public enum Operation
        {
            Any,
            All
        }

        private readonly Operation _operation;
        private readonly ITransition[] _bundle;

        public TransitionBundle(Operation operation, params ITransition[] transitions)
        {
            _operation = operation;
            _bundle = transitions != null ? new ITransition[transitions.Length] : Array.Empty<ITransition>();

            if (transitions != null)
                Array.Copy(transitions, _bundle, transitions.Length);
        }

        public bool Evaluate()
        {
            if (_operation == Operation.Any)
                foreach (var transition in _bundle)
                    if (transition.Evaluate()) return true;

            bool intermediate = false;
            if (_operation == Operation.All)
                foreach (var transition in _bundle)
                    intermediate &= transition.Evaluate();

            return intermediate;
        }
    }
}
