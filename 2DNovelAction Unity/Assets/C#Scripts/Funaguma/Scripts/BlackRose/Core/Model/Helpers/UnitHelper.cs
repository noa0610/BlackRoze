using BlackRose.Core.Models.Units;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BlackRose.Core.Models.Helper
{
    public static class UnitHelper
    {
        public static UnitBase GetUnitNearest(this List<UnitBase> units, Vector3 position)
        {
            if (units == null || units.Count == 0)
                return null;

            return units.Aggregate((nearest, next) =>
                (next.transform.position - position).sqrMagnitude <
                (nearest.transform.position - position).sqrMagnitude ? next : nearest);
        }

    }
}