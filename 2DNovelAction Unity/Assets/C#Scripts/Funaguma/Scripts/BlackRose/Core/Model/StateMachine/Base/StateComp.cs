using BlackRose.Core.Models.Units;
using HighElixir;
using HighElixir.Timers;
using System;
using UnityEngine;

namespace BlackRose.Core.Models.States
{
    public class StateComp : IState, ISerializationCallbackReceiver
    {
        // ステート切り替えを拒否する待機フレーム数
        private Timer _timeHolders = new();
        protected int _waitFrame = 0;

        protected Timer Timer => _timeHolders;
        public StateComp()
        {
            _timeHolders = new();
            _timeHolders.CountDownRegister(nameof(_waitFrame), _waitFrame, type: CountType.Tick);
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
            _timeHolders.Update(deltaTime);
        }

        public T SetWaitTick<T>(int frame, Action onFinished = null) where T : StateComp
        {
            _waitFrame = frame;
            _timeHolders.ChangeDuration(nameof(_waitFrame), _waitFrame);
            return this as T;
        }

        public virtual void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            if (_timeHolders == null)
            {
                _timeHolders = new();
                _timeHolders.CountDownRegister(nameof(_waitFrame), _waitFrame, type: CountType.Tick);
            }
            OnDeserialize();
        }

        public virtual void OnDeserialize()
        {
        }
    }
}