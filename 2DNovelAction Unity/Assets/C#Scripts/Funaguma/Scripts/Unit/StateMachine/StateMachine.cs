using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlackRose
{
    // ===============================
    // 汎用ステートマシン（StateMachine）
    // ===============================
    public class StateMachine : IStateMachine
    {
        private IUnit _parent; // ステートマシンを持ってるキャラ（親オブジェクト）
        private Dictionary<string, IState> _stateMap = new(); // ステートの登録一覧（名前とステート）

        private Tuple<string, IState> _currentState; // 現在のステート
        private string _defaultStateKey; // 条件なしの時に戻るステート（基本ステート）
        private string request = string.Empty; // ステート遷移の予約

        public IUnit Parent => _parent;
        public Dictionary<string, IState> StateMap => _stateMap;
        public Tuple<string, IState> CurrentState => _currentState;
        public string DefaultStateKey => _defaultStateKey;

        // 外部で設定する「ステート条件判定用の関数」
        private Func<string> _condition;

        // ===============================
        // コンストラクタ（初期化処理）
        // ===============================
        // parent: このステートマシンを使うキャラ本体
        // defaultState: 最初に入っておくステート
        // defaultStateKey: 登録名（デフォルトは "idle"）
        public StateMachine(IUnit parent, IState defaultState,Func<string> condition, string defaultStateKey = "idle")
        {
            _parent = parent;
            _defaultStateKey = defaultStateKey;
            SetCondition(condition);

            AddState(defaultStateKey, defaultState); // ステートを登録
            SetStateDirect(defaultStateKey);         // 最初のステートに入る
        }

        // ===============================
        // ステートを決定する関数（条件式から）
        // ===============================
        private string StateDecision()
        {
            // 条件が設定されていれば実行、なければデフォルトに戻る
            return _condition?.Invoke() ?? _defaultStateKey;
        }

        public void SetCondition(Func<string> condition)
        {
            _condition = condition;
        }

        // ===============================
        // ステート変更を予約（次フレームで反映）
        // ===============================
        public void ChangeRequest(string toState)
        {
            if (toState != null)
                request = toState;
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
        public void ChangeState(string targetState)
        {
            if (_currentState.Item1 == targetState)
                return;
            if (_stateMap.TryGetValue(targetState, out var state))
            {

                // 今のステートから出る（Exit）
                if (!_currentState.Item2.Exit(state, _parent))
                {
                    Debug.LogError("Error in " + _currentState.ToString() + "'s Exit");
                }

                var tmp = _currentState.Item2;
                _currentState = new Tuple<string, IState>(targetState, state);
                // 新しいステートに入る（Enter）
                _currentState.Item2.Enter(tmp, _parent);
                Debug.Log("Changed State. Current:" + targetState);
            }
            else
            {
                // 登録されてないステート名だった場合
                Debug.LogError("State not found: " + targetState);
            }
        }

        // ===============================
        // 毎フレーム呼び出して状態更新（Update内で呼ぶ）
        // ===============================
        public void Update()
        {
            // 優先順位：予約があればそれ → なければ条件から決める
            if (!string.IsNullOrEmpty(request))
                ChangeState(ReadRequest());
            else
                ChangeState(StateDecision());

            // 現在のステートの処理を実行（Stay）
            _currentState.Item2.Stay(_parent);
        }

        // ===============================
        // ステートの登録（事前にAddして使う）
        // ===============================
        public void AddState(string newStateKey, IState state)
        {
            _stateMap[newStateKey] = state;
        }

        // ===============================
        // ステートを即時設定（Enterも自動で実行される）
        // ===============================
        public void SetStateDirect(string targetState)
        {
            var tmp = _currentState?.Item2;
            if (_stateMap.TryGetValue(targetState, out var state))
            {
                _currentState = new Tuple<string, IState>(targetState, state);
                _currentState.Item2.Enter(tmp, _parent);
            }
        }

        public override string ToString()
        {
            return $"CurrentState: {_currentState?.Item1 ?? "None"}";
        }
    }
}
//unicode