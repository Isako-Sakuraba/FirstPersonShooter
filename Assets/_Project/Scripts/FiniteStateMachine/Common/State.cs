using FiniteStateMachine.Core;

namespace FiniteStateMachine.Common
{
    public abstract class State : IState
    {
        protected IStateMachine machine;

        public virtual void Initialize(IStateMachine machine)
        {
            this.machine = machine;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Process() { }

        public string GetPath<EStateId>(EStateId idInParent)
        {
            return idInParent.ToString();
        }
    }
}
