using BlackRose.Core.Models.Units;
using HighElixir;
using System;
using UnityEngine;

namespace BlackRose.Core.Models.States
{

    /// <summary>
    /// 状態（ステート）ごとのインターフェース。
    /// </summary>
    public interface IState
    {
        void Enter(IState previousState, UnitBase parent);

        void Stay(UnitBase parent, float deltaTime);

        void Exit(IState nextState, UnitBase parent);

        /// <summary>
        /// Trueの場合IStateMachineは動作を続行する
        /// </summary>
        bool CatchError(Exception exception);

        /// <summary>
        /// falseの場合IStateMachineは遷移をあきらめる
        /// </summary>
        bool AllowChange(IState nextState, UnitBase parent);

        bool AllowEnter(IState previousState, UnitBase parent);
    }

    public class StateComp : IState, ISerializationCallbackReceiver
    {
        // ステート切り替えを拒否する待機フレーム数
        protected TimeHolders _timeHolders = new TimeHolders();
        protected int _waitFrame = 0;
        public StateComp()
        {
            _timeHolders = new();
            _timeHolders.Register(nameof(_waitFrame), _waitFrame, TimeHolders.CountType.Tick);
        }
        public virtual bool AllowChange(IState nextState, UnitBase parent)
        {
            return _timeHolders.IsFinished(nameof(_waitFrame));
        }

        public virtual bool AllowEnter(IState previousState, UnitBase parent)
        {
            return true;
        }
        public virtual bool CatchError(Exception exception)
        {
            return true;
        }

        public virtual void Enter(IState previousIState, UnitBase parent)
        {
            if (_waitFrame > 0)
                _timeHolders.Start(nameof(_waitFrame));
        }

        public virtual void Exit(IState nextIState, UnitBase parent)
        {
        }

        public virtual void Stay(UnitBase parent, float deltaTime)
        {
            if (_timeHolders == null)
            {
                Debug.LogWarning("_timeHolders is null");
                return;
            }
            if (!_timeHolders.IsFinished(nameof(_waitFrame)))
                _timeHolders.Update(deltaTime);
        }

        public T SetWaitTick<T>(int frame, Action onFinished = null) where T : StateComp
        {
            _waitFrame = frame;
            _timeHolders.ChangeDuration(nameof(_waitFrame), _waitFrame);
            return this as T;
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            if (_timeHolders == null)
            {
                _timeHolders = new();
                _timeHolders.Register(nameof(_waitFrame), _waitFrame, TimeHolders.CountType.Tick);
            }
        }
    }
}
