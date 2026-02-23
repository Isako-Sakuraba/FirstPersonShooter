namespace FiniteStateMachine.Core
{
    public abstract partial class StateMachine<EStateId>
    {
        private class MachineExitTransition : ITransition
        {
            private readonly StateMachine<EStateId> _machine;

            public MachineExitTransition(StateMachine<EStateId> machine)
            {
                _machine = machine;
            }

            public bool Evaluate()
                => _machine.Exited;
        }
    }
}
