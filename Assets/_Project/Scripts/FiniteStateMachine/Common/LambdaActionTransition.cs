using FiniteStateMachine.Core;

namespace FiniteStateMachine.Common
{

    public class LambdaActionTransition : ActionTransition
    {
        public delegate void LambdaAction();

        private readonly LambdaAction _action;

        public LambdaActionTransition(ITransition target, LambdaAction onTransition) : base(target)
        {
            _action = onTransition;
        }

        public override void OnTransition()
            => _action.Invoke();
    }

    public class LambdaActionTransition<TContext> : ActionTransition
    {
        public delegate void LambdaAction(TContext context);

        private readonly LambdaAction _action;
        private readonly TContext _context;

        public LambdaActionTransition(ITransition target, TContext context, LambdaAction onTransition) : base(target)
        {
            _action = onTransition;
            _context = context;
        }

        public override void OnTransition()
            => _action.Invoke(_context);
    }
}
