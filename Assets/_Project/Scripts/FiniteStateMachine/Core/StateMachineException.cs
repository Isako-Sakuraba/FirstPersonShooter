using System;

namespace FiniteStateMachine.Core
{
    public class StateMachineException : Exception
    {
        public StateMachineException()
        {
        }

        public StateMachineException(string message) : base(message)
        {
        }
    }
}
