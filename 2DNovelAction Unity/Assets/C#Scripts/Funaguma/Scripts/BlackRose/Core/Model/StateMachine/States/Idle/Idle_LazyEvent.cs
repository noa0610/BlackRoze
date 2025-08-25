using BlackRose.Core.Models.Units;
using UnityEngine;
using UnityEngine.Events;

namespace BlackRose.Core.Models.States
{
    /// <summary>
    /// 途中でほかのステートに移動した場合、カウンターはリセットされる
    /// </summary>
    public class Idle_LazyEvent : Idle
    {
        protected readonly float _lazyChangeTime;
        protected readonly bool _isBlock;
        protected UnityEvent _lazyEvent = new();
        protected float _time;
        protected bool _blocked;

        public UnityEvent LazyEvent
        {
            get => _lazyEvent;
            set => _lazyEvent = value;
        }
        /// <param name="lazyChange">一定時間後に遷移するステート</param>
        /// <param name="lazyChangeTime">遷移の遅延</param>
        /// <param name="isBlock">遅延時間が終わるまで遷移を阻むかどうか</param>
        public Idle_LazyEvent(float lazyChangeTime, bool isBlock = false)
        {
            _lazyChangeTime = lazyChangeTime;
            _isBlock = isBlock;
        }

        public override void Enter(IState previousIState, UnitBase parent)
        {
            _blocked = _isBlock;
            _time = _lazyChangeTime;
        }

        public override void Stay(UnitBase parent, float deltaTime)
        {
            _time -= Time.deltaTime;
            if (_time <= 0)
            {
                Debug.Log("Invoked Lazy Event");
                _blocked = false;
                _lazyEvent?.Invoke();
            }
        }

        public override bool AllowChange(IState nextState, UnitBase parent)
        {
            if (_blocked) return false;
            return base.AllowChange(nextState, parent);
        }
    }
}