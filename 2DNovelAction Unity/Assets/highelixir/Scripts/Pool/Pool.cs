using System;
using System.Collections.Generic;
using UnityEngine;

namespace HighElixir.Pool
{
    public class Pool<T> where T : UnityEngine.Object
    {
        private readonly T _original;
        private readonly int _maxPoolSize;
        private readonly Stack<T> _available = new();
        private readonly HashSet<T> _inUse = new();

        private readonly Transform _container;

        public Transform Container => _container;
        public List<T> InUse => new List<T>(_inUse);

        public Pool(T original, int maxPoolSize, Transform container = null)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            if (maxPoolSize <= 0) throw new ArgumentOutOfRangeException(nameof(maxPoolSize));

            _original = original;
            _maxPoolSize = maxPoolSize;
            _container = container;

            for (int i = 0; i < maxPoolSize; i++)
            {
                var obj = CreateInstance();
                SetActive(obj, false);
                _available.Push(obj);
            }
        }

        public T Get()
        {
            T obj = _available.Count > 0
                ? _available.Pop()
                : CreateInstance();

            if (!_inUse.Add(obj))
                Debug.LogWarning($"{obj.name} はすでに使用中です", obj);

            SetActive(obj, true);
            return obj;
        }

        public void Release(T obj)
        {
            if (obj == null) return;
            if (_available.Contains(obj)) return;

            if (_inUse.Remove(obj))
            {
                SetActive(obj, false);
                SetParent(obj, _container);
                if (_available.Count < _maxPoolSize)
                    _available.Push(obj);
                else
                    UnityEngine.Object.Destroy(obj);
            }
            else
            {
                Debug.LogWarning($"{obj.name} はプール外のオブジェクトです", obj);
            }
        }

        public void Dispose()
        {
            foreach (var obj in _available)
                UnityEngine.Object.Destroy(obj);
            foreach (var obj in _inUse)
                UnityEngine.Object.Destroy(obj);
        }

        // 👅 GameObject / Component 両対応の生成処理
        private T CreateInstance()
        {
            if (_original is GameObject go)
            {
                var instance = UnityEngine.Object.Instantiate(go, _container);
                return instance as T;
            }
            else if (_original is Component comp)
            {
                var instance = UnityEngine.Object.Instantiate(comp, _container);
                return instance as T;
            }

            throw new InvalidOperationException($"PoolはGameObjectまたはComponentにしか対応していません: {typeof(T)}");
        }

        private void SetActive(T obj, bool active)
        {
            if (obj is GameObject go)
                go.SetActive(active);
            else if (obj is Component comp)
                comp.gameObject.SetActive(active);
        }

        private void SetParent(T obj, Transform parent)
        {
            if (obj is GameObject go)
                go.transform.SetParent(parent, false);
            else if (obj is Component comp)
                comp.transform.SetParent(parent, false);
        }
    }
}
