using BlackRose.Core.Models.Units;
using System;

namespace BlackRose.Core.Models.States
{

    /// <summary>
    /// 状態（ステート）ごとのインターフェース。
    /// </summary>
    public interface IState
    {
        void Enter(IState previousIState, UnitBase parent);

        void Stay(UnitBase parent);

        void Exit(IState nextIState, UnitBase parent);

        /// <summary>
        /// Trueの場合IStateMachineは動作を続行する
        /// </summary>
        bool CatchError(Exception exception);

        /// <summary>
        /// falseの場合IStateMachineは遷移をあきらめる
        /// </summary>
        bool AllowChange(IState nextState, UnitBase parent);
    }

    public class StateComp : IState
    {
        public StateComp() { }
        public virtual bool AllowChange(IState nextState, UnitBase parent)
        {
            return true;
        }

        public virtual bool CatchError(Exception exception)
        {
            return true;
        }

        public virtual void Enter(IState previousIState, UnitBase parent)
        {
        }

        public virtual void Exit(IState nextIState, UnitBase parent)
        {
        }

        public virtual void Stay(UnitBase parent)
        {
        }
    }
}
