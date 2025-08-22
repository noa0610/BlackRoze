using BlackRose.Core.Models.Units;
using System.Collections.Generic;

namespace BlackRose.Core.Models.States
{
    // ===============================
    // 汎用ステートマシン（IStateMachine）
    // ===============================
    public class StateMachine : IStateMachine
    {
        private UnitBase _parent;
        private Dictionary<string, StateComp> _stateMap = new(); // ステートの登録一覧（名前とステート）
        private Dictionary<(string state, string trigger), string> _transmissionGroup = new();
        private (string key, StateComp state) _currentState; // 現在のステート
        private string _request = string.Empty; // ステート遷移の予約
        private Queue<string> _requests = new();

        public Dictionary<string, StateComp> StateMap => _stateMap;
        public (string key, StateComp state) CurrentState => _currentState;
        public Dictionary<(string state, string trigger), string> TransmissionGroup => _transmissionGroup;

        public StateMachine(UnitBase parent)
        {
            _parent = parent;
        }



        // ===============================
        // ステートを変更
        // ===============================
        public bool ChangeState(string trigger)
        {
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
        public bool ChangeState(object trigger)
        {
            return ChangeState(trigger.ToString());
        }

        /// <summary>
        /// 次のフレームまで更新を遅延（最後のものだけ有効）
        /// </summary>
        public void LazyChange(string trigger)
        {
            if (!string.IsNullOrEmpty(trigger))
                _requests.Enqueue(trigger);
        }
        public void LazyChange(object request)
        {
            LazyChange(request.ToString());
        }

        // ===============================
        // 毎フレーム呼び出して状態更新（Update内で呼ぶ）
        // ===============================
        public void UpdateMachine()
        {
            while (_requests.Count > 0)
            {
                var trig = _requests.Dequeue();
                // 成功したらその時点で抜けて次フレームへ
                if (ChangeState(trig)) return;
            }
            _currentState.state.Stay(_parent);
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
                {
                    throw new System.ArgumentException();
                }
            }

        }

        // ===============================
        // ステートの登録（事前にAddして使う）
        // ===============================
        public void AddState(string key, StateComp state)
        {
            _stateMap[key] = state;
        }
        public void AddState(object key, StateComp state)
        {
            AddState(key.ToString(), state);
        }
        // ===============================
        // Exit、AllowChangeを無視して遷移を行う
        // ===============================
        public void SetStateDirect(string target)
        {
            var tmp = _currentState.state;
            if (_stateMap.TryGetValue(target, out var state))
            {
                _currentState = (target, state);
                _currentState.state.Enter(tmp, _parent);
            }
        }

        public override string ToString()
        {
            return $"CurrentState: {_currentState.key ?? "None"}";
        }
    }
}