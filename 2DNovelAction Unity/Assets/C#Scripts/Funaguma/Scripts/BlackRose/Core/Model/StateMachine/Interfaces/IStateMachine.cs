using System;
using System.Collections.Generic;

namespace BlackRose.Core.Models.States
{
    /// <summary>
    /// ステートマシン本体のインターフェース（モード依存遷移対応版）
    /// 既存の (state, trigger) 遷移はそのまま継続利用可能。
    /// 必要に応じて (mode, state, trigger) の上書きを追加できる。
    /// </summary>
    public interface IStateMachine
    {
        /// <summary>登録済みステート一覧（キー=ステート名）</summary>
        Dictionary<string, StateInfo> StateMap { get; }

        /// <summary>現在のステート（キーとインスタンス）</summary>
        StateInfo CurrentState { get; }

        // オプション

        /// <summary>
        /// もし現在のレイヤーで指定された遷移先が見つからなかった場合、
        /// デフォルトレイヤーから探すかどうか (デフォルトはtrue)
        /// </summary>
        bool UseDefaultLayerIfMissingTransition { get; set; }

        /// <summary>
        /// デフォルトの遷移表。（fromState, trigger）→ toState  
        /// ※モード依存の定義が無い場合のフォールバックとして利用される
        /// </summary>
        Dictionary<(string state, string trigger), (string state, string animetrigger)> TransitionGroup { get; }

        /// <summary>
        /// モード依存の遷移表。（layer, fromState, trigger）→ toState  
        /// ※定義がある場合は必ずこちらが優先される
        /// </summary>
        Dictionary<(string layer, string state, string trigger), (string state, string animetrigger)> LayerTransitionGroup { get; }

        /// <summary>
        /// どの状態からでも遷移できるステートのグループ
        /// </summary>
        Dictionary<(string layer, string trigger), (string toState, string animeTrigger)> AnyTransitionGroup { get; }
        /// <summary>現在モード（必要なら実装側で enum ラップ可）</summary>
        string CurrentLayer { get; }

        // ======================
        // ランタイム操作
        // ======================

        /// <summary>即時ステート変更を試みる（成功時 true）</summary>
        bool ChangeState(string trigger);

        /// <summary>次フレームまで変更を遅延（最後に入れたものから順に評価）</summary>
        void LazyChange(string trigger);

        /// <summary>trigger を ToString() して ChangeState に渡す糖衣</summary>
        bool ChangeState<T>(T trigger) where T : Enum;

        /// <summary>request を ToString() して LazyChange に渡す糖衣</summary>
        void LazyChange<T>(T request) where T : Enum;

        /// <summary>毎フレーム呼び出し。Stay/遅延要求/直接遷移を処理</summary>
        void UpdateMachine(float deltaTime);

        // ======================
        // ステート登録
        // ======================

        /// <summary>ステートの追加</summary>
        void AddState(string key, StateComp state, params string[] tags);

        /// <summary>ステートの追加（Enumキー対応）</summary>
        void AddState<T>(T key, StateComp state, params string[] tags) where T : Enum;

        // ======================
        // 遷移定義（追加）
        // ======================

        /// <summary>
        /// デフォルト遷移の追加。（fromState, trigger）→ toState  
        /// モード定義が無ければこのルールが使われる
        /// </summary>
        void AddTransition(string fromState, string trigger, string toState, string animationTrigger = "");

        /// <summary>
        /// Layer依存遷移の追加。（mode, fromState, trigger）→ toState  
        /// 同一組み合わせがある場合はこちらが常に優先される
        /// </summary>
        void AddTransitionForLayer(string layer, string fromState, string trigger, string toState, string animationTrigger = "");

        /// <summary>Enum対応の糖衣：デフォルト遷移</summary>
        void AddTransition<TState, TTrig>(TState fromState, TTrig trigger, string toState, string animationTrigger = "")
            where TState : Enum where TTrig : Enum;

        /// <summary>Enum対応の糖衣：Layer依存遷移</summary>
        void AddTransitionForLayer<TLayer, TState, TTrig>(TLayer mode, TState fromState, TTrig trigger, string toState, string animationTrigger = "")
            where TLayer : Enum where TState : Enum where TTrig : Enum;

        void AddAnyTransition(string trigger, string toState, string mode = LayerChar.COMMON, string animationTrigger = "");

        void AddAnyTransition<TMode, TState, TTrig>(TTrig trig, TState state, TMode mode = default, string animationTrigger = "")
            where TMode : Enum
            where TState : Enum
            where TTrig : Enum;

        /// <summary>現在Layerを設定</summary>
        void SetLayer(string layer);

        // ======================
        // 直接遷移（既存）
        // ======================

        /// <summary>
        /// 遷移表やガードを無視して直接ステート遷移。乱用厳禁！
        /// </summary>
        bool SetStateDirect(string target);

        /// <summary>直接遷移を次フレームに予約</summary>
        void SetStateDirectLazy(string target);

        // ======================
        // 起動
        // ======================

        /// <summary>
        /// ステートマシンを起動。既定は "idle"（大文字小文字無視で探索）
        /// </summary>
        void Awake(string startStateKey = "idle", bool log = false);
    }
}
