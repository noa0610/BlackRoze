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
        protected float _eventTime;
        protected bool _isBlock = false;

        protected bool _blocked;

        public event Action OnCompleted;

        public float RemainTime
        {
            get
            {
                if (Timer.TryGetRemaining(nameof(_eventTime), out var t))
                {
                    return t;
                }
                // 完了扱い
                return 0f;
            }
        }
        /// <param name="lazyChange">一定時間後に遷移するステート</param>
        /// <param name="lazyChangeTime">遷移の遅延</param>
        /// <param name="isBlock">遅延時間が終わるまで遷移を阻むかどうか</param>
        public Idle_LazyEvent(float eventTime, bool isBlock = false) : base()
        {
            _eventTime = eventTime;
            _isBlock = isBlock;
            Timer.CountDownRegister(nameof(_eventTime), _eventTime);
        }

        public Idle_LazyEvent() : base()
        {

        }
        public override void Enter(IState previousIState, UnitBase parent)
        {
            base.Enter(previousIState, parent);
            _blocked = _isBlock;
            Timer.Start(nameof(_eventTime));
        }

        public override void Stay(UnitBase parent, float deltaTime)
        {
            base.Stay(parent, deltaTime);
            if (Timer.IsFinished(nameof(_eventTime)))
            {
                Debug.Log("Invoked Lazy Event");
                _blocked = false;
                OnCompleted?.Invoke();
            }
        }

        public override bool AllowChange(IState nextState, UnitBase parent)
        {
            if (_blocked) return false;
            return base.AllowChange(nextState, parent);
        }

        public void SetTime(float time)
        {
            _eventTime = time;
        }

        public void SetBlock(bool isBlock)
        {
            _isBlock = isBlock;
            _blocked = isBlock;
        }
    }
}