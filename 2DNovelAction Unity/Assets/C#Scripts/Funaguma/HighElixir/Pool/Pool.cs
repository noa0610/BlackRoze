using System;
using System.Collections.Generic;
using UnityEngine;

namespace HighElixir.Pool
{
    public class Pool<T> where T : MonoBehaviour
    {
        private readonly T _original;
        private readonly int _maxPoolSize;
        private readonly Stack<T> _available = new();
        private readonly Transform _container;
        private readonly HashSet<T> _inUse = new();

        public Transform Container => _container;
        public Pool(T original, int maxPoolSize, Transform customContainer, Transform parent = null, bool isRect = false)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            if (maxPoolSize <= 0) throw new ArgumentOutOfRangeException(nameof(maxPoolSize));

            _original = original;
            _maxPoolSize = maxPoolSize;
            if (customContainer != null)
            {
                _container = customContainer;
            }
            else
            {
                var c = new GameObject(typeof(T).Name + "Container" + "_" + Guid.NewGuid().ToString());
                _container = c.transform;
            }

            if (parent != null) _container.transform.SetParent(parent);
            if (isRect && !_container.TryGetComponent<RectTransform>(out var _))
            {
                var r = _container.gameObject.AddComponent<RectTransform>();
                r.anchoredPosition = Vector3.zero;
            }
            for (int i = 0; i < maxPoolSize; i++)
            {
                var obj = GameObject.Instantiate(_original, _container.transform);
                obj.gameObject.SetActive(false);
                _available.Push(obj);
            }
        }

        public T Get()
        {
            T obj = _available.Count > 0
                ? _available.Pop()
                : GameObject.Instantiate(_original, _container.transform);

            if (!_inUse.Add(obj))
                Debug.LogWarning($"{obj.name} がすでにプール中にあります", obj);

            obj.gameObject.SetActive(true);
            return obj;
        }

        public void Release(T obj)
        {
            if (obj == null) return;

            if (_inUse.Remove(obj))
            {
                obj.gameObject.SetActive(false);
                if (_available.Count < _maxPoolSize)
                    _available.Push(obj);
                else
                    GameObject.Destroy(obj.gameObject);
            }
            else
            {
                Debug.LogWarning($"{obj.name} はプール外のオブジェクトです", obj);
            }
        }
    }

}
// unicode