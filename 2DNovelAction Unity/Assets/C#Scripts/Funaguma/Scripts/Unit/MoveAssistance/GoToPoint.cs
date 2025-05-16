using System;
using UnityEngine;

namespace BlackRose
{
    public class GotoPoint : IMoveAssist
    {
        private readonly Transform _parent;
        private Vector2 _targetPosition;
        private float _speed;

        public Action ArrivedCallback { get; set; }


        public GotoPoint(Transform parent, Vector2 target, float speed)
        {
            _parent = parent;
            _targetPosition = target;
            _speed = speed;
        }

        public void Go(float time)
        {
            Vector2 curr = _parent.position;
            if ((curr - _targetPosition).sqrMagnitude < 0.0001f)
            {
                ArrivedCallback?.Invoke();
            }
            Vector2 smoothedPos = Vector2.MoveTowards(curr, _targetPosition, _speed * time);
            _parent.position = smoothedPos;
        }
        public void SetTarget(Transform target)
        {
            _targetPosition = target.position;
        }
        public void SetTarget(Vector2 target)
        {
            _targetPosition = target;
        }
    }
}