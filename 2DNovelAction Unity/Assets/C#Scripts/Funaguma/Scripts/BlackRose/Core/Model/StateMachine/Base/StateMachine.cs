using BlackRose.Core.Models.Units;
using System.Collections.Generic;

namespace BlackRose.Core.Models.States
{
    public class StateMachine : IStateMachine
    {
        private UnitBase _parent;
        private Dictionary<string, StateComp> _stateMap = new();
        private Dictionary<(string state, string trigger), string> _transmissionGroup = new();

        private Dictionary<(string layer, string state, string trigger), string> _layerTransmissionGroup = new();

        public string CurrentLayer { get; private set; } = "Default";

        private (string key, StateComp state) _currentState;
        private Queue<string> _requests = new();
        private Queue<string> _directRequests = new();

        public Dictionary<string, StateComp> StateMap => _stateMap;
        public (string key, StateComp state) CurrentState => _currentState;
        public Dictionary<(string state, string trigger), string> TransmissionGroup => _transmissionGroup;

        public Dictionary<(string layer, string state, string trigger), string> LayerTransmissionGroup => _layerTransmissionGroup;

        public StateMachine(UnitBase parent) { _parent = parent; }

        // ===============================
        // モード操作（★追加API）
        // ===============================
        public void SetLayer(string layer)
        {
            if (!string.IsNullOrEmpty(layer))
                CurrentLayer = layer;
        }

        // ===============================
        // 遷移追加API（★追加）
        // ===============================
        public void AddTransition(string fromState, string trigger, string toState)
        {
            _transmissionGroup[(fromState, trigger)] = toState;
        }
        public void AddTransitionForLayer(string mode, string fromState, string trigger, string toState)
        {
            _layerTransmissionGroup[(mode, fromState, trigger)] = toState;
        }

        public void AddTransition<TState, TTrig>(TState fromState, TTrig trigger, string toState)
            where TState : System.Enum where TTrig : System.Enum
            => AddTransition(fromState.ToString(), trigger.ToString(), toState);

        public void AddTransitionForLayer<TMode, TState, TTrig>(TMode mode, TState fromState, TTrig trigger, string toState)
            where TMode : System.Enum where TState : System.Enum where TTrig : System.Enum
            => AddTransitionForLayer(mode.ToString(), fromState.ToString(), trigger.ToString(), toState);

        // ===============================
        // ステート変更
        // ===============================
        public bool ChangeState(string trigger)
        {
            // ① モード付き優先
            if (_layerTransmissionGroup.TryGetValue((CurrentLayer, _currentState.key, trigger), out string keyByMode))
            {
                var to = _stateMap[keyByMode];
                if (_currentState.state.AllowChange(to, _parent))
                {
                    var from = _currentState.state;
                    from.Exit(to, _parent);
                    _currentState = (keyByMode, to);
                    to.Enter(from, _parent);
                    return true;
                }
                return false; // モード指定があったのに許可されないならここで終了
            }

            // ② 従来のフォールバック
            if (_transmissionGroup.TryGetValue((_currentState.key, trigger), out string key))
            {
                var to = _stateMap[key];
                if (_currentState.state.AllowChange(to, _parent))
                {
                    var from = _currentState.state;
                    from.Exit(to, _parent);
                    _currentState = (key, to);
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
                    throw new System.ArgumentException();
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
                _currentState = (target, state);
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
