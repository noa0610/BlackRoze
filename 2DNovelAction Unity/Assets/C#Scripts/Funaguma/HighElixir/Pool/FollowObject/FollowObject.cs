using UnityEngine;

namespace HighElixir.FollowObject
{
    public abstract class FollowObject : MonoBehaviour
    {
        protected Transform targetTransform;
        protected Vector3 targetPosition;
        protected Vector3 offset;
        protected bool UseTargetPosition => targetTransform != null;
        public bool Enable { get; set; } = true;
        public void SetTarget(Transform target, Vector3 offset = default)
        {
            targetTransform = target;
            targetPosition = offset;
        }
        public void SetTarget(Vector3 target)
        {
            targetPosition = target;
        }

        protected abstract void Update();
    }
}