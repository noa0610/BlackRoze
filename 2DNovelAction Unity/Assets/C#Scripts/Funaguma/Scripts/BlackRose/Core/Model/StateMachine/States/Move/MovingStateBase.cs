using BlackRose.Core.Models.Units;
using UnityEngine;

namespace BlackRose.Core.Models.States
{
    public class MovingStateBase : StateComp, IRigidbodyUser
    {
        public Rigidbody2D Rigidbody2D { get; protected set; }

        public void SetRB2(Rigidbody2D rb)
        {
            Rigidbody2D = rb;
        }

        protected virtual Vector2 GetDirection(UnitBase unit) => unit.Direction.normalized;
    }
}