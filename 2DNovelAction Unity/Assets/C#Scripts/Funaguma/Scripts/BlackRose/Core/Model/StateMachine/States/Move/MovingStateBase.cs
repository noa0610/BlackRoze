using BlackRose.Core.Models.Units;
using UnityEngine;

namespace BlackRose.Core.Models.States
{
    public class MovingStateBase : StateWithAnime
    {
        protected virtual Vector2 GetDirection(UnitBase unit) => unit.Direction.normalized;
    }
}