using System.Collections.Generic;
using UnityEngine;


namespace FiniteStateMachine.Core
{
    public interface IStateMachine 
    {
        public void RequestResettle();
        public string GetFullPath();
    }

    public abstract partial class StateMachine<EStateId> : IState, IStateMachine
    {
        private const int MAX_TRANSITION_DEPTH = 10;

        protected IStateMachine machine;

        private bool _isRoot = true;
        public bool IsRoot => _isRoot;

        private bool _isInitialized;
        public bool IsInitialized => _isInitialized;

        private bool _exited = false;
        public bool Exited => _exited;

        private bool _resettleRequested = false;
        public bool ResettleRequested => _resettleRequested;

        private StateNode _currentNode;
        private bool _currentNodeExists;
        private Dictionary<EStateId, StateNode> _states = new ();

        private EStateId _defaultState;
        private bool _processDefault;
        public EStateId DefaultState => _defaultState;

        private List<TransitionNode> _anyTransitions = new();
        private List<TransitionNode> _enterTransitions = new();
        private Dictionary<EStateId, List<ITransition>> _exitTransitions = new();
        private List<ITransition> _anyExitTransitions = new();
        private List<ITransition> _enterExitTransitions = new();

        private ITransition _exitTransition;
        public ITransition ExitTransition => _exitTransition;

        public StateMachine()
        {
            _exitTransition = new MachineExitTransition(this);
        }

        public void Initialize(IStateMachine machine)
        {
            _isRoot = false;
            this.machine = machine;
        }

        public virtual void Enter()
        {
            if (!_isInitialized)
                return;

            Debug.Log($"Entered {GetType().Name}");
            DoEnter();
        }

        private void DoEnter()
        {
            _exited = false;

            if (TryEnterExitTransition())
            {
                DoExit();
                return;
            }

            bool enterEvaluated = TryEnterTransition(out var node);
            if (enterEvaluated)
                _currentNode = _states[node.To];
            else
                _currentNode = _states[_defaultState];

            _currentNodeExists = true;
            _currentNode.State.Enter();

            bool immediate = (enterEvaluated && node.Immediate) || _processDefault;
            if (immediate)
                Settle();
        } 

        public virtual void Exit()
        {
            if (!_isInitialized || _exited)
                return;

            DoExit();
        }

        public void AddState(EStateId id, IState state)
        {
            AssertDoesNotContainState(id);

            StateNode node = new StateNode(id, state);
            state.Initialize(this);
            _states.Add(id, node);
        }

        #region Adding Transitions
        public void AddTransition(EStateId from, EStateId to, ITransition transition, bool immediate = false)
        {
            AssertDifferentStates(from, to);
            AssertGetState(from, out StateNode fromNode);
            AssertContainsState(to);

            fromNode.AddTransition(to, transition, immediate);
        }

        public void AddAnyTransition(EStateId to, ITransition transition, bool immediate = false)
        {
            AssertContainsState(to);

            _anyTransitions.Add(new TransitionNode(to, transition, immediate));
        }

        public void AddEnterTransition(EStateId to, ITransition transition, bool immediate = false)
        {
            AssertContainsState(to);

            _enterTransitions.Add(new TransitionNode(to, transition, immediate));
        }

        public void AddExitTransition(EStateId from, ITransition transition)
        {
            AssertContainsState(from);

            if(_exitTransitions.TryGetValue(from, out var list))
                list.Add(transition);
            else
                _exitTransitions.Add(from, new List<ITransition>{transition});
        }

        // bool immediate asks for resettle?
        public void AddAnyExitTransition(ITransition transition)
            => _anyExitTransitions.Add(transition);

        public void AddEnterExitTransition(ITransition transition)
            => _enterExitTransitions.Add(transition);
        #endregion

        #region Assertions
        private void AssertContainsState(EStateId id)
        {
            if (!_states.ContainsKey(id))
                throw new StateMachineException($"State {id.ToString()} does not exist!");
        }

        private void AssertGetState(EStateId id, out StateNode node)
        {
            if (!_states.TryGetValue(id, out node))
                throw new StateMachineException($"State {id.ToString()} does not exist!");
        }

        private void AssertDoesNotContainState(EStateId id)
        {
            if (_states.ContainsKey(id))
                throw new StateMachineException($"State {id.ToString()} is already added!");
        }

        private void AssertDifferentStates(EStateId id1, EStateId id2)
        {
            if (id1.Equals(id2))
                throw new StateMachineException($"Passed states ({id1.ToString()}) are the same!");
        }
        #endregion

        public void Run(EStateId defaultState, bool process = false)
        {
            SetDefault(defaultState);
            if (_isRoot)
                Enter();

            if (process)
                ProcessMachine();
        }

        public void SetDefault(EStateId defaultState, bool process = false)
        {
            AssertContainsState(defaultState);

            _defaultState = defaultState;
            _processDefault = process;
            _isInitialized = true;
        }

        public virtual void Process()
        {
            if (!_isInitialized || _exited)
                return;

            ProcessMachine();
        }

        private void ProcessMachine()
        {
            bool mustProcess = Settle();

            if (mustProcess)
                _currentNode.State.Process();

            if (_resettleRequested)
            {
                Settle();
                _resettleRequested = false;
            }
        }

        private bool Settle()
        {
            bool settled = false;

            for (int currentDepth = 0; currentDepth < MAX_TRANSITION_DEPTH; currentDepth++)
            {

                if (TryAnyExitTransition() || TryCurrentExitTransition())
                {
                    DoExit();
                    return false;
                }

                if (settled) 
                    break;

                TransitionNode transition;
                bool evaluated = TryAnyTransition(out transition) || TryCurrentTransition(out transition);

                if (!evaluated)
                    break;

                ChangeState(transition.To);

                settled = !transition.Immediate;
            }

            return !_exited;
        }

        public void RequestResettle()
        {
            _resettleRequested = true;
        }

        private void ChangeState(EStateId nextId)
        {
            if (_currentNode.Id.Equals(nextId))
                return;

            _currentNode.State.Exit();
            _currentNode = _states[nextId];
            _currentNode.State.Enter();
        }

        private bool TryCurrentTransition(out TransitionNode transition)
            => AnyTransition(_currentNode.Transitions, out transition);

        private bool TryCurrentExitTransition()
            => _exitTransitions.TryGetValue(_currentNode.Id, out var container) && AnyTransition(container);

        private bool TryAnyExitTransition()
            => AnyTransition(_anyExitTransitions);

        private bool TryEnterExitTransition()
            => AnyTransition(_enterExitTransitions);

        private bool TryAnyTransition(out TransitionNode transition)
            => AnyTransition(_anyTransitions, out transition);

        private bool TryEnterTransition(out TransitionNode transition)
            => AnyTransition(_enterTransitions, out transition);

        #region Transition evaluation
        private bool AnyTransition(List<TransitionNode> container,  out TransitionNode evaluated)
        {
            foreach (TransitionNode node in container)
                if (node.Evaluate())
                {
                    if (_currentNodeExists && node.To.Equals(_currentNode.Id))
                        continue;
                    evaluated = node;
                    return true;
                }

            evaluated = null;
            return false;
        }

        private bool AnyTransition(List<ITransition> container)
        {
            foreach (ITransition transition in container)
                if (transition.Evaluate())
                    return true;

            return false;
        }
        #endregion

        private void DoExit()
        {
            _exited = true;

            // Ask parent fsm to check transitions in case they depend on this fsm's exit
            if (!_isRoot)
                machine.RequestResettle();

            _currentNode?.State.Exit();
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

            _currentNode = _states[_defaultState];
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
