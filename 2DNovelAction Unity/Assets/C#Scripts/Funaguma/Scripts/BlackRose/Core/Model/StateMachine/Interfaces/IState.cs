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
}
