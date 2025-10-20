using BlackRose.Core.Models.States.Animators;
using BlackRose.Core.Models.Units;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlackRose.Core.Models.States
{
    public class StateMachine : IStateMachine
    {
        private readonly UnitBase _parent;
        private readonly IAnimationDriver _anim;

        private readonly Dictionary<string, StateInfo> _stateMap = new();
        private readonly Dictionary<(string state, string trigger), (string state, string animetrigger)> _transitionGroup = new();

        private readonly Dictionary<(string layer, string state, string trigger), (string state, string animetrigger)> _layerTransitionGroup = new();

        private readonly Dictionary<(string layer, string trigger), (string toState, string animeTrigger)> _anyTransitionGroup = new();

        private StateInfo _currentState;
        private readonly Queue<string> _requests = new();
        private readonly Queue<string> _directRequests = new();

        public string CurrentLayer { get; private set; } = "Default";
        public Dictionary<string, StateInfo> StateMap => _stateMap;
        public StateInfo CurrentState => _currentState;
        public Dictionary<(string state, string trigger), (string state, string animetrigger)> TransitionGroup => _transitionGroup;
        public Dictionary<(string layer, string state, string trigger), (string state, string animetrigger)> LayerTransitionGroup => _layerTransitionGroup;
        public Dictionary<(string layer, string trigger), (string toState, string animeTrigger)> AnyTransitionGroup => _anyTransitionGroup;
        public bool UseDefaultLayerIfMissingTransition { get; set; } = true;

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
            _transitionGroup[(fromState, trigger)] = (toState, animationTrigger);
        }

        public void AddTransitionForLayer(string mode, string fromState, string trigger, string toState, string animationTrigger = "")
        {
            _layerTransitionGroup[(mode, fromState, trigger)] = (toState, animationTrigger);
        }

        public void AddAnyTransition(string trigger, string toState, string mode = LayerChar.COMMON, string animationTrigger = "")
        {
            _anyTransitionGroup[(mode, trigger)] = (toState, animationTrigger);
        }

        public void AddAnyTransition<TMode, TState, TTrig>(TTrig trig, TState state, TMode mode = default, string animationTrigger = "")
            where TMode : Enum
            where TState : Enum
            where TTrig : Enum
        {
            if (mode.Equals(default))
                AddAnyTransition(trig.ToString(), state.ToString(), animationTrigger: animationTrigger);
            else
                AddAnyTransition(trig.ToString(), state.ToString(), mode.ToString(), animationTrigger);
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
            // CurrentLayer
            if (_layerTransitionGroup.TryGetValue((CurrentLayer, _currentState.key, trigger), out var trs))
            {
                // 遷移の成功失敗にかかわらず続けない
                return Change(trs);
            }

            if (_anyTransitionGroup.TryGetValue((CurrentLayer, trigger), out trs))

            { // 遷移の成功失敗にかかわらず続けない
                return Change(trs);
            }
            // Common
            if (_layerTransitionGroup.TryGetValue((LayerChar.COMMON, _currentState.key, trigger), out trs))
            {
                if (Change(trs)) return true;
            }
            if (_anyTransitionGroup.TryGetValue((LayerChar.COMMON, trigger), out trs))
            {
                if (Change(trs)) return true;
            }
            if (!UseDefaultLayerIfMissingTransition) return false;

            // Default
            if (_transitionGroup.TryGetValue((_currentState.key, trigger), out trs))
            {
                return Change(trs);
            }
            return false;
        }

        private bool Change((string state, string animetrigger) trs)
        {
            var to = _stateMap[trs.state];
            if (_currentState.Instance.AllowChange(to.Instance, _parent) && to.Instance.AllowEnter(_currentState.state, _parent))
            {
                var from = _currentState.state;
                var fromKey = _currentState.key;

                from.Exit(to.Instance, _parent);
                _currentState = to;

                _anim.OnTransition(fromKey, _currentState.key, trs.animetrigger);

                to.Instance.Enter(from, _parent);
                return true;
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

        public void Awake(string startStateKey = "idle", bool log = false)
        {
            if (_stateMap.ContainsKey(startStateKey))
            {
                SetStateDirect(startStateKey);
            }
            else
            {
                startStateKey = char.ToUpper(startStateKey[0]) + startStateKey.Substring(1);
                if (_stateMap.ContainsKey(startStateKey))
                    SetStateDirect(startStateKey);
                else
                    throw new System.ArgumentException($"Unknown start state: {startStateKey}");
            }
            if (!log) return;
            // ログ
            Debug.Log($"[StateMachine] Start State: {_currentState.key}");
            foreach (var t in _transitionGroup)
            {
                Debug.Log($"[StateMachine] Transition added: {t.Key.state} --({t.Key.trigger})-> {t.Value.state}");
            }
            foreach (var t in _layerTransitionGroup)
            {
                Debug.Log($"[StateMachine] Layer Transition added: [Layer:{t.Key.layer}] {t.Key.state} --({t.Key.trigger})-> {t.Value.state}");
            }
            foreach (var t in _anyTransitionGroup)
            {
                Debug.Log($"[StateMachine] Any Transition added: [Layer:{t.Key.layer}] --({t.Key.trigger})-> {t.Value.toState}");
            }
        }

        public void AddState(string key, StateComp state, params string[] tags)
        {
            _stateMap[key] = new StateInfo(key, state, tags);
            if (state is IRigidbodyUser user)
            {
                if (_parent.TryGetComponent<Rigidbody2D>(out var rb)) user.SetRB2(rb);
                else Debug.LogWarning("RigitBody が未設定");
            }
        }

        public void AddState<T>(T key, StateComp state, params string[] tags) where T : System.Enum
            => AddState(key.ToString(), state, tags);

        public bool SetStateDirect(string target)
        {
            var tmp = _currentState.state;
            if (_stateMap.TryGetValue(target, out var state))
            {
                var prevKey = _currentState.key;
                _currentState = state;
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
