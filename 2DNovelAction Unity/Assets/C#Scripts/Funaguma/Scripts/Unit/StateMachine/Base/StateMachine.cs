using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlackRose
{
    // ===============================
    // 汎用ステートマシン（IStateMachine）
    // ===============================
    public class StateMachine : IStateMachine
    {
        private IUnit _parent; // ステートマシンを持ってるキャラ（親オブジェクト）
        private Dictionary<string, StateComp> _stateMap = new(); // ステートの登録一覧（名前とステート）
        private Dictionary<string, Dictionary<string, StateComp>> _transmissionGroup;
        private (string Key, StateComp state) _currentState; // 現在のステート
        private string _defaultIStateKey; // 条件なしの時に戻るステート（基本ステート）
        private string request = string.Empty; // ステート遷移の予約

        public IUnit Parent => _parent;
        public Dictionary<string, StateComp> StateMap => _stateMap;
        public (string Key, StateComp IState) CurrentState => _currentState;
        public string DefaultStateKey => _defaultIStateKey;

        public Dictionary<string, Dictionary<string, StateComp>> TransmissionGroup => _transmissionGroup;

        // 外部で設定する「ステート条件判定用の関数」
        private Func<string> _condition;

        // ===============================
        // コンストラクタ（初期化処理）
        // ===============================
        // parent: このステートマシンを使うキャラ本体
        // defaultIState: 最初に入っておくステート
        // defaultIStateKey: 登録名（デフォルトは "idle"）
        public StateMachine(IUnit parent, StateComp defaultState, Func<string> condition, string defaultStateKey = "idle")
        {
            _parent = parent;
            _defaultIStateKey = defaultStateKey;
            SetCondition(condition);

            AddState(defaultStateKey, defaultState); // ステートを登録
            SetIStateDirect(defaultStateKey);         // 最初のステートに入る
        }
        // ===============================
        // ステートを決定する関数（条件式から）
        // ===============================
        private string StateDecision()
        {
            // 条件が設定されていれば実行、なければデフォルトに戻る
            return _condition?.Invoke() ?? _defaultIStateKey;
        }

        public void SetCondition(Func<string> condition)
        {
            _condition = condition;
        }

        // ===============================
        // ステート変更を予約（次フレームで反映）
        // ===============================
        public void ChangeRequest(string toIState)
        {
            if (toIState != null)
                request = toIState;
        }

        // ===============================
        // 予約されたステートを読み取る（一度きり）
        // ===============================
        private string ReadRequest()
        {
            var tmp = request;
            request = string.Empty; // 読み取り後は削除
            return tmp;
        }

        // ===============================
        // ステートを変更
        // ===============================
        public bool ChangeState(string targetIState)
        {
            if (string.IsNullOrEmpty(targetIState)) return false;
            if (_currentState.Key == targetIState) return false;
            var tmp = _currentState.state;
            if (_stateMap.TryGetValue(targetIState, out var state))
            {
                if (!tmp.AllowChange(state, _parent)) return false;
                _currentState.state.Exit(state, _parent);
                _currentState = (targetIState, state);
                _currentState.state.Enter(tmp, _parent);
                return true;
            }
            else
            {
                Debug.LogError("IState not found: " + targetIState);
                return false;
            }
        }

        // ===============================
        // 毎フレーム呼び出して状態更新（Update内で呼ぶ）
        // ===============================
        public void UpdateMachine()
        {
            ReadRequest();
            // 優先順位：予約があればそれ → なければ条件から決める
            if (string.IsNullOrEmpty(request))
                request = StateDecision();

            if (!ChangeState(request))
                _currentState.state.Stay(_parent);
        }

        // ===============================
        // ステートの登録（事前にAddして使う）
        // ===============================
        public void AddState(string newIStateKey, StateComp state)
        {
            _stateMap[newIStateKey] = state;
        }

        // ===============================
        // ステートを即時設定（Enterも自動で実行される）
        // ===============================
        public void SetIStateDirect(string targetIState)
        {
            var tmp = _currentState.state;
            if (_stateMap.TryGetValue(targetIState, out var state))
            {
                _currentState = (targetIState, state);
                _currentState.state.Enter(tmp, _parent);
            }
        }

        public override string ToString()
        {
            return $"CurrentState: {_currentState.Key ?? "None"}";
        }
    }
}
//unicode