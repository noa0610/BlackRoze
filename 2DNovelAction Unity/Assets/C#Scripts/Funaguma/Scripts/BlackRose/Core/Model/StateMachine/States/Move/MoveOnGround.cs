using BlackRose.Core.Models.Units;
using System;
using UnityEngine;

namespace BlackRose.Core.Models.States
{
    // =======================
    // Move（移動）状態
    // =======================
    [Serializable]
    public class MoveOnGround : AccelMoveBase
    {
        public MoveOnGround(bool isStopInExit = false)
            : base(isStopInExit)
        {
        }

        [Obsolete]
        public MoveOnGround(Rigidbody2D rigidbody2D, bool isStopInExit = false)
            : base(isStopInExit)
        {
        }

        public override float GetAccel(UnitBase parent)
        {
            if (parent.StatusManager.TryGetCurrentValue(Status.Speed, out var c))
            {
                return c;
            }
            else
            {
                return base.GetAccel(parent);
            }
        }
    }
}