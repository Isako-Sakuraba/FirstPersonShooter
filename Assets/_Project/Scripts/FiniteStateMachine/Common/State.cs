using FiniteStateMachine.Core;

namespace FiniteStateMachine.Common
{
    public abstract class State : IState
    {
        public virtual void Initialize<EStateId>(EStateId id) { }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Process() { }

        public string GetPath<EStateId>(EStateId idInParent)
        {
            return idInParent.ToString();
        }

    }
}
