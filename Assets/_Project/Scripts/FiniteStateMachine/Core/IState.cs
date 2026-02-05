namespace FiniteStateMachine.Core
{
    public interface IState
    {
        public void Initialize(IStateMachine machine);
        
        public void Enter();
        public void Exit();
        public void Process();

        public string GetPath<EStateId>(EStateId idInParent);
    }
}
