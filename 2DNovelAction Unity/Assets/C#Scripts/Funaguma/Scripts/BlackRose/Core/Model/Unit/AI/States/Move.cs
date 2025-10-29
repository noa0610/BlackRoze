using System;

namespace BlackRose.Core.Models.Units.State
{
    // =======================
    // Move（移動）状態
    // =======================
    [Serializable]
    public class MoveOnGround : AccelMoveBase
    {
        protected override Status Status => Status.Speed;
        public MoveOnGround(bool isStopInExit = false)
            : base(isStopInExit)
        {
        }
    }

    [Serializable]
    public class MoveOnAir : AccelMoveBase
    {
        protected override Status Status => Status.SpeedInAir;
        public MoveOnAir(bool isStopInExit = false)
            : base(isStopInExit)
        {
        }
    }
    [Serializable]
    public class DashOnGround : AccelMoveBase
    {
        protected override Status Status => Status.DashSpeed;

        // Exit→着地までの購読を保持しておく（破棄時に保険で解除）
        private Action _onLandingHandler;

        public DashOnGround(bool isStopInExit = false)
            : base(isStopInExit)
        {
        }

    }
}