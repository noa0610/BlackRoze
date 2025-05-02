using UnityEngine;

namespace Test
{
    // =======================
    // Move（移動）状態
    // =======================
    public class MoveHorizontal : IState
    {
        private Rigidbody2D _rigidbody2D;

        public MoveHorizontal(Rigidbody2D rigidbody2D)
        {
            _rigidbody2D = rigidbody2D;
        }

        public bool Enter(IState previousState, IUnit parent)
        {
            // Move状態に入ったときの処理（今は無し）
            return true;
        }

        public bool Exit(IState nextState, IUnit parent)
        {
            // Move状態を抜けるときの処理（今は無し）
            return true;
        }

        public bool Stay(IUnit parent)
        {
            var s = parent.UnitStatus;
            _rigidbody2D.linearVelocity = new Vector2(s.speed * s.direction.x, _rigidbody2D.linearVelocity.y);
            return true;
        }
    }
}