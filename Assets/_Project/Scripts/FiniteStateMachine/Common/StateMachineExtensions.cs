using FiniteStateMachine.Core;

namespace FiniteStateMachine.Common
{
    public static class StateMachineExtensions
    {
        public static ReverseTransition AddTwoWayTransition<EStateId>
            (this StateMachine<EStateId> machine,
            EStateId from, EStateId to, ITransition transition, bool immediate = false)
        {
            machine.AddTransition(from, to, transition, immediate);
            ReverseTransition reverse = new ReverseTransition(transition);
            machine.AddTransition(to, from, reverse, immediate);

            return reverse;
        }
    }
}
