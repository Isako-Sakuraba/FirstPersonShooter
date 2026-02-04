using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace FiniteStateMachine.Core
{
    public enum DefaultState
    {
        Any,
        Enter,
        Exit
    }

    public partial class StateMachine<EStateId> : IState
    {

        private const int MAX_TRANSITION_DEPTH = 10;

        private bool _isRoot = true;
        public bool IsRoot => _isRoot;

        private bool _isInitialized;
        public bool IsInitialized;

        private bool _exited = false;
        public bool Exited;

        private StateNode _currentNode;
        private Dictionary<EStateId, StateNode> _states = new ();

        private EStateId _intialState;
        public EStateId InitialState;

        private HashSet<TransitionNode> _anyTransitions=new();
        private HashSet<TransitionNode> _enterTransitions=new();
        private Dictionary<EStateId, ITransition> _exitTransitions = new();

        private ITransition _exitTransition;
        public ITransition ExitTransition => _exitTransition;

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

        public StateMachine()
        {
            _exitTransition = new MachineExitTransition(this);
        }

        public void Initialize<ParentStateId>(ParentStateId id)
        {
            _isRoot = false;
            _isInitialized = true;
        }

        public virtual void Enter()
        {
            if (!_isInitialized)
                return;

            _exited = false;

            HandleEnterTransition();
        }

        public virtual void Exit()
        {
            if (!_isInitialized)
                return;

            _exited = true;
        }

        public void AddState(EStateId id, IState state)
        {
            StateNode node = new StateNode(id, state);
            state.Initialize(id);
            _states.Add(id, node);
        }

        public void AddTransition(EStateId from, EStateId to, ITransition transition, bool immediate = false)
        {
            if (!_states.TryGetValue(from, out StateNode fromNode))
                throw new StateMachineException($"State {from.ToString()} does not exist!");
            if (!_states.ContainsKey(to))
                throw new StateMachineException($"State {to.ToString()} does not exist!");

            fromNode.AddTransition(to, transition, immediate);
        }

        public void AddTransition(DefaultState from, EStateId to, ITransition transition, bool immediate = false)
        {
            if (from == DefaultState.Exit)
                throw new StateMachineException($"Cannot create transitions from {from}");
            if (!_states.ContainsKey(to))
                throw new StateMachineException($"State {to.ToString()} does not exist!");

            var node = new TransitionNode(to, transition, immediate);

            if (from == DefaultState.Enter)
                _enterTransitions.Add(node);
            else if (from == DefaultState.Any)
                _anyTransitions.Add(node);
        }

        public void AddTransition(EStateId from, DefaultState to, ITransition transition, bool immediate = false)
        {
            if (!_states.TryGetValue(from, out StateNode fromNode))
                throw new StateMachineException($"State {from.ToString()} does not exist!");
            if (to == DefaultState.Enter || to == DefaultState.Any)
                throw new StateMachineException($"Cannot create transitions to {to}");

            _exitTransitions.Add(from, transition);
        }

        public void Run(EStateId initialState, bool process = false)
        {
            if (!_states.ContainsKey(initialState))
                throw new StateMachineException($"State {initialState} does not exist!");
            
            _intialState = initialState;

            _isInitialized = true;

            Enter();

            if (process)
                ProcessMachine();
        }

        public virtual void Process()
        {
            if (!_isInitialized || _exited)
                return;

            ProcessMachine();
        }

        private void ProcessMachine()
        {
            for (int i = 0; i < MAX_TRANSITION_DEPTH; i++)
            {
                if (TryExitTransition())
                {
                    HandleExitTransition();
                    return;
                }

                if (!TryTransition(out TransitionNode transition))
                    break;

                ChangeState(transition.To);

                if (!transition.Immediate)
                    break;
            }

            _currentNode.State.Process();
        }

        private void ChangeState(EStateId nextId)
        {
            if (_currentNode.Id.Equals(nextId))
                return;

            _currentNode.State.Exit();
            _currentNode = _states[nextId];
            _currentNode.State.Enter();
        }

        private bool TryTransition(out TransitionNode transition)
        {
            foreach (TransitionNode anyTransition in _anyTransitions)
                if (anyTransition.Evaluate())
                {
                    transition = anyTransition;
                    return true;
                }

            foreach (TransitionNode availableTransition in _currentNode.Transitions)
                if (availableTransition.Evaluate())
                {
                    transition = availableTransition;
                    return true;
                }

            transition = null;
            return false;
        }

        private bool TryExitTransition()
            => _exitTransitions.TryGetValue(_currentNode.Id, out ITransition exitTransition) && exitTransition.Evaluate();

        private void HandleExitTransition()
        {
            _exited = true;

            _currentNode.State.Exit();
        }

        public void HandleEnterTransition()
        {
            foreach (var transitionNode in _enterTransitions)
                if (transitionNode.Evaluate())
                {
                    _currentNode = _states[transitionNode.To];
                    _currentNode.State.Enter();
                    return;
                }

            _currentNode = _states[_intialState];
            _currentNode.State.Enter();
        }

        public string GetPath<EParentId>(EParentId assignedId)
        {
            var childPath = _currentNode.State.GetPath(_currentNode.Id);
            return $"{assignedId}/{childPath}";
        }

        public string GetFullPath()
        {
            var childPath = _currentNode.State.GetPath(_currentNode.Id);
            var name = GetType().Name;
            return $"{name}/{childPath}";
        }
    }
}
