using FiniteStateMachine.Core;

namespace FiniteStateMachine.Common
{

    public class LambdaTransition : ITransition
    {
        public delegate bool LambdaCondition();

        public readonly LambdaCondition Condition;

        public LambdaTransition(LambdaCondition condition)
        {
            Condition = condition;
        }

        public bool Evaluate()
            => Condition.Invoke();

        public void OnHappened() { }
    }

    // For not creating a closure without reason with simple LambdaTransition
    public class LambdaTransition<TContext> : ITransition
    {
        public delegate bool LambdaCondition(TContext context);

        public readonly LambdaCondition Condition;
        private readonly TContext _context;

        public LambdaTransition(TContext context, LambdaCondition condition)
        {
            _context = context;
            Condition = condition;
        }

        public bool Evaluate()
            => Condition.Invoke(_context);
    }
}
