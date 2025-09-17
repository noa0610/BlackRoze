using BlackRose.Core.Models.Units;
using System;
using UnityEngine;

namespace BlackRose.Core.Models.States
{
    /// <summary>
    /// 指定時間後に自動で遷移する。
    /// 途中でほかのステートに移動した場合、カウンターはリセットされる
    /// <example>
    /// 基本的な例
    /// <code>    
    /// private void RegisterStates()
    /// {
    ///     var lazy = new Idle_LazyChange("起動したいトリガー", 待機秒数(float), 遷移を阻むかどうか(bool));
    ///     _stateMachine.AddState(States.waitForAttack, lazy);
    /// }
    /// </code>
    /// StateWithAnimeを継承しているため、アニメーションの遷移も可能
    /// <code>
    /// var lazy = new Idle_LazyChange(~~)
    ///                     .SetAnimeTrigger(アニメーターのトリガー名)
    ///                     .SetNeedWait(アニメーションの完了を待つかどうか(bool))
    ///                     .SetCancelableProgress(遷移を許可するアニメーションの進行度→割合(float));
    /// </code>
    /// </example>
    /// </summary>
    public class Idle_LazyChange : Idle_LazyEvent
    {
        private string _lazyChange;
        /// <param name="lazyChange">一定時間後に遷移するステート</param>
        /// <param name="lazyChangeTime">遷移の遅延</param>
        /// <param name="isBlock">遅延時間が終わるまで遷移を阻むかどうか</param>
        public Idle_LazyChange(string lazyChange, float lazyChangeTime, bool isBlock = false)
            : base(lazyChangeTime, isBlock)
        {
            _lazyChange = lazyChange;
        }

        public override void Enter(IState previousIState, UnitBase parent)
        {
            base.Enter(previousIState, parent);
            Action evt = null;
            evt = () =>
            {
                parent.StateMachine.LazyChange(_lazyChange);
                OnCompleted -= evt;
            };
            OnCompleted += evt;
        }
    }
}