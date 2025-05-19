using UnityEngine;

namespace BlackRose
{
    // =======================
    // IdleÅië“ã@ÅjèÛë‘
    // =======================
    public class Idle_Rotate : IState
    {
        private Transform _transform;
        private float _rotateSpeed;
        public Idle_Rotate(Transform transform, float rotateSpeed)
        {
            _transform = transform;
            _rotateSpeed = rotateSpeed;
        }
        public bool Enter(IState previousState, IUnit parent)
        {
            return true;
        }

        public bool Exit(IState nextState, IUnit parent)
        {
            return true;
        }

        public bool Stay(IUnit parent)
        {
            _transform.Rotate(Vector3.forward, _rotateSpeed * Time.deltaTime);
            return true;
        }
    }
}
