using UnityEngine;

namespace Test
{
    // =======================
    // IdleÅië“ã@ÅjèÛë‘
    // =======================
    public class Idle : IState
    {
        private string _animationKey;
        private Animation _animation = null;
        public Idle(string animationKey = null, Animation animation = null)
        {
            _animationKey = animationKey;
            _animation = animation;
        }
        public bool Enter(IState previousState, IUnit parent)
        {
            if (_animation != null && _animationKey != null)
            {
                _animation.Play();
            }
            return true;
        }

        public bool Exit(IState nextState, IUnit parent)
        {
            return true;
        }

        public bool Stay(IUnit parent)
        {
            return true;
        }
    }
}
