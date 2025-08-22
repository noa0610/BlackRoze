using BlackRose.Core.Models.Units;
using UnityEngine;

namespace BlackRose.Core.Models.States
{
    /// <summary>
    /// 指定時間後に自動で遷移する。
    /// 途中でほかのステートに移動した場合、カウンターはリセットされる
    /// </summary>
    public class Idle_LazyChange : Idle
    {
        private readonly string _lazyChange;
        private readonly float _lazyChangeTime;
        private readonly bool _isBlock;
        private float _time;
        private bool _blocked;

        /// <param name="lazyChange">一定時間後に遷移するステート</param>
        /// <param name="lazyChangeTime">遷移の遅延</param>
        /// <param name="isBlock">遅延時間が終わるまで遷移を阻むかどうか</param>
        public Idle_LazyChange(string lazyChange, float lazyChangeTime, bool isBlock = false)
        {
            _lazyChange = lazyChange;
            _lazyChangeTime = lazyChangeTime;
            _isBlock = isBlock;
        }

        public override void Enter(IState previousIState, UnitBase parent)
        {
            _blocked = _isBlock;
            _time = _lazyChangeTime;
        }

        public override void Stay(UnitBase parent)
        {
            _time -= Time.deltaTime;
            if (_time <= 0)
            {
                parent.StateMachine.LazyChange(_lazyChange);
                _blocked = false;
            }
        }

        public override bool AllowChange(IState nextState, UnitBase parent)
        {
            if (_blocked) return false;
            return base.AllowChange(nextState, parent);
        }
    }
}