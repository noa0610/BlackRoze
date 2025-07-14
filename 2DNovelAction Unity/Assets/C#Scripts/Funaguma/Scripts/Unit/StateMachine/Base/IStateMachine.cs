using System.Collections.Generic;

namespace BlackRose
{
    // ステートマシン本体のインターフェース
    public interface IStateMachine
    {
        Dictionary<string, StateComp> StateMap { get; }  // ステートの一覧（名前と対応するインスタンス）
        // state, trigger, state
        Dictionary<(string state, string trigger), string> TransmissionGroup { get; }
        (string key, StateComp state) CurrentState { get; }                  // 現在のステート

        bool ChangeState(string trigger);   // 即時ステート変更
        void LazyChange(string request);    // 次のフレームまで遅延

        /// <summary>
        /// 内部でTostring()を行い、stringを引数にとるCurrentStateに引き渡す
        /// </summary>
        bool ChangeState(object trigger);
        /// <summary>
        /// 内部でTostring()を行い、stringを引数にとるLazyChangeに引き渡す
        /// </summary>
        void LazyChange(object request);    
        void UpdateMachine();               // ステートマシンの更新処理（Stayの呼び出し、LazyChangeの反映）
        void AddState(string key, StateComp IState);  // ステートの追加
        void AddState(object key, StateComp IState);  // ステートの追加

        /// <summary>
        /// TransmissionGroupや遷移条件を無視してステートを遷移させる。
        /// 乱用厳禁。
        /// </summary>
        void SetStateDirect(string target);

        /// <summary>
        /// ステートマシンを起動する
        /// </summary>
        void Awake(string startStateKey = "idle");
    }
}