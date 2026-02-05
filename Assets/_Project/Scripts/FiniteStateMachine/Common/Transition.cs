using FiniteStateMachine.Core;
using static UnityEditor.VersionControl.Asset;

namespace FiniteStateMachine.Common
{
    public abstract class Transition : ITransition
    {
        public abstract bool Evaluate();
    }
}
