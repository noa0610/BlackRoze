using System;
using System.Collections.Generic;

namespace BlackRose
{

    /// <summary>
    /// 状態（ステート）ごとのインターフェース。
    /// 各ステート（行動パターン）にこのインターフェースを実装させる。
    /// 返り値のboolは処理の成功／失敗を示す。
    /// </summary>
    // Note : アニメーターと連携する場合は、コンストラクタにAnimatorとEnter時に起動すべきトリガーを渡すなどして、連携すること
    public interface IState
    {
        void Enter(IState previousIState, IUnit parent);

        void Stay(IUnit parent);

        void Exit(IState nextIState, IUnit parent);

        /// <summary>
        /// Trueの場合IStateMachineは動作を続行する
        /// </summary>
        bool CatchError(Exception exception);

        /// <summary>
        /// falseの場合IStateMachineは遷移をあきらめる
        /// </summary>
        bool AllowChange(IState nextState, IUnit parent);
    }

    public class StateComp : IState
    {
        public StateComp() { }
        public virtual bool AllowChange(IState nextState, IUnit parent)
        {
            return true;
        }

        public virtual bool CatchError(Exception exception)
        {
            return true;
        }

        public virtual void Enter(IState previousIState, IUnit parent)
        {
        }

        public virtual void Exit(IState nextIState, IUnit parent)
        {
        }

        public virtual void Stay(IUnit parent)
        {
        }
    }
}
