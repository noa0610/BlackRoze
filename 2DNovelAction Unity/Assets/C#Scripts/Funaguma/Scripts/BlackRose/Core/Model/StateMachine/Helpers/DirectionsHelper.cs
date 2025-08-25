using BlackRose.Core.Models.Units;
using UnityEngine;

namespace BlackRose.Core.Models.States.Helpers
{
    public static class DirectionsHelper
    {
        public static Vector2 X (this UnitBase unit)
        {
            return new Vector2(unit.Direction.x, 0).normalized;
        }
        public static Vector2 Y (this UnitBase unit)
        {
            return new Vector2(0, unit.Direction.y).normalized;
        }
    }
}