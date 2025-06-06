using HighElixir.Pool;
using UnityEngine;

namespace HighElixir.FollowObject
{
	public class FollowObjectPool : MonoBehaviour
	{
		[SerializeField] private FollowObject _objectPrefab;
		private GameObject _container;
		private Pool<FollowObject> _objectPool;
		public virtual FollowObject CreateObject(Vector2 targetPosition)
		{
			var effect = _objectPool.Get();
			effect.SetTarget(targetPosition);
			return effect;
		}
        public virtual FollowObject CreateObject(Transform target, Vector2 offset = default)
		{
			var effect = _objectPool.Get();
			effect.SetTarget(target, offset);
			return effect;
		}
		public virtual void ReleaseEffect(FollowObject effect)
		{
			if (effect == null) return;
			effect.Enable = false; // Disable the effect before releasing it
			effect.SetTarget(null); // Clear the target to avoid lingering references
			effect.SetTarget(Vector2.zero); // Reset position to avoid visual glitches
            _objectPool.Release(effect);
		}
		//=== Unity Lifecycle ===


		private void Awake()
        {
			_container = new GameObject("EffectContainer");
            _objectPool = new Pool<FollowObject>(_objectPrefab, 10, _container.transform);
		}
    }
}