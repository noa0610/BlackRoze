using BlackRose.Core.Models.Units;
using HighElixir;
using HighElixir.Timers;
using System;
using UnityEngine;

namespace BlackRose.Core.Models.States
{
    public class StateComp : IState
    {
        // ステート切り替えを拒否する待機フレーム数
        private Timer _timeHolders;
        protected int _waitFrame = 0;
        protected TimerTicket _tickTicket;
        protected Timer Timer => _timeHolders;

        public event Action WaitTickHasCompleted;
        public StateComp(string parentName = "")
        {
            _timeHolders = new(parentName);
            if (_waitFrame > 0)
                _tickTicket = _timeHolders.CountDownRegister(_waitFrame, "待機フレーム", WaitTickHasCompleted, true);
        }
        public virtual bool AllowChange(IState nextState, UnitBase parent)
        {
            return !_timeHolders.Contains(_tickTicket) || _timeHolders.IsFinished(_tickTicket);
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
                _timeHolders.Start(_tickTicket);
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

        public T SetWaitTick<T>(int frame) where T : StateComp
        {
            _waitFrame = frame;
            if (!_timeHolders.Contains(_tickTicket))
                _timeHolders.CountDownRegister(_waitFrame, "待機フレーム", WaitTickHasCompleted, true);
            _timeHolders.ChangeDuration(_tickTicket, _waitFrame);
            return this as T;
        }
    }
}