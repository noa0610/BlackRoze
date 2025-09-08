namespace BlackRose.Core.Models.States.Animators
{
    /// <summary>
    /// アニメーション制御の抽象化。Unity非依存。
    /// </summary>
    public interface IAnimationDriver
    {
        /// <summary>現在の“論理レイヤー名”を渡す。必要なら内部でAnimator Layer等にマップしてね。</summary>
        string CurrentLayer { get; set; }

        /// <summary>ステート直接変更時に呼ばれる（初期化などに利用可）</summary>
        void OnSetState(string stateKey);

        /// <summary>遷移時に呼ばれる。必要ならアニメトリガーやクロスフェードを実行。</summary>
        void OnTransition(string fromState, string toState, string animationTrigger);
    }

    /// <summary>デフォルト：何もしない（後方互換用）。</summary>
    public sealed class NullAnimationDriver : IAnimationDriver
    {
        public string CurrentLayer { get; set; } = "Default";
        public void OnSetState(string stateKey) { /* no-op */ }
        public void OnTransition(string fromState, string toState, string animationTrigger) { /* no-op */ }
    }
}
