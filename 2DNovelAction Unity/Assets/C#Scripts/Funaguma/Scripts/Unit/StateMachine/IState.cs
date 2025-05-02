using System;
using System.Collections.Generic;

namespace Test
{

    /// <summary>
    /// 状態（ステート）ごとのインターフェース。
    /// 各ステート（行動パターン）にこのインターフェースを実装させる。
    /// 返り値のboolは処理の成功／失敗を示す。
    /// </summary>
    public interface IState
    {
        // ステートに入ったときの処理（失敗したらfalseを返してエラーハンドリングへ）
        bool Enter(IState previousState, IUnit parent);

        // ステート中に毎フレーム呼ばれる処理（falseならログ出すけど続行）
        bool Stay(IUnit parent);

        // ステートから抜けるときの処理（falseでもログ出して続ける）
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
