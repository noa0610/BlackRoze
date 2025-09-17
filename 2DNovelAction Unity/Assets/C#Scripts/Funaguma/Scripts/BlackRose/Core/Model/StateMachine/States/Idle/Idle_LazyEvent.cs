using BlackRose.Core.Models.Units;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace BlackRose.Core.Models.States
{
    /// <summary>
    /// 途中でほかのステートに移動した場合、カウンターはリセットされる
    /// </summary>
    public class Idle_LazyEvent : Idle, ICompleteEmitter
    {
        protected readonly float _lazyChangeTime;
        protected readonly bool _isBlock;

        [Obsolete]
        protected UnityEvent _lazyEvent = new();
        protected bool _blocked;

        public event Action OnCompleted;

        [Obsolete]
        public UnityEvent LazyEvent
        {
            get => _lazyEvent;
            set => _lazyEvent = value;
        }
        /// <param name="lazyChange">一定時間後に遷移するステート</param>
        /// <param name="lazyChangeTime">遷移の遅延</param>
        /// <param name="isBlock">遅延時間が終わるまで遷移を阻むかどうか</param>
        public Idle_LazyEvent(float lazyChangeTime, bool isBlock = false) : base()
        {
            _lazyChangeTime = lazyChangeTime;
            _isBlock = isBlock;
            _timeHolders.Register(nameof(_lazyChangeTime), _lazyChangeTime);
        }

        public override void Enter(IState previousIState, UnitBase parent)
        {
            base.Enter(previousIState, parent);
            _blocked = _isBlock;
            _timeHolders.Start(nameof(_lazyChangeTime));
        }

        public override void Stay(UnitBase parent, float deltaTime)
        {
            base.Stay(parent, deltaTime);
            if (_timeHolders.IsFinished(nameof(_lazyChangeTime)))
            {
                Debug.Log("Invoked Lazy Event");
                _blocked = false;
                _lazyEvent?.Invoke();
                OnCompleted?.Invoke();
            }
        }

        public override bool AllowChange(IState nextState, UnitBase parent)
        {
            if (_blocked) return false;
            return base.AllowChange(nextState, parent);
        }
    }
}