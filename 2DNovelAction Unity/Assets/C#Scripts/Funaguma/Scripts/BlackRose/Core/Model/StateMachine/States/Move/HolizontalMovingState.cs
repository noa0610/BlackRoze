using BlackRose.Core.Models.Units;
using UnityEngine;

namespace BlackRose.Core.Models.States
{
    public class HolizontalMovingStates : MovingStateBase
    {
        protected override Vector2 GetDirection(UnitBase unit) => unit.Direction * new Vector2(1, 0);
    }
}