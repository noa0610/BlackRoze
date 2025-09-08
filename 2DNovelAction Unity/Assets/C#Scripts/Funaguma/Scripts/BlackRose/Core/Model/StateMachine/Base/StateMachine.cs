using BlackRose.Core.Models.States.Animators;
using BlackRose.Core.Models.Units;
using System.Collections.Generic;

namespace BlackRose.Core.Models.States
{
    public class StateMachine : IStateMachine
    {
        private readonly UnitBase _parent;
        private readonly IAnimationDriver _anim;

        private readonly Dictionary<string, StateComp> _stateMap = new();
        private readonly Dictionary<(string state, string trigger), (string state, string animetrigger)> _transmissionGroup = new();

        private readonly Dictionary<(string layer, string state, string trigger), (string state, string animetrigger)> _layerTransmissionGroup = new();


        private (string key, StateComp state) _currentState;
        private readonly Queue<string> _requests = new();
        private readonly Queue<string> _directRequests = new();

        public string CurrentLayer { get; private set; } = "Default";
        public Dictionary<string, StateComp> StateMap => _stateMap;
        public (string key, StateComp state) CurrentState => _currentState;
        public Dictionary<(string state, string trigger), (string state, string animetrigger)> TransmissionGroup => _transmissionGroup;
        public Dictionary<(string layer, string state, string trigger), (string state, string animetrigger)> LayerTransmissionGroup => _layerTransmissionGroup;
        public bool UseDefaultLayerIfMissingTransmission { get; set; } = true;

        public StateMachine(UnitBase parent) : this(parent, new NullAnimationDriver()) { }

        public StateMachine(UnitBase parent, IAnimationDriver animationDriver)
        {
            _parent = parent;
            _anim = animationDriver ?? new NullAnimationDriver();
            _anim.CurrentLayer = CurrentLayer;
        }

        // ===============================
        // モード操作
        // ===============================
        public void SetLayer(string layer)
        {
            if (!string.IsNullOrEmpty(layer))
            {
                CurrentLayer = layer;
                _anim.CurrentLayer = layer;
            }
        }

        // ===============================
        // 遷移追加API
        // ===============================
        public void AddTransition(string fromState, string trigger, string toState, string animationTrigger = "")
        {
            _transmissionGroup[(fromState, trigger)] = (toState, animationTrigger);
        }

        public void AddTransitionForLayer(string mode, string fromState, string trigger, string toState, string animationTrigger = "")
        {
            _layerTransmissionGroup[(mode, fromState, trigger)] = (toState, animationTrigger);
        }

        public void AddTransition<TState, TTrig>(TState fromState, TTrig trigger, string toState, string animationTrigger = "")
            where TState : System.Enum where TTrig : System.Enum
            => AddTransition(fromState.ToString(), trigger.ToString(), toState, animationTrigger); // ★fix: animationTriggerを渡す

        public void AddTransitionForLayer<TMode, TState, TTrig>(TMode mode, TState fromState, TTrig trigger, string toState, string animationTrigger = "")
            where TMode : System.Enum where TState : System.Enum where TTrig : System.Enum
            => AddTransitionForLayer(mode.ToString(), fromState.ToString(), trigger.ToString(), toState, animationTrigger); // ★fix

        // ===============================
        // ステート変更
        // ===============================
        public bool ChangeState(string trigger)
        {
            if (_layerTransmissionGroup.TryGetValue((CurrentLayer, _currentState.key, trigger), out var transByLayer))
            {
                var to = _stateMap[transByLayer.state];
                if (_currentState.state.AllowChange(to, _parent) && to.AllowEnter(_currentState.state, _parent))
                {
                    var from = _currentState.state;
                    var fromKey = _currentState.key;

                    from.Exit(to, _parent);
                    _currentState = (transByLayer.state, to);

                    _anim.OnTransition(fromKey, _currentState.key, transByLayer.animetrigger);

                    to.Enter(from, _parent);
                    return true;
                }
                return false; // レイヤー指定があるのに不可なら即終了
            }

            if (!UseDefaultLayerIfMissingTransmission) return false;

            if (_transmissionGroup.TryGetValue((_currentState.key, trigger), out var trans))
            {
                var to = _stateMap[trans.state];
                if (_currentState.state.AllowChange(to, _parent) && to.AllowEnter(_currentState.state, _parent))
                {
                    var from = _currentState.state;
                    var fromKey = _currentState.key;

                    from.Exit(to, _parent);
                    _currentState = (trans.state, to);

                    _anim.OnTransition(fromKey, _currentState.key, trans.animetrigger);

                    to.Enter(from, _parent);
                    return true;
                }
            }
            return false;
        }

        public bool ChangeState<T>(T trigger) where T : System.Enum
            => ChangeState(trigger.ToString());

        public void LazyChange(string trigger)
        {
            if (!string.IsNullOrEmpty(trigger))
                _requests.Enqueue(trigger);
        }

        public void LazyChange<T>(T request) where T : System.Enum
            => LazyChange(request.ToString());

        public void UpdateMachine(float deltaTime)
        {
            while (_directRequests.Count > 0)
            {
                var direct = _directRequests.Dequeue();
                if (SetStateDirect(direct)) return;
            }
            while (_requests.Count > 0)
            {
                var trig = _requests.Dequeue();
                if (ChangeState(trig)) return;
            }
            _currentState.state.Stay(_parent, deltaTime);
        }

        public void Awake(string startStateKey = "idle")
        {
            if (_stateMap.ContainsKey(startStateKey))
                SetStateDirect(startStateKey);
            else
            {
                startStateKey = char.ToUpper(startStateKey[0]) + startStateKey.Substring(1);
                if (_stateMap.ContainsKey(startStateKey))
                    SetStateDirect(startStateKey);
                else
                    throw new System.ArgumentException($"Unknown start state: {startStateKey}");
            }
        }

        public void AddState(string key, StateComp state) => _stateMap[key] = state;

        public void AddState<T>(T key, StateComp state) where T : System.Enum
            => AddState(key.ToString(), state);

        public bool SetStateDirect(string target)
        {
            var tmp = _currentState.state;
            if (_stateMap.TryGetValue(target, out var state))
            {
                var prevKey = _currentState.key;
                _currentState = (target, state);
                _anim.OnSetState(target); // ★アニメーション委譲（直接セット時）
                _currentState.state.Enter(tmp, _parent);
                return true;
            }
            return false;
        }

        public void SetStateDirectLazy(string target)
        {
            if (!string.IsNullOrEmpty(target))
                _directRequests.Enqueue(target);
        }

        public override string ToString() => $"CurrentState: {_currentState.key ?? "None"} (Layer: {CurrentLayer})";
    }
}
