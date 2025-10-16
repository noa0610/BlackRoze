using BlackRose.Core.Models.Units;
using System;

namespace BlackRose.Core.Models.States
{
    // =======================
    // Move（移動）状態
    // =======================
    [Serializable]
    public class MoveOnAir : AccelMoveBase
    {
        public override float GetAccel(UnitBase parent)
        {
            if (parent.StatusManager.TryGetCurrentValue(Status.SpeedInAir, out var c))
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