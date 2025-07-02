using System;
using System.Collections.Generic;

namespace BlackRose
{
    // ステートマシン本体のインターフェース
    public interface IStateMachine
    {
        Dictionary<string, StateComp> StateMap { get; }  // ステートの一覧（名前と対応するインスタンス）

        Dictionary<string, Dictionary<string, StateComp>> TransmissionGroup { get; }
        (string Key, StateComp IState) CurrentState { get; }                  // 現在のステート

        string DefaultStateKey { get; }

        void SetCondition(Func<string> condition);
        bool ChangeState(string newIStateKey);         // 即時ステート変更
        void ChangeRequest(string requestKey);        // ステート変更の予約 or 条件付き変更
        void UpdateMachine();                                // ステートマシンの更新処理（Stayの呼び出しなど）
        void AddState(string newIStateKey, StateComp IState);  // ステートの追加
    }
}