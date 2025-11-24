using System;

namespace BlackRose.Core.Models.Units.State
{
    // =======================
    // Move（移動）状態
    // =======================
    [Serializable]
    public class MoveOnGround<T> : AccelMoveBase<T>
        where T : UnitBase
    {
        protected override Status Status => Status.Speed;
        public MoveOnGround(bool isStopInExit = false)
            : base(isStopInExit)
        {
        }
    }

    [Serializable]
    public class MoveOnAir<T> : AccelMoveBase<T>
        where T : UnitBase
    {
        protected override Status Status => Status.SpeedInAir;
        public MoveOnAir(bool isStopInExit = false)
            : base(isStopInExit)
        {
        }
    }
    [Serializable]
    public class DashOnGround<T> : AccelMoveBase<T>
        where T : UnitBase
    {
        protected override Status Status => Status.DashSpeed;

        public DashOnGround(bool isStopInExit = false)
            : base(isStopInExit)
        {
        }

    }
}