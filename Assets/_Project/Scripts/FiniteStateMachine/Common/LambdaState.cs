namespace FiniteStateMachine.Common
{
    public class LambdaState : State
    {
        public delegate void LambdaAction();

        public LambdaAction OnEnter = delegate { };
        public LambdaAction OnExit = delegate { };
        public LambdaAction OnProcess = delegate { };

        public override void Enter()
            => OnEnter.Invoke();

        public override void Exit()
            => OnExit.Invoke();

        public override void Process()
            => OnProcess.Invoke();
    }

    public class LambdaState<TContext> : State
    {
        public delegate void LambdaAction(TContext context);

        public LambdaAction OnEnter = delegate { };
        public LambdaAction OnExit = delegate { };
        public LambdaAction OnProcess = delegate { };
        private TContext _context;

        public LambdaState(TContext context)
        {
            _context = context;
        }

        public override void Enter()
            => OnEnter.Invoke(_context);

        public override void Exit()
            => OnExit.Invoke(_context);

        public override void Process()
            => OnProcess.Invoke(_context);
    }
}
