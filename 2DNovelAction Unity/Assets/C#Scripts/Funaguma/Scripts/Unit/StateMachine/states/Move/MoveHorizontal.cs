using UnityEngine;

namespace Test
{
    // =======================
    // Move（移動）状態
    // =======================
    public class MoveHorizontal : IState
    {
        private Rigidbody2D _rigidbody2D;
        private string _animationTrigger;

        public MoveHorizontal(Rigidbody2D rigidbody2D, string animationTrigger)
        {
            _rigidbody2D = rigidbody2D;
            _animationTrigger = animationTrigger;
        }

        public bool Enter(IState previousState, IUnit parent)
        {
            parent.Animator.SetTrigger(_animationTrigger);
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
            _rigidbody2D.velocity = new Vector2(s.speed * s.direction.x, _rigidbody2D.velocity.y);
            return true;
        }
    }
}