using System;
using System.Collections.Generic;

namespace BlackRose
{

    /// <summary>
    /// 状態（ステート）ごとのインターフェース。
    /// 各ステート（行動パターン）にこのインターフェースを実装させる。
    /// 返り値のboolは処理の成功／失敗を示す。
    /// </summary>
    public interface IState
    {
        bool Enter(IState previousState, IUnit parent);

        bool Stay(IUnit parent);

        bool Exit(IState nextState, IUnit parent);
    }

    // ステートマシン本体のインターフェース
    public interface IStateMachine
    {
        Dictionary<string, IState> StateMap { get; }  // ステートの一覧（名前と対応するインスタンス）
        Tuple<string, IState> CurrentState { get; }                  // 現在のステート

        string DefaultStateKey { get; }

        void SetCondition(Func<string> condition);
        void ChangeState(string newStateKey);         // 即時ステート変更
        void ChangeRequest(string requestKey);        // ステート変更の予約 or 条件付き変更
        void Update();                                // ステートマシンの更新処理（Stayの呼び出しなど）
        void AddState(string newStateKey, IState state);  // ステートの追加
    }
}
